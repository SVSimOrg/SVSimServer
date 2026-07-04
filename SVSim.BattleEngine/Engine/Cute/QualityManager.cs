using System.Collections;
using UnityEngine;

namespace Cute;

public class QualityManager : MonoBehaviour, IManager
{
	public enum GPUQualityLevel
	{
	}

	public enum AssetQualityLevel
	{
		None}

	public enum SoundQualityLevel
	{
		None}

	public enum MovieQualityLevel
	{
		None}

	public enum MemoryQualityLevel
	{
	}

	public enum GameQualityLevel
	{
	}

	public enum OutLineLevel
	{
	}

	private Vector2 _deviceResolution = Vector2.zero;

	private int _defaultFrameRate = 60;

	public bool isFullScreen;

	private void Awake()
	{
		UpdateFromUnity();
	}

	public int GetFrameRate()
	{
		return _defaultFrameRate;
	}

	public void UpdateFromUnity()
	{
		isFullScreen = Screen.fullScreen;
		_deviceResolution = new Vector2(Screen.width, Screen.height);
	}
}
