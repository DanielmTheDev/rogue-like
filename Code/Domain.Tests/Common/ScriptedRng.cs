using System;
using System.Collections.Generic;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

/// <summary>
/// Test <see cref="IRng"/> that replays a fixed script of values, so probabilistic
/// domain logic can be asserted exactly (no statistical sampling). Throws if it
/// runs out of scripted values — surfacing a wrong assumption about draw count.
/// </summary>
internal sealed class ScriptedRng : IRng
{
    private readonly Queue<double> _doubles;
    private readonly Queue<int> _ints;

    public ScriptedRng(params double[] doubles)
    {
        _doubles = new Queue<double>(doubles);
        _ints = new Queue<int>();
    }

    public ScriptedRng(double[] doubles, int[] ints)
    {
        _doubles = new Queue<double>(doubles);
        _ints = new Queue<int>(ints);
    }

    public double NextDouble()
    {
        if (_doubles.Count == 0)
            throw new InvalidOperationException("ScriptedRng ran out of scripted NextDouble values.");
        return _doubles.Dequeue();
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (_ints.Count == 0)
            throw new InvalidOperationException("ScriptedRng ran out of scripted Next values.");
        return _ints.Dequeue();
    }
}
