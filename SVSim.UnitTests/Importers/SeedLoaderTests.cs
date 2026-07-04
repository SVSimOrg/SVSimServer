using SVSim.Bootstrap.Importers;

namespace SVSim.UnitTests.Importers;

public class SeedLoaderTests
{
    private sealed record Row(int Id, string Name);

    [Test]
    public void LoadList_returns_empty_when_file_missing()
    {
        string path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid()}.json");
        var rows = SeedLoader.LoadList<Row>(path);
        Assert.That(rows, Is.Empty);
    }

    [Test]
    public void LoadList_deserializes_snake_case_array()
    {
        string path = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}.json");
        File.WriteAllText(path, "[{\"id\":1,\"name\":\"a\"},{\"id\":2,\"name\":\"b\"}]");
        try
        {
            var rows = SeedLoader.LoadList<Row>(path);
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].Id, Is.EqualTo(1));
            Assert.That(rows[1].Name, Is.EqualTo("b"));
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void LoadObject_returns_null_when_file_missing()
    {
        string path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid()}.json");
        var row = SeedLoader.LoadObject<Row>(path);
        Assert.That(row, Is.Null);
    }
}
