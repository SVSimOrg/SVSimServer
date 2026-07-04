using System;
using System.Collections.Generic;

namespace Cute;

public class ManifestDatahashKVS : ILocalKVS, IDisposable
{
	protected LocalSqliteKVS _kvs;

	public string savePath => _kvs.savePath;

	public ManifestDatahashKVS(string path)
	{
		_kvs = LocalSqliteKVS.Open(path, enableCache: true);
	}

	public virtual void Dispose()
	{
		if (_kvs != null)
		{
			_kvs.Dispose();
			_kvs = null;
		}
	}

	public string Get(string name)
	{
		string text = _kvs.Get(name);
		return (text == null) ? "" : text;
	}

	public void Set(string name, string hash)
	{
		_kvs.Set(name, hash);
	}

	public void Delete(string name)
	{
		_kvs.Delete(name);
	}

	public void DeleteAll()
	{
		_kvs.DeleteAll();
		Optimize();
	}

	public void Optimize()
	{
		_kvs.Optimize();
	}

	public void Transaction(Action block)
	{
		_kvs.Transaction(block);
	}
}
