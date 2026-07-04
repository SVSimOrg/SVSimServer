using System;

public class NetworkLog
{
	public enum LogLevel
	{
		Info	}

	public LogLevel Level { get; }

	public string Message { get; }

	public DateTime Time { get; }

	public NetworkLog(LogLevel level, string message)
	{
		Level = level;
		Message = message;
		Time = DateTime.UtcNow;
	}

	public override string ToString()
	{
		return $"{Time}:[{Level.ToString()}]{Message}";
	}
}
