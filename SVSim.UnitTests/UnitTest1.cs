namespace SVSim.UnitTests;

public class SmokeTests
{
    [Test]
    public void CanLoadAssembly()
    {
        Assert.That(typeof(SVSim.EmulatedEntrypoint.Program).Assembly, Is.Not.Null);
    }
}
