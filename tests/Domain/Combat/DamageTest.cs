using GdUnit4;
using RogueLike.Code.Domain.Combat;
using static GdUnit4.Assertions;

namespace RogueLike.tests.Domain.Combat;

[TestSuite]
public class DamageTest
{
    [TestCase]
    public void Constructor_StoresAmount()
    {
        var damage = new Damage(7);

        AssertInt(damage.Amount).IsEqual(7);
    }

    [TestCase]
    public void Constructor_NegativeAmount_Throws()
    {
        var threw = false;
        try { _ = new Damage(-1); }
        catch (System.ArgumentOutOfRangeException) { threw = true; }

        AssertBool(threw).IsTrue();
    }

    [TestCase]
    public void None_IsZero()
    {
        AssertInt(Damage.None.Amount).IsEqual(0);
    }

    [TestCase]
    public void Equality_IsByValue()
    {
        AssertBool(new Damage(5) == new Damage(5)).IsTrue();
        AssertBool(new Damage(5) == new Damage(6)).IsFalse();
    }
}
