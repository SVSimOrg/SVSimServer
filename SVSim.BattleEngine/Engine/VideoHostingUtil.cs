using System;
using System.Collections;
using UnityEngine;
using Wizard;

public class VideoHostingUtil : SingletonMonoBehaviour<VideoHostingUtil>
{
	public enum Option
	{
		EnemyNameDisplay,
		AutoPausePublishing,
		AutoStopRecording,
		Max
	}

	public enum HUDScene
	{
		Battle = 2,
		Default = 1
	}

	private enum RecordingAutoStopStatus
	{
}

	public static void SetOptionFlagFromPlayerPrefs(Option option, bool enable)
	{
		if (option >= Option.EnemyNameDisplay && option < Option.Max)
		{
			int value = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.VIDEOHOSTING_FLAGS);
			value = ((!enable) ? (value & ~(1 << (int)option)) : (value | (1 << (int)option)));
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.VIDEOHOSTING_FLAGS, value);
		}
	}

	public static void CheckVideoHostingAndExecAction(Action cbClose)
	{
		bool flag = SingletonMonoBehaviour<VideoHostingManager>.instance.IsPublising() && !SingletonMonoBehaviour<VideoHostingManager>.instance.IsPublishingPause();
		bool flag2 = SingletonMonoBehaviour<VideoHostingManager>.instance.IsRecording() && !SingletonMonoBehaviour<VideoHostingManager>.instance.IsRecordingPause();
		if (flag || flag2)
		{
			SystemText systemText = Data.SystemText;
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetSize(DialogBase.Size.S);
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			dialogBase.SetTitleLabel(systemText.Get("Common_0021"));
			dialogBase.SetButtonText(systemText.Get("Common_0004"));
			dialogBase.SetPanelDepth(5000);
			dialogBase.OnClose = (Action)Delegate.Combine(dialogBase.OnClose, cbClose);
			if (flag)
			{
				dialogBase.SetText(systemText.Get("VideoHosting_0069"));
			}
			else
			{
				dialogBase.SetText(systemText.Get("VideoHosting_0077"));
			}
		}
		else
		{
			cbClose();
		}
	}

	public static void AutoPausePublishing(bool isSave)
	{
		if (SingletonMonoBehaviour<VideoHostingManager>.instance.IsPublising() && !SingletonMonoBehaviour<VideoHostingManager>.instance.IsPublishingPause())
		{
			SingletonMonoBehaviour<VideoHostingManager>.instance.PausePublishing();
			if (isSave)
			{
				SetOptionFlagFromPlayerPrefs(Option.AutoPausePublishing, enable: true);
			}
		}
	}

	public static void AutoStopRecording(bool isSave)
	{
		if (SingletonMonoBehaviour<VideoHostingManager>.instance.IsRecording())
		{
			SingletonMonoBehaviour<VideoHostingManager>.instance.StopRecording();
			if (isSave)
			{
				SetOptionFlagFromPlayerPrefs(Option.AutoStopRecording, enable: true);
			}
		}
	}

	public static void OnSoftwareReset()
	{
		AutoPausePublishing(isSave: true);
		AutoStopRecording(isSave: true);
		SingletonMonoBehaviour<VideoHostingManager>.instance.SetRecordingFaceCameraMicrophoneStatus(isEnableCamera: false, isEnableMicrophone: false);
		SingletonMonoBehaviour<VideoHostingManager>.instance.SetPublishingFaceCameraMicrophoneStatus(isEnableCamera: false, isEnableMicrophone: false);
	}
}
