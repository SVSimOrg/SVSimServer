using System;

namespace Cute;

public interface ILocalKVS : IDisposable
{
	string savePath { get; }

	string Get(string key);

	void Set(string key, string value);

	void Delete(string key);

	void DeleteAll();

	void Transaction(Action block);

	void Optimize();
}
