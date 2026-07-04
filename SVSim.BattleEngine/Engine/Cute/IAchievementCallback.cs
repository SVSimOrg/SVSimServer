using UnityEngine.SocialPlatforms;

namespace Cute;

public interface IAchievementCallback
{
	void OnSignIn(bool success);

	void OnSignOut();

	void OnReleaseAchievement(bool success);

	void OnProceedAchievement(bool success);

	void OnLoadAchievements(IAchievement[] achievements);

	void OnLoadAchievementDescriptions(IAchievementDescription[] descriptions);
}
