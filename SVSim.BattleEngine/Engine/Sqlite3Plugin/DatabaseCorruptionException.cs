using System;

namespace Sqlite3Plugin;

public class DatabaseCorruptionException : Exception
{
	public DatabaseCorruptionException(int rc)
		: base($"Database is corrupted: code {rc}")
	{
	}
}
