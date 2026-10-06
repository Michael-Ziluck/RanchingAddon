using System;

namespace RanchingAddon;

internal sealed class GrowthClock
{
    private double lastTime;
    private long lastOwner;
    private bool wasOwner;

    // ConditionalWeakTable.GetOrCreateValue requires a public constructor.
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
