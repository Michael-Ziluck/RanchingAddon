using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using RanchingAddon;
using UnityEngine;

internal static class Program
{
    private static int checks;

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }

    private static T Attach<T>(GameObject creature, T component) where T : MonoBehaviour
    {
        component.gameObject = creature;
        creature.Components[typeof(T)] = component;
        return component;
    }

    private static void Main()
    {
        Chainloader.PluginInfos["org.bepinex.plugins.ranching"] = new PluginInfo();
        var plugin = new Plugin();
        typeof(Plugin).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(plugin, null);
        var player = new Player();
        var playerView = Attach(player.gameObject, new ZNetView());
        playerView.Record.Floats["Ranching Skill"] = 1;
        Player.m_localPlayer = player;
        Player.Players.Add(player);

        foreach (var species in new[] { ("Chicken", "Hen", "Ranching Chicken Growth Bonus"), ("Asksvin_hatchling", "Asksvin", "Ranching Asksvin Growth Bonus") })
        {
            var creature = new GameObject { name = species.Item1 };
            var growup = Attach(creature, new Growup { m_grownPrefab = new GameObject { name = species.Item2 } });
            var character = Attach(creature, new Character());
            var view = Attach(creature, new ZNetView());
            Attach(creature, new BaseAI());
            Time.timeAsDouble = 100;
            growup.GrowUpdate();
            Check(view.Record.Writes == 0, species.Item1 + " first observation earns no bonus");
            Time.timeAsDouble = 110;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, species.Item1 + " earns skill-scaled bonus while owned");
            view.Owner = false;
            Time.timeAsDouble = 120;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, "non-owner does not award bonus");
            view.Owner = true;
            Time.timeAsDouble = 130;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, "ownership reacquisition starts a new interval");
            view.Record.Owner = 8;
            Time.timeAsDouble = 140;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, "changed owner identity starts a new interval");
            player.transform.position = new Vector3(11, 0, 0);
            Time.timeAsDouble = 150;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, "distant player earns no bonus");
            player.transform.position = new Vector3(0, 0, 0);
            playerView.Valid = false;
            Time.timeAsDouble = 160;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 10, "player without valid synchronized state earns no bonus");
            playerView.Valid = true;
            playerView.Record.Floats["Ranching Skill"] = .5f;
            Time.timeAsDouble = 170;
            growup.GrowUpdate();
            Check(view.Record.GetFloat(species.Item3) == 15, "nearest player skill scales bonus");
            playerView.Record.Floats["Ranching Skill"] = 1;
            view.Record.Longs[ZDOVars.s_spawnTime] = ZNet.instance.Now.AddSeconds(-600).Ticks;
            int writes = view.Record.Writes;
            Check(character.GetHoverText() == species.Item1 + "\nGrowth: 20%", "empty juvenile hover gains name and growth");
            Check(view.Record.Writes == writes, "juvenile hover does not write network data");
            character.Hover = "Existing hover";
            Check(character.GetHoverText() == "Existing hover\nGrowth: 20%", "existing hover text is preserved");
            playerView.Record.Floats["Ranching Skill"] = .29f;
            Check(character.GetHoverText() == "Existing hover", "hover uses viewer skill threshold");
            playerView.Record.Floats["Ranching Skill"] = 1;
            view.Record.Floats[species.Item3] = float.NaN;
            Check(character.GetHoverText() == "Existing hover\nGrowth: 20%", "invalid saved bonus does not corrupt hover percentage");
            view.Record.Longs.Remove(ZDOVars.s_spawnTime);
            Check(character.GetHoverText() == "Existing hover" && view.Record.Writes == writes, "missing spawn timestamp is not initialized by hovering");
            view.Record.Floats[species.Item3] = 2495;
            Plugin.ChickenGrowthFactor.Value = 1;
            Plugin.AsksvinGrowthFactor.Value = 1;
            Time.timeAsDouble = 180;
            growup.GrowUpdate();
            Check(!growup.Grown && view.Record.GetFloat(species.Item3) == 2495, "factor one preserves earned age without adding bonus");
            view.Record.Floats[species.Item3] = 2500;
            growup.GrowUpdate();
            Check(growup.Grown, "earned bonus contributes to actual maturation check");
            Plugin.ChickenGrowthFactor.Value = 2;
            Plugin.AsksvinGrowthFactor.Value = 2;
        }

        foreach (var species in new[] { ("ChickenEgg", "Chicken"), ("AsksvinEgg", "Asksvin_hatchling") })
        {
            var shell = new GameObject { name = species.Item1 };
            var egg = Attach(shell, new EggGrow { m_grownPrefab = new GameObject { name = species.Item2 } });
            var drop = Attach(shell, new ItemDrop());
            var view = Attach(shell, new ZNetView());
            view.Record.Timers[ZDOVars.s_growStart] = 100;
            Check(egg.GetHoverText() == "Warm\nIncubation: 50% (15:00 remaining)", "egg hover preserves vanilla status and adds progress");
            Check(view.Record.Writes == 0, "egg hover does not write network data");
            drop.m_itemData.m_stack = 2;
            Check(egg.GetHoverText() == "Warm\nIncubation: separate stacked eggs to hatch", "stacked eggs show separation advice");
            drop.m_itemData.m_stack = 1;
            view.Record.Timers.Clear();
            Check(egg.GetHoverText() == "Warm\nIncubation: not incubating", "cold egg uses vanilla timer state");
            Plugin.ChickenInfoLevel.Value = 0;
            Plugin.AsksvinInfoLevel.Value = 0;
            Check(egg.GetHoverText() == "Warm", "zero info level hides egg hover");
            Plugin.ChickenInfoLevel.Value = 30;
            Plugin.AsksvinInfoLevel.Value = 30;
        }
        typeof(Plugin).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(plugin, null);
        Console.WriteLine($"PASS: {checks} growth and hover hook checks against simulated game objects. Unity and ServerSync networking are not exercised.");
    }
}
