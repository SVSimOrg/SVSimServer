using System;
using System.Text;

namespace Sqlite3Plugin;

public class PreparedQuery : Query
{
	public PreparedQuery(DBProxy proxy, string sql)
		: base(proxy, sql)
	{
	}

	public bool BindText(int idx, string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		int num = Sqlite3LibImport.sqlite3_bind_text(_stmt, idx, bytes, bytes.Length, IntPtr.Zero);
		if (num != 0)
		{
			Debug.LogError($"sqlite3_bind_text error at idx {idx}: code {num}");
			ResultCode.CheckCorruption(num);
		}
		return num == 0;
	}

	public bool Reset()
	{
		int num = Sqlite3LibImport.sqlite3_reset(_stmt);
		if (num != 0)
		{
			Debug.LogError($"sqlite3_reset error: code {num}");
			ResultCode.CheckCorruption(num);
		}
		return num == 0;
	}
}
