using System;
using Cute;

public class NetworkStatus : IDisposable
{
	public enum Status
	{
		None,
		Alive,
		TimeOut
	}

	public Action OnAlive;

	public Action OnDisconnect;

	public Action OnOffLine;

	public Action OnTimeOut;

	public Status Current { get; private set; }

	public bool IsAlive => Current == Status.Alive;

	public NetworkStatus()
	{
		Current = Status.None;
	}

	public void Dispose()
	{
		OnAlive = null;
		OnDisconnect = null;
		OnOffLine = null;
		OnTimeOut = null;
	}
}
