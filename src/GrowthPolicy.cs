namespace RanchingAddon;

internal enum GrowthSpecies
{
    None,
    Chicken,
    Asksvin
}

internal static class GrowthPolicy
{
    // Check both ends of maturation, so other livestock and custom creatures
    // aren't accelerated merely because they have a Growup component.
    internal static GrowthSpecies Classify(string juvenile, string adult)
    {
        if (juvenile == "Chicken" && adult == "Hen") return GrowthSpecies.Chicken;
        if (juvenile == "Asksvin_hatchling" && adult == "Asksvin") return GrowthSpecies.Asksvin;
        return GrowthSpecies.None;
    }

    internal static bool ShowInfo(float skill, int requiredLevel) =>
        requiredLevel > 0 && !float.IsNaN(skill) && !float.IsInfinity(skill) && skill >= requiredLevel / 100f;
}
