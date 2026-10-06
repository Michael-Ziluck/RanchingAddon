using System;
using RanchingAddon;

int checks = 0;
void Equal(double expected, double actual, string scenario)
{
    if (Math.Abs(expected - actual) > 0.000001 || double.IsNaN(actual))
        throw new Exception($"{scenario}: expected {expected}, got {actual}");
    checks++;
}

Equal(0, GrowthMath.BonusSeconds(10, 0, 2), "level zero earns no bonus");
Equal(5, GrowthMath.BonusSeconds(10, 0.5f, 2), "level fifty gives 1.5x total growth");
Equal(10, GrowthMath.BonusSeconds(10, 1, 2), "level one hundred gives 2x total growth");
Equal(0, GrowthMath.BonusSeconds(10, 1, 1), "factor one disables new bonus");
Equal(90, GrowthMath.BonusSeconds(10, 1, 10), "maximum configured factor");
Equal(0, GrowthMath.BonusSeconds(-10, 1, 2), "negative time cannot reverse growth");
Equal(10, GrowthMath.BonusSeconds(3600, 1, 2), "no unloaded-time catchup");
Equal(0, GrowthMath.BonusSeconds(10, float.NaN, 2), "invalid skill cannot corrupt saved bonus");

var clock = new GrowthClock();
Equal(0, clock.Sample(100, true, 7), "first owner observation gives no retroactive bonus");
Equal(10, clock.Sample(110, true, 7), "continuous ownership counts interval");
Equal(0, clock.Sample(120, false, 0), "non-owner gives no bonus");
Equal(0, clock.Sample(130, true, 7), "reacquisition starts new interval");
Equal(0, clock.Sample(140, true, 8), "owner identity change starts new interval");
Equal(10, clock.Sample(500, true, 8), "suspension capped at one update");
Equal(0, clock.Sample(490, true, 8), "clock regression gives no bonus");
Equal(0, new GrowthClock().Sample(10000, true, 8), "reload does not count time away");

Equal(63, GrowthMath.Percent(600, 30, 1000), "display includes accumulated bonus");
Equal(99, GrowthMath.Percent(999, 0, 1000), "does not round premature completion up");
Equal(100, GrowthMath.Percent(900, 200, 1000), "display clamps completion");
Equal(0, GrowthMath.Percent(-10, 0, 1000), "display clamps negative age");
Equal(0, GrowthMath.Percent(10, 0, 0), "zero duration handled");
Equal(50, GrowthMath.Percent(400, 100, 1000), "persisted bonus remains in progress after reload");
void Species(GrowthSpecies expected, string juvenile, string adult)
{
    if (GrowthPolicy.Classify(juvenile, adult) != expected)
        throw new Exception($"Unexpected species for {juvenile} -> {adult}");
    checks++;
}

Species(GrowthSpecies.Chicken, "Chicken", "Hen");
Species(GrowthSpecies.Asksvin, "Asksvin_hatchling", "Asksvin");
Species(GrowthSpecies.None, "Wolf_cub", "Wolf");
Species(GrowthSpecies.None, "Boar_piggy", "Boar");
Species(GrowthSpecies.None, "Asksvin", "Asksvin");
Species(GrowthSpecies.None, "CustomHatchling", "Asksvin");
Species(GrowthSpecies.None, "Chicken", "Asksvin");
Species(GrowthSpecies.None, "Asksvin_hatchling", "Hen");
Species(GrowthSpecies.None, "ChickenEgg", "Chicken");
Species(GrowthSpecies.None, "AsksvinEgg", "Asksvin_hatchling");
Species(GrowthSpecies.None, "asksvin_hatchling", "Asksvin");

Equal(0, GrowthPolicy.ShowInfo(1f, 0) ? 1 : 0, "zero requirement disables hover");
Equal(0, GrowthPolicy.ShowInfo(0.29f, 30) ? 1 : 0, "hover locked below skill requirement");
Equal(1, GrowthPolicy.ShowInfo(0.30f, 30) ? 1 : 0, "hover unlocks at requirement");
Equal(0, GrowthPolicy.ShowInfo(float.NaN, 30) ? 1 : 0, "invalid skill cannot unlock hover");
Equal(0, GrowthPolicy.ShowInfo(float.PositiveInfinity, 30) ? 1 : 0, "infinite skill cannot unlock hover");
Equal(1, GrowthPolicy.ShowInfo(1f, 100) ? 1 : 0, "level 100 requirement is attainable");
void Text(string expected, string actual, string scenario)
{
    if (expected != actual) throw new Exception($"{scenario}: expected {expected}, got {actual}");
    checks++;
}
Equal((double)GrowthSpecies.Chicken, (double)GrowthPolicy.ClassifyEgg("ChickenEgg", "Chicken"), "chicken egg identified");
Equal((double)GrowthSpecies.Asksvin, (double)GrowthPolicy.ClassifyEgg("AsksvinEgg", "Asksvin_hatchling"), "Asksvin egg identified");
Equal((double)GrowthSpecies.None, (double)GrowthPolicy.ClassifyEgg("DragonEgg", "Asksvin_hatchling"), "unrelated egg excluded");
Equal((double)GrowthSpecies.None, (double)GrowthPolicy.ClassifyEgg("AsksvinEgg", "Chicken"), "mismatched hatchling excluded");
Text("Incubation: 50% (15:00 remaining)", EggInfo.Describe(1, 100, 1000, 1800), "warm egg progress and countdown");
Text("Incubation: 99% (00:01 remaining)", EggInfo.Describe(1, 100, 1899, 1800), "not rounded to completion early");
Text("Incubation: 100% (00:00 remaining)", EggInfo.Describe(1, 100, 1910, 1800), "completed timer awaits vanilla hatch update");
Text("Incubation: not incubating", EggInfo.Describe(1, 0, 1000, 1800), "cold egg is not shown as complete");
Text("Incubation: not incubating", EggInfo.Describe(1, double.NaN, 1000, 1800), "invalid timer is not treated as active");
Text("Incubation: separate stacked eggs to hatch", EggInfo.Describe(2, 100, 1000, 1800), "stacked eggs cannot hatch");
Text("Incubation: 0% (30:00 remaining)", EggInfo.Describe(1, 1000, 100, 1800), "future timestamp is clamped");
Text("Incubation: unavailable", EggInfo.Describe(1, 100, 1000, 0), "invalid duration cannot divide by zero");
Text("Incubation: 0% (90:00 remaining)", EggInfo.Describe(1, 100, 100, 5400), "long modded incubation duration");
Console.WriteLine($"PASS: {checks} growth, ownership, species, hover, and egg incubation checks");
