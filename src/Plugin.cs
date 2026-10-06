using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace RanchingAddon;

[BepInPlugin(Guid, "RanchingAddon", Version)]
[BepInDependency(RanchingGuid, "1.1.9")]
public sealed class Plugin : BaseUnityPlugin
{
    // Retain the plugin identity and config path for existing installations.
    // RanchingAddon replaces RanchingChickAddon; install only one package.
    public const string Guid = "com.ziluck.valheim.ranchingchickaddon";
    public const string Version = "3.0.1";
    private const string RanchingGuid = "org.bepinex.plugins.ranching";
    private readonly ConfigSync sync = new(Guid) { DisplayName = "RanchingAddon", CurrentVersion = Version, MinimumRequiredVersion = Version };
    internal static ConfigEntry<float> ChickenGrowthFactor = null!;
    internal static ConfigEntry<float> AsksvinGrowthFactor = null!;
    internal static ConfigEntry<int> ChickenInfoLevel = null!;
    internal static ConfigEntry<int> AsksvinInfoLevel = null!;
    private Harmony? harmony;

    private void Awake()
    {
        // The earlier experimental fork already contains chicken acceleration.
        if (Chainloader.PluginInfos[RanchingGuid].Instance.GetType().Assembly.GetType("Ranching.ChickenGrowth") != null)
        {
            Logger.LogError("RanchingAddon disabled: replace the experimental Ranching fork with original Smoothbrain-Ranching before using this addon.");
            return;
        }
        var locked = Config.Bind("General", "Lock Configuration", true, "Only server admins can change synchronized settings when the addon is installed on the host/server.");
        sync.AddLockingConfigEntry(locked);
        // Keep the original section and keys so chick settings survive upgrades.
        ChickenGrowthFactor = BindGrowthFactor("Chicks", "Chicken Growth Factor");
        ChickenInfoLevel = BindInfoLevel("Chicks");
        AsksvinGrowthFactor = BindGrowthFactor("Asksvin", "Asksvin Growth Factor");
        AsksvinInfoLevel = BindInfoLevel("Asksvin");
        try
        {
            harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo("RanchingAddon ready: chick and Asksvin hatchling growth, plus egg incubation information enabled. Taming, drops, XP, and breeding remain supplied by original Ranching.");
        }
        catch (Exception error)
        {
            harmony?.UnpatchSelf();
            Logger.LogError($"RanchingAddon could not apply its patches and is disabled: {error}");
        }
    }

    private ConfigEntry<float> BindGrowthFactor(string section, string name)
    {
        var entry = Config.Bind(section, name, 2f, new ConfigDescription("Juvenile growth speed at Ranching level 100 with a player within 10 metres. Scales with the nearest player's skill. 1 disables additional growth; earned bonus remains. Egg incubation is unaffected.", new AcceptableValueRange<float>(1f, 10f)));
        sync.AddConfigEntry(entry);
        return entry;
    }

    private ConfigEntry<int> BindInfoLevel(string section)
    {
        var entry = Config.Bind(section, "Growth Info Level Requirement", 30, new ConfigDescription("Viewer's minimum Ranching level to see juvenile growth percentage and egg incubation progress. 0 disables both displays for this species.", new AcceptableValueRange<int>(0, 100)));
        sync.AddConfigEntry(entry);
        return entry;
    }

    internal static float GetSkill(Player player)
    {
        ZNetView view = player.GetComponent<ZNetView>();
        return view && view.IsValid() ? view.GetZDO().GetFloat("Ranching Skill") : 0f;
    }

    private void OnDestroy() => harmony?.UnpatchSelf();
}
