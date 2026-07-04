using System;
using UnityEngine;

namespace Cute;

public static class AchievementManager
{
	private static IAchievementCallback mCallback;

	public static void Initialize(IAchievementCallback callback)
	{
		mCallback = callback;
	}

	public static void ShowAchievementsUI()
	{
		Social.ShowAchievementsUI();
	}

	public static void ReleaseAchievement(string id)
	{
		Social.ReportProgress(id, 100.0, delegate(bool success)
		{
			if (mCallback != null)
			{
				mCallback.OnReleaseAchievement(success);
			}
		});
	}
}
