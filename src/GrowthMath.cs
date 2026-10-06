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

internal sealed class GrowthClock
{
	private double lastTime;
	private long lastOwner;
	private bool wasOwner;

	public GrowthClock() { }

	internal double Sample(double now, bool owner, long ownerId)
	{
		double elapsed = owner && wasOwner && lastOwner == ownerId ? Math.Max(0, Math.Min(10, now - lastTime)) : 0;
		lastTime = now;
		lastOwner = ownerId;
		wasOwner = owner;
		return elapsed;
	}
}
