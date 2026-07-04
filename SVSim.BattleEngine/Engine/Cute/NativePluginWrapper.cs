using UnityEngine;

namespace Cute;

public static class NativePluginWrapper
{
	public enum BatteryState
	{
}

	public enum NetworkMode
	{
}

	public static void SetStringToClipboard(string copyText)
	{
		ClipboardHelper.Clipboard = copyText;
	}
}
