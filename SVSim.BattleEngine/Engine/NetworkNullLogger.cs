using System.Collections;
using System.Collections.Generic;

public class NetworkNullLogger : INetworkLogger<NetworkLog>, IEnumerable<NetworkLog>, IEnumerable
{
	public void LogInfo(string text)
	{
	}

	public void LogError(string text)
	{
	}

	public void LogWarning(string text)
	{
	}

	public void ClearLog()
	{
	}

	public IEnumerator<NetworkLog> GetEnumerator()
	{
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
