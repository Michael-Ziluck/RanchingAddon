using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace RanchingChickAddon;

[BepInPlugin(Guid, "Ranching - Chick Addon", Version)]
[BepInDependency(RanchingGuid, "1.1.6")]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.ziluck.valheim.ranchingchickaddon";
    public const string Version = "2.0.1";
    private const string RanchingGuid = "org.bepinex.plugins.ranching";
    private readonly ConfigSync sync = new(Guid) { DisplayName = "Ranching - Chick Addon", CurrentVersion = Version, MinimumRequiredVersion = Version };
    internal static ConfigEntry<float> GrowthFactor = null!;
    internal static ConfigEntry<int> InfoLevel = null!;
    private Harmony? harmony;

    private void Awake()
    {
        // The earlier experimental fork already contains these effects.
        if (Chainloader.PluginInfos[RanchingGuid].Instance.GetType().Assembly.GetType("Ranching.ChickenGrowth") != null)
        {
            Logger.LogError("Chick Addon disabled: replace the experimental Ranching fork with original Smoothbrain-Ranching before using this addon.");
            return;
        }
        var locked = Config.Bind("General", "Lock Configuration", true, "Only server admins can change synchronized settings when the addon is installed on the host/server.");
        sync.AddLockingConfigEntry(locked);
        GrowthFactor = Config.Bind("Chicks", "Chicken Growth Factor", 2f, new ConfigDescription("Growth speed at Ranching level 100 with a player within 10 metres. Scales with the nearest player's skill. 1 disables additional growth; earned bonus remains. Eggs and other animals are unaffected.", new AcceptableValueRange<float>(1f, 10f)));
        InfoLevel = Config.Bind("Chicks", "Growth Info Level Requirement", 30, new ConfigDescription("Viewer's minimum Ranching level to see chick growth percentage. 0 disables the display.", new AcceptableValueRange<int>(0, 100)));
        sync.AddConfigEntry(GrowthFactor);
        sync.AddConfigEntry(InfoLevel);
        try
        {
            harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo("Ranching - Chick Addon ready; using the original Ranching skill.");
        }
        catch (Exception error)
        {
            harmony?.UnpatchSelf();
            Logger.LogError($"Chick Addon could not apply its patches and is disabled: {error}");
        }
    }

    internal static float GetSkill(Player player)
    {
        ZNetView view = player.GetComponent<ZNetView>();
        return view && view.IsValid() ? view.GetZDO().GetFloat("Ranching Skill") : 0f;
    }

    private void OnDestroy() => harmony?.UnpatchSelf();
}
