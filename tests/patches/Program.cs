using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

internal static class Program
{
    private static int checks;

    private static int Main(string[] args)
    {
        string game = args[0];
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
            string name = new AssemblyName(e.Name).Name + ".dll";
            foreach (string directory in new[] { "valheim_Data/Managed", "BepInEx/core" })
            {
                string path = Path.Combine(game, directory, name);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        try { Run(); return 0; }
        catch (Exception error) { System.Console.Error.WriteLine(error); return 1; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        Assembly addon = typeof(RanchingAddon.Plugin).Assembly;
        Type growth = addon.GetType("RanchingAddon.CreatureGrowth", true)!;
        Type update = growth.GetNestedType("GrowUpdate", BindingFlags.NonPublic)!;
        MethodInfo transpiler = AccessTools.Method(update, "Transpiler");
        MethodInfo age = AccessTools.Method(typeof(BaseAI), nameof(BaseAI.GetTimeSinceSpawned));
        MethodInfo effectiveAge = AccessTools.Method(growth, "EffectiveAge");
        MethodInfo target = AccessTools.Method(typeof(Growup), "GrowUpdate");
        Assert(target != null && target.ReturnType == typeof(void) && target.GetParameters().Length == 0, "growth patch target signature");
        MethodInfo hover = AccessTools.Method(typeof(Character), nameof(Character.GetHoverText));
        Assert(hover != null && hover.ReturnType == typeof(string) && hover.GetParameters().Length == 0, "hover patch target signature");
        MethodInfo eggHover = AccessTools.Method(typeof(EggGrow), nameof(EggGrow.GetHoverText));
        Assert(eggHover != null && eggHover.ReturnType == typeof(string) && eggHover.GetParameters().Length == 0, "egg hover patch target signature");
        Type eggPatch = addon.GetType("RanchingAddon.EggHover", true)!;
        List<CodeInstruction> eggCode = PatchProcessor.GetOriginalInstructions(AccessTools.Method(eggPatch, "Postfix"));
        Assert(eggCode.Any(i => i.operand is MethodInfo m && m.DeclaringType == typeof(ZDO) && m.Name == "GetFloat"), "egg hover reads synchronized incubation timer");
        Assert(!eggCode.Any(i => i.operand is MethodInfo m && (m.Name == "Set" || m.Name == "Load" || m.Name == "CanGrow")), "egg hover does not mutate or restart incubation");

        List<CodeInstruction> original = PatchProcessor.GetOriginalInstructions(target);
        List<CodeInstruction> rewritten = Rewrite(transpiler, original);
        Assert(rewritten.Count == original.Count, "transpiler preserves instruction count");
        Assert(rewritten.Count(i => i.Calls(effectiveAge)) == 1, "real game IL uses effective age exactly once");
        Assert(rewritten.All(i => !i.Calls(age)), "vanilla age call is replaced");
        Assert(rewritten.Count(i => i.operand is MethodInfo m && m.Name == "IsOwner") == 1, "game owner guard remains");
        Assert(rewritten.Count(i => i.operand is MethodInfo m && m.Name == "GetPrefab") == 1, "vanilla adult selection remains");

        var generator = new System.Reflection.Emit.DynamicMethod("growthLabels", typeof(void), Type.EmptyTypes).GetILGenerator();
        var label = generator.DefineLabel();
        var ageInstruction = new CodeInstruction(System.Reflection.Emit.OpCodes.Call, age);
        ageInstruction.labels.Add(label);
        ageInstruction.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock));
        var labelled = Rewrite(transpiler, new[] { ageInstruction }).Single();
        Assert(labelled.labels.Single() == label, "age replacement retains branch labels");
        Assert(labelled.blocks.Single().blockType == ExceptionBlockType.BeginExceptionBlock, "age replacement retains exception boundaries");

        Reject(transpiler, new[] { new CodeInstruction(System.Reflection.Emit.OpCodes.Ret) }, "missing age call rejected");
        Reject(transpiler, new[] { new CodeInstruction(System.Reflection.Emit.OpCodes.Call, age), new CodeInstruction(System.Reflection.Emit.OpCodes.Call, age) }, "ambiguous age calls rejected");

        Type species = addon.GetType("RanchingAddon.GrowthSpecies", true)!;
        MethodInfo key = AccessTools.Method(growth, "BonusKey");
        string chickenKey = (string)key.Invoke(null, new[] { Enum.Parse(species, "Chicken") })!;
        string asksvinKey = (string)key.Invoke(null, new[] { Enum.Parse(species, "Asksvin") })!;
        Assert(chickenKey == "Ranching Chicken Growth Bonus", "legacy saved chick growth key retained");
        Assert(asksvinKey != chickenKey, "Asksvin growth has independent storage");
        Assert(RanchingAddon.Plugin.Guid == "com.ziluck.valheim.ranchingchickaddon", "legacy configuration identity retained");
        System.Console.WriteLine($"PASS: {checks} patch checks against actual game IL; Unity execution and in-game behavior are not tested.");
    }

    private static List<CodeInstruction> Rewrite(MethodInfo method, IEnumerable<CodeInstruction> instructions) =>
        ((IEnumerable<CodeInstruction>)method.Invoke(null, new object[] { instructions })!).ToList();

    private static void Reject(MethodInfo method, IEnumerable<CodeInstruction> instructions, string scenario)
    {
        try { Rewrite(method, instructions); }
        catch (InvalidOperationException) { Assert(true, scenario); return; }
        throw new Exception(scenario);
    }

    private static void Assert(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }
}
