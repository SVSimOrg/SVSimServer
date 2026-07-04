using System;

namespace Sqlite3Plugin;

public class ResultCode
{

	public static void CheckCorruption(int rc, string errMsg = null)
	{
		if (rc == 11 || rc == 26)
		{
			throw new DatabaseCorruptionException(rc);
		}
		if (!string.IsNullOrEmpty(errMsg) && errMsg.IndexOf("unsupported file format", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			throw new DatabaseCorruptionException(26);
		}
	}
}
