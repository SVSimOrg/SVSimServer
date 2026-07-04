using Wizard.Battle.Player.ClassCharacter;

namespace Wizard.Battle.Player.Emotion;

public class NetworkOpponentEmotion : EnemyEmotionBase
{
	public NetworkOpponentEmotion(string emotionId, IClassCharacter classCharacter)
		: base(emotionId, classCharacter)
	{
	}
}
