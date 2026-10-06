using HarmonyLib;

namespace RanchingAddon;

[HarmonyPatch(typeof(EggGrow), nameof(EggGrow.GetHoverText))]
internal static class EggHover
{
    private static void Postfix(EggGrow __instance, ref string __result)
    {
        if (!__instance || !__instance.m_grownPrefab) return;
        GrowthSpecies species = GrowthPolicy.ClassifyEgg(Utils.GetPrefabName(__instance.gameObject), __instance.m_grownPrefab.name);
        Player player = Player.m_localPlayer;
        if (species == GrowthSpecies.None || !player || !GrowthPolicy.ShowInfo(Plugin.GetSkill(player), CreatureGrowth.InfoLevel(species))) return;
        ZNetView view = __instance.GetComponent<ZNetView>();
        ItemDrop egg = __instance.GetComponent<ItemDrop>();
        if (!view || !view.IsValid() || !egg || egg.m_itemData?.m_shared == null || !ZNet.instance) return;

        // Read the vanilla timer without calling CanGrow/Load or setting any
        // network fields. Remote viewers must not restart or advance incubation.
        float started = view.GetZDO().GetFloat(ZDOVars.s_growStart);
        string incubationText = EggInfo.Describe(egg.m_itemData.m_stack, started, ZNet.instance.GetTimeSeconds(), __instance.m_growTime);
        __result = string.IsNullOrEmpty(__result) ? $"{egg.GetHoverName()}\n{incubationText}" : $"{__result}\n{incubationText}";
    }
}
