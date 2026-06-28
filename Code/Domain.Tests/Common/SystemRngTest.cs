using Xunit;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

public class SystemRngTest
{
    [Fact]
    public void SameSeed_ProducesIdenticalSequence()
    {
        var a = new SystemRng(1234);
        var b = new SystemRng(1234);

        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(a.NextDouble(), b.NextDouble());
            Assert.Equal(a.Next(0, 100), b.Next(0, 100));
        }
    }

    [Fact]
    public void DifferentSeeds_Differ()
    {
        var a = new SystemRng(1);
        var b = new SystemRng(2);

        var sameRun = true;
        for (var i = 0; i < 20; i++)
            if (a.NextDouble() != b.NextDouble())
                sameRun = false;

        Assert.False(sameRun);
    }

    [Fact]
    public void Next_StaysWithinRange()
    {
        var rng = new SystemRng(7);

        for (var i = 0; i < 100; i++)
        {
            var n = rng.Next(3, 8);
            Assert.InRange(n, 3, 7);
        }
    }
}
