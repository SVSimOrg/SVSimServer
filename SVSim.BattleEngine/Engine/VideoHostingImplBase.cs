using UnityEngine;

public abstract class VideoHostingImplBase : MonoBehaviour
{
	protected virtual void Awake()
	{
	}

	public virtual void StopRecording()
	{
	}

	public virtual bool IsRecording()
	{
		return false;
	}

	public virtual bool IsRecordingPause()
	{
		return false;
	}

	public virtual void PausePublishing()
	{
	}

	public virtual bool IsPublising()
	{
		return false;
	}

	public virtual bool IsPublishingPause()
	{
		return false;
	}

	public virtual void SetRecordingFaceCameraMicrophoneStatus(bool isEnableCamera, bool isEnalbeMicrophon)
	{
	}

	public virtual void SetPublishingFaceCameraMicrophoneStatus(bool isEnableCamera, bool isEnalbeMicrophon)
	{
	}
}
