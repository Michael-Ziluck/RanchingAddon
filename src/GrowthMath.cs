using System;

namespace RanchingAddon;

internal static class GrowthMath
{
	internal static double BonusSeconds(double seconds, float skill, float factor)
	{
		if (double.IsNaN(seconds) || float.IsNaN(skill) || float.IsNaN(factor)) return 0;
		return Math.Max(0, Math.Min(10, seconds)) * Math.Max(0, Math.Min(1, skill)) * Math.Max(0, Math.Min(10, factor) - 1);
	}

	internal static int Percent(double age, double bonus, double duration)
	{
		if (duration <= 0 || double.IsNaN(age + bonus + duration)) return 0;
		return (int)Math.Floor(Math.Max(0, Math.Min(100, (age + bonus) / duration * 100)));
	}
}
