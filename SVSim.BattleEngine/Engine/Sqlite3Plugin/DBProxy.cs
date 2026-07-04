using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite3Plugin;

public class DBProxy : IDisposable
{
	public IntPtr DBHandle { get; private set; }

	public string dbPath { get; private set; }

	public DBProxy()
	{
		dbPath = null;
		DBHandle = IntPtr.Zero;
	}

	public bool Open(string fileName, string vfsName = null)
	{
		dbPath = fileName;
		IntPtr ppDB = IntPtr.Zero;
		byte[] bytes = Encoding.UTF8.GetBytes(fileName + "\0");
		byte[] zVfs = null;
		if (!string.IsNullOrEmpty(vfsName))
		{
			zVfs = Encoding.UTF8.GetBytes(vfsName + "\0");
		}
		int num = Sqlite3LibImport.sqlite3_open_v2(bytes, out ppDB, 1, zVfs);
		DBHandle = ppDB;
		bool num2 = num == 0;
		if (num2)
		{
			Exec("pragma journal_mode=OFF");
			Exec("pragma synchronous=0");
			Exec("pragma locking_mode=EXCLUSIVE");
			return num2;
		}
		Debug.LogError("sqlite3_open failed: code " + num);
		return num2;
	}

	public bool OpenWritable(string fileName)
	{
		dbPath = fileName;
		IntPtr ppDB = IntPtr.Zero;
		bool flag = true;
		try
		{
			int num = Sqlite3LibImport.sqlite3_open(Encoding.UTF8.GetBytes(fileName + "\0"), out ppDB);
			DBHandle = ppDB;
			flag = num == 0;
			if (flag)
			{
				Exec("pragma journal_mode=MEMORY");
				Exec("pragma synchronous=1");
				Exec("pragma locking_mode=EXCLUSIVE");
			}
			else
			{
				Debug.LogError("sqlite3_open failed: " + num);
			}
		}
		catch (Exception ex)
		{
			if (ppDB != IntPtr.Zero)
			{
				Sqlite3LibImport.sqlite3_close(ppDB);
				DBHandle = IntPtr.Zero;
			}
			throw ex;
		}
		return flag;
	}

	public bool Begin()
	{
		return Exec("BEGIN;");
	}

	public bool Commit()
	{
		return Exec("COMMIT;");
	}

	public bool Rollback()
	{
		return Exec("ROLLBACK;");
	}

	public bool Vacuum()
	{
		return Exec("VACUUM;");
	}

	public virtual void Dispose()
	{
		CloseDB();
	}

	public virtual void CloseDB()
	{
		if (DBHandle != IntPtr.Zero)
		{
			int num = Sqlite3LibImport.sqlite3_close(DBHandle);
			if (num != 0)
			{
				Debug.LogError("failed to close db at " + dbPath + ": " + num);
			}
			DBHandle = IntPtr.Zero;
		}
	}

	public bool Exec(string sql)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(sql + "\0");
		IntPtr pzErrMsg;
		int num = Sqlite3LibImport.sqlite3_exec(DBHandle, bytes, IntPtr.Zero, IntPtr.Zero, out pzErrMsg);
		if (num != 0)
		{
			string text = ((pzErrMsg == IntPtr.Zero) ? "" : Marshal.PtrToStringAnsi(pzErrMsg));
			Debug.LogError($"sqlite3_exec failed (code {num}: {text}) with sql: {sql}");
			ResultCode.CheckCorruption(num, text);
		}
		return num == 0;
	}

	public Query Query(string sql)
	{
		return new Query(this, sql);
	}

	public PreparedQuery PreparedQuery(string sql)
	{
		return new PreparedQuery(this, sql);
	}
}
