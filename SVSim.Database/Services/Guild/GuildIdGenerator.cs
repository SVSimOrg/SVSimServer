namespace SVSim.Database.Services.Guild;

public interface IGuildIdGenerator
{
    Task<int> NextAsync(Func<int, CancellationToken, Task<bool>> existsAsync, CancellationToken ct);
}

public sealed class GuildIdGenerator : IGuildIdGenerator
{
    private static readonly System.Security.Cryptography.RandomNumberGenerator _rng =
        System.Security.Cryptography.RandomNumberGenerator.Create();

    public async Task<int> NextAsync(Func<int, CancellationToken, Task<bool>> existsAsync, CancellationToken ct)
    {
        const int min = 100_000_000, max = 999_999_999;
        const int attempts = 16;
        for (int i = 0; i < attempts; i++)
        {
            var bytes = new byte[4]; _rng.GetBytes(bytes);
            var raw = BitConverter.ToUInt32(bytes, 0);
            int id = min + (int)(raw % (uint)(max - min + 1));
            if (!await existsAsync(id, ct)) return id;
        }
        throw new InvalidOperationException("Exhausted guild_id collision retries — id space too saturated.");
    }
}
