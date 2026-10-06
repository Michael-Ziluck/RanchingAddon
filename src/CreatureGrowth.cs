using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace RanchingAddon;

internal static class CreatureGrowth
{
	// Keep the chicken key so existing chicks retain bonus age after upgrading.
	internal static string BonusKey(GrowthSpecies species) => species == GrowthSpecies.Chicken
		? "Ranching Chicken Growth Bonus" : "Ranching Asksvin Growth Bonus";
	private static readonly ConditionalWeakTable<Growup, GrowthClock> Clocks = new();

	internal static GrowthSpecies Species(Growup growup) => growup && growup.m_grownPrefab
		? GrowthPolicy.Classify(Utils.GetPrefabName(growup.gameObject), growup.m_grownPrefab.name)
		: GrowthSpecies.None;

	private static float Factor(GrowthSpecies species) => species == GrowthSpecies.Chicken
		? Plugin.ChickenGrowthFactor.Value : Plugin.AsksvinGrowthFactor.Value;

	internal static int InfoLevel(GrowthSpecies species) => species == GrowthSpecies.Chicken
		? Plugin.ChickenInfoLevel.Value : Plugin.AsksvinInfoLevel.Value;

	// Only the network owner awards bonus age. First observation/ownership change
	// starts a new interval; unloaded time must never earn a proximity bonus.
	[HarmonyPatch(typeof(Growup), "GrowUpdate")]
	private static class GrowUpdate
	{
		private static void Prefix(Growup __instance)
		{
			GrowthSpecies species = Species(__instance);
			if (species == GrowthSpecies.None) return;
			ZNetView view = __instance.GetComponent<ZNetView>();
			GrowthClock clock = Clocks.GetOrCreateValue(__instance);
			bool owner = view && view.IsValid() && view.IsOwner();
			long ownerId = owner ? view!.GetZDO().GetOwner() : 0;
			double seconds = clock.Sample(Time.timeAsDouble, owner, ownerId);
			if (!owner || seconds <= 0 || Factor(species) <= 1f) return;
			Player closest = Player.GetClosestPlayer(__instance.transform.position, 10f);
			if (!closest) return;
			ZNetView playerView = closest.GetComponent<ZNetView>();
			if (!playerView || !playerView.IsValid()) return;
			float skill = playerView.GetZDO().GetFloat("Ranching Skill");
			float extra = (float)GrowthMath.BonusSeconds(seconds, skill, Factor(species));
			if (extra <= 0) return;
			ZDO zdo = view!.GetZDO();
			zdo.Set(BonusKey(species), Math.Min(__instance.m_growTime, ReadBonus(view, species) + extra));
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var original = AccessTools.Method(typeof(BaseAI), nameof(BaseAI.GetTimeSinceSpawned));
			var replacement = AccessTools.Method(typeof(CreatureGrowth), nameof(EffectiveAge));
			List<CodeInstruction> code = instructions.ToList();
			if (code.Count(instruction => instruction.Calls(original)) != 1)
				throw new InvalidOperationException("RanchingAddon: unsupported Growup.GrowUpdate; expected one age check.");
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

	private static float ReadBonus(ZNetView view, GrowthSpecies species)
	{
		float bonus = view && view.IsValid() ? view.GetZDO().GetFloat(BonusKey(species)) : 0f;
		return float.IsNaN(bonus) || float.IsInfinity(bonus) ? 0f : Math.Max(0f, bonus);
	}

	private static TimeSpan EffectiveAge(BaseAI ai)
	{
		TimeSpan age = ai.GetTimeSinceSpawned();
		GrowthSpecies species = Species(ai.GetComponent<Growup>());
		return species != GrowthSpecies.None ? age.Add(TimeSpan.FromSeconds(ReadBonus(ai.GetComponent<ZNetView>(), species))) : age;
	}

	[HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
	private static class GrowthHover
	{
		private static void Postfix(Character __instance, ref string __result)
		{
			Growup growup = __instance.GetComponent<Growup>();
			GrowthSpecies species = Species(growup);
			if (species == GrowthSpecies.None) return;
			Player player = Player.m_localPlayer;
			if (!player || !GrowthPolicy.ShowInfo(Plugin.GetSkill(player), InfoLevel(species))) return;
			ZNetView view = __instance.GetComponent<ZNetView>();
			BaseAI ai = __instance.GetComponent<BaseAI>();
			if (!view || !view.IsValid() || !ai || growup.m_growTime <= 0) return;
			// Read spawn time without GetTimeSinceSpawned(), which initializes missing
			// timestamps. Hovering on a remote client must not write creature state.
			long spawned = view.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
			if (spawned <= 0 || spawned > DateTime.MaxValue.Ticks || !ZNet.instance) return;
			double age = (ZNet.instance.GetTime() - new DateTime(spawned)).TotalSeconds;
			int percent = GrowthMath.Percent(age, ReadBonus(view, species), growup.m_growTime);
			string text = $"Growth: {percent}%";
			__result = string.IsNullOrEmpty(__result) ? $"{__instance.GetHoverName()}\n{text}" : $"{__result}\n{text}";
		}
	}
}
