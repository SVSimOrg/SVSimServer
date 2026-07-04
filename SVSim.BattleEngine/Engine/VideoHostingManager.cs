public class VideoHostingManager : SingletonMonoBehaviour<VideoHostingManager>
{
	private VideoHostingImplBase _impl;

	public void StopRecording()
	{
		_impl.StopRecording();
	}

	public bool IsRecording()
	{
		return _impl.IsRecording();
	}

	public bool IsRecordingPause()
	{
		return _impl.IsRecordingPause();
	}

	public void PausePublishing()
	{
		_impl.PausePublishing();
	}

	public bool IsPublising()
	{
		return _impl.IsPublising();
	}

	public bool IsPublishingPause()
	{
		return _impl.IsPublishingPause();
	}

	public void SetRecordingFaceCameraMicrophoneStatus(bool isEnableCamera, bool isEnableMicrophone)
	{
		_impl.SetRecordingFaceCameraMicrophoneStatus(isEnableCamera, isEnableMicrophone);
	}

	public void SetPublishingFaceCameraMicrophoneStatus(bool isEnableCamera, bool isEnableMicrophone)
	{
		_impl.SetPublishingFaceCameraMicrophoneStatus(isEnableCamera, isEnableMicrophone);
	}
}
