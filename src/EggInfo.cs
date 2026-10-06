using System;

namespace RanchingAddon;

internal static class EggInfo
{
    internal static string Describe(int stack, double started, double now, double duration)
    {
        if (stack > 1) return "Incubation: separate stacked eggs to hatch";
        if (started <= 0 || double.IsNaN(started) || double.IsInfinity(started))
            return "Incubation: not incubating";
        if (duration <= 0 || double.IsNaN(duration) || double.IsInfinity(duration) || double.IsNaN(now) || double.IsInfinity(now))
            return "Incubation: unavailable";
        double elapsed = Math.Max(0, now - started);
        int percent = GrowthMath.Percent(elapsed, 0, duration);
        double seconds = Math.Ceiling(Math.Max(0, duration - elapsed));
        // Show total minutes so a modded incubation duration can exceed an hour.
        string remaining = $"{Math.Floor(seconds / 60):00}:{seconds % 60:00}";
        return $"Incubation: {percent}% ({remaining} remaining)";
    }
}
