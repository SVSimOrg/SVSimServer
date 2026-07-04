using System.Collections;
using System.Collections.Generic;

public interface INetworkLogger<out T> : IEnumerable<T>, IEnumerable
{
	void LogInfo(string text);

	void LogWarning(string text);

	void LogError(string text);

	void ClearLog();
}
