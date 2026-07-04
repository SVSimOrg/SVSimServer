using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Sqlite3Plugin;

namespace Cute;

public class LocalSqliteKVS : ILocalKVS, IDisposable
{
	protected DBProxy _db;

	protected PreparedQuery _keyQuery;

	protected PreparedQuery _upsertQuery;

	protected PreparedQuery _deleteQuery;

	protected PreparedQuery _likeQuery;

	protected PreparedQuery _selectAllQuery;

	protected bool _enableCache;

	protected Dictionary<string, string> _tableCache;

	public string savePath => _db.dbPath;

	protected LocalSqliteKVS(string path, bool enableCache)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			_db = new DBProxy();
			if (!_db.OpenWritable(path))
			{
				throw new ApplicationException($"Failed to open LocalKVS at {path}");
			}
			if (!_db.Exec("CREATE TABLE IF NOT EXISTS t (k TEXT NOT NULL, v TEXT NOT NULL, PRIMARY KEY(k));"))
			{
				throw new ApplicationException($"Failed to initialize LocalKVS at {path}");
			}
			_db.Exec("pragma cache_size=0");
			_keyQuery = _db.PreparedQuery("SELECT v FROM t WHERE k=?;");
			_upsertQuery = _db.PreparedQuery("REPLACE INTO t(k,v)VALUES(?,?);");
			_deleteQuery = _db.PreparedQuery("DELETE FROM t WHERE k=?;");
			_likeQuery = _db.PreparedQuery("SELECT k FROM t WHERE k LIKE ? ESCAPE '!';");
			_selectAllQuery = _db.PreparedQuery("SELECT k FROM t;");
			_enableCache = enableCache;
			if (!_enableCache)
			{
				return;
			}
			using (Query query = _db.Query("SELECT COUNT(*) FROM t;"))
			{
				query.Step();
				int capacity = query.GetInt(0);
				_tableCache = new Dictionary<string, string>(capacity);
			}
			using Query query2 = _db.Query("SELECT k,v FROM t;");
			while (query2.Step())
			{
				string text = query2.GetText(0);
				string text2 = query2.GetText(1);
				_tableCache[text] = text2;
			}
		}
		catch (Exception ex)
		{
			Dispose();
			throw ex;
		}
	}

	public static LocalSqliteKVS Open(string path, bool enableCache)
	{
		return new LocalSqliteKVS(path, enableCache);
	}

	public virtual void Dispose()
	{
		if (_db != null)
		{
			if (_keyQuery != null)
			{
				_keyQuery.Dispose();
				_keyQuery = null;
			}
			if (_upsertQuery != null)
			{
				_upsertQuery.Dispose();
				_upsertQuery = null;
			}
			if (_deleteQuery != null)
			{
				_deleteQuery.Dispose();
				_deleteQuery = null;
			}
			if (_likeQuery != null)
			{
				_likeQuery.Dispose();
				_likeQuery = null;
			}
			if (_selectAllQuery != null)
			{
				_selectAllQuery.Dispose();
				_selectAllQuery = null;
			}
			_db.Dispose();
			_db = null;
			_tableCache = null;
		}
	}

	public string Get(string key)
	{
		if (_enableCache)
		{
			if (!_tableCache.TryGetValue(key, out var value))
			{
				return null;
			}
			return value;
		}
		try
		{
			_keyQuery.BindText(1, key);
			if (_keyQuery.Step())
			{
				return _keyQuery.GetText(0);
			}
			return null;
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			_keyQuery.Reset();
		}
	}

	public void Set(string key, string value)
	{
		if (_enableCache)
		{
			_tableCache[key] = value;
		}
		try
		{
			_upsertQuery.BindText(1, key);
			_upsertQuery.BindText(2, value);
			_upsertQuery.Step();
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			_upsertQuery.Reset();
		}
	}

	public void Delete(string key)
	{
		if (_enableCache && !_tableCache.Remove(key))
		{
			return;
		}
		try
		{
			_deleteQuery.BindText(1, key);
			_deleteQuery.Step();
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			_deleteQuery.Reset();
		}
	}

	public void Transaction(Action block)
	{
		if (!_db.Begin())
		{
			throw new ApplicationException("Failed to begin LocalKVS transaction");
		}
		try
		{
			block();
			_db.Commit();
		}
		catch (Exception ex)
		{
			_db.Rollback();
			throw ex;
		}
	}

	public void DeleteAll()
	{
		_db.Exec("DELETE FROM t;");
		if (_enableCache)
		{
			_tableCache.Clear();
		}
	}

	public void Optimize()
	{
		_db.Vacuum();
	}
}
