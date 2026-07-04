using System;

namespace Cute;

public class AssetRequestContext
{
	public Action<AssetHandle> callback { get; set; }

	public AssetErrorState errorState { get; set; }

	public Utility.LeanSemaphore semaphore { get; set; }

	public bool preferSynchronousLoad { get; set; }

	public AssetRequestContext(Action<AssetHandle> callback = null, Utility.LeanSemaphore semaphore = null, AssetErrorState errorState = null, bool preferSynchronousLoad = false)
	{
		this.callback = callback;
		this.semaphore = semaphore;
		this.errorState = errorState;
		this.preferSynchronousLoad = preferSynchronousLoad;
	}
}
