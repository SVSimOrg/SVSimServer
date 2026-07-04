using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite3Plugin;

public class Query : IDisposable
{
	protected DBProxy _proxy;

	protected IntPtr _stmt = IntPtr.Zero;

	public Query(DBProxy proxy, string sql)
	{
		_Setup(proxy, sql);
	}

	protected void _Setup(DBProxy proxy, string sql)
	{
		_proxy = proxy;
		byte[] bytes = Encoding.UTF8.GetBytes(sql);
		IntPtr ppStmt;
		int num = Sqlite3LibImport.sqlite3_prepare_v2(proxy.DBHandle, bytes, bytes.Length, out ppStmt, IntPtr.Zero);
		if (num != 0)
		{
			ResultCode.CheckCorruption(num);
			throw new Exception($"sqlite3_prepare_v2 failed(code {num}) with sql: {sql}");
		}
		_stmt = ppStmt;
	}

	public virtual void Dispose()
	{
		if (_stmt != IntPtr.Zero)
		{
			int num = Sqlite3LibImport.sqlite3_finalize(_stmt);
			_stmt = IntPtr.Zero;
			if (num != 0)
			{
				Debug.LogError("sqlite3_finalize error: " + num);
				ResultCode.CheckCorruption(num);
			}
		}
	}

	public bool Step()
	{
		int num = Sqlite3LibImport.sqlite3_step(_stmt);
		bool num2 = num == 100;
		if (!num2)
		{
			ResultCode.CheckCorruption(num);
		}
		return num2;
	}

	public int GetInt(int idx)
	{
		return Sqlite3LibImport.sqlite3_column_int(_stmt, idx);
	}

	public string GetText(int idx)
	{
		return Marshal.PtrToStringAnsi(Sqlite3LibImport.sqlite3_column_text(_stmt, idx));
	}
}
