using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

using UnityEngine;

namespace RanchingChickAddon;

internal static class ChickenGrowth
{
	private const string BonusKey = "Ranching Chicken Growth Bonus";
	private static readonly ConditionalWeakTable<Growup, GrowthClock> Clocks = new();

	private static bool IsChick(Growup growup) => growup && growup.m_grownPrefab && growup.m_grownPrefab.name == "Hen";

	// Only the network owner awards bonus age. First observation/ownership change
	// starts a new interval; unloaded time must never earn a proximity bonus.
	[HarmonyPatch(typeof(Growup), "GrowUpdate")]
	private static class GrowUpdate
	{
		private static void Prefix(Growup __instance)
		{
			if (!IsChick(__instance)) return;
			ZNetView view = __instance.GetComponent<ZNetView>();
			GrowthClock clock = Clocks.GetOrCreateValue(__instance);
			bool owner = view && view.IsValid() && view.IsOwner();
			long ownerId = owner ? view!.GetZDO().GetOwner() : 0;
			double seconds = clock.Sample(Time.timeAsDouble, owner, ownerId);
			if (!owner || seconds <= 0 || Plugin.GrowthFactor.Value <= 1f) return;
			Player closest = Player.GetClosestPlayer(__instance.transform.position, 10f);
			if (!closest || !closest.GetComponent<ZNetView>() || !closest.GetComponent<ZNetView>().IsValid()) return;
			float skill = closest.GetComponent<ZNetView>().GetZDO().GetFloat("Ranching Skill");
			float extra = (float)GrowthMath.BonusSeconds(seconds, skill, Plugin.GrowthFactor.Value);
			if (extra <= 0) return;
			ZDO zdo = view!.GetZDO();
			zdo.Set(BonusKey, Math.Min(__instance.m_growTime, ReadBonus(view) + extra));
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var original = AccessTools.Method(typeof(BaseAI), nameof(BaseAI.GetTimeSinceSpawned));
			var replacement = AccessTools.Method(typeof(ChickenGrowth), nameof(EffectiveAge));
			List<CodeInstruction> code = instructions.ToList();
			if (code.Count(instruction => instruction.Calls(original)) != 1)
				throw new InvalidOperationException("Ranching: unsupported Growup.GrowUpdate; expected one age check.");
			foreach (CodeInstruction instruction in code)
			{
				if (instruction.Calls(original))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = replacement;
				}
				yield return instruction;
			}
		}
	}

	private static float ReadBonus(ZNetView view)
	{
		float bonus = view && view.IsValid() ? view.GetZDO().GetFloat(BonusKey) : 0f;
		return float.IsNaN(bonus) || float.IsInfinity(bonus) ? 0f : Math.Max(0f, bonus);
	}

	private static TimeSpan EffectiveAge(BaseAI ai)
	{
		TimeSpan age = ai.GetTimeSinceSpawned();
		return IsChick(ai.GetComponent<Growup>()) ? age.Add(TimeSpan.FromSeconds(ReadBonus(ai.GetComponent<ZNetView>()))) : age;
	}

	[HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
	private static class GrowthHover
	{
		private static void Postfix(Character __instance, ref string __result)
		{
			int level = Plugin.InfoLevel.Value;
			Player player = Player.m_localPlayer;
			if (level <= 0 || !player || Plugin.GetSkill(player) < level / 100f) return;
			Growup growup = __instance.GetComponent<Growup>();
			if (!IsChick(growup)) return;
			ZNetView view = __instance.GetComponent<ZNetView>();
			BaseAI ai = __instance.GetComponent<BaseAI>();
			if (!view || !view.IsValid() || !ai || growup.m_growTime <= 0) return;
			// Read spawn time without GetTimeSinceSpawned(), which initializes missing
			// timestamps. Hovering on a remote client must not write creature state.
			long spawned = view.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
			if (spawned <= 0 || spawned > DateTime.MaxValue.Ticks || !ZNet.instance) return;
			double age = (ZNet.instance.GetTime() - new DateTime(spawned)).TotalSeconds;
			int percent = GrowthMath.Percent(age, ReadBonus(view), growup.m_growTime);
			string text = $"Growth: {percent}%";
			__result = string.IsNullOrEmpty(__result) ? $"{__instance.GetHoverName()}\n{text}" : $"{__result}\n{text}";
		}
	}
}
