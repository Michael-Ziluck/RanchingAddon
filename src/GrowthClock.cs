using System;

namespace RanchingAddon;

internal sealed class GrowthClock
{
    private double lastSampleTime;
    private long lastOwnerId;
    private bool wasOwner;

    // ConditionalWeakTable.GetOrCreateValue requires a public constructor.
    public GrowthClock() { }

    internal double Sample(double now, bool owner, long ownerId)
    {
        bool continuousOwnership = owner && wasOwner && lastOwnerId == ownerId;
        double elapsed = continuousOwnership ? Math.Max(0, Math.Min(10, now - lastSampleTime)) : 0;
        lastSampleTime = now;
        lastOwnerId = ownerId;
        wasOwner = owner;
        return elapsed;
    }
}
