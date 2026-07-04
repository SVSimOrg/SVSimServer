using System;
using System.Runtime.InteropServices;

namespace Sqlite3Plugin;

public static class Sqlite3LibImport
{

	[DllImport("libsqlite3")]
	public static extern int sqlite3_open(byte[] zFilename, out IntPtr ppDB);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_open_v2(byte[] zFilename, out IntPtr ppDB, int flags, byte[] zVfs);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_close(IntPtr db);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_exec(IntPtr db, byte[] zSql, IntPtr xCallback, IntPtr pArg, out IntPtr pzErrMsg);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_prepare_v2(IntPtr db, byte[] zSql, int nBytes, out IntPtr ppStmt, IntPtr pzTail);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_bind_text(IntPtr pStmt, int i, byte[] zData, int nData, IntPtr xDel);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_reset(IntPtr pStmt);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_step(IntPtr pStmt);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_finalize(IntPtr pStmt);

	[DllImport("libsqlite3")]
	public static extern int sqlite3_column_int(IntPtr pStmt, int i);

	[DllImport("libsqlite3")]
	public static extern IntPtr sqlite3_column_text(IntPtr pStmt, int i);
}
