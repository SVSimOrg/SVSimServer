using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public class EnemyAIEmotion : EnemyEmotionBase
{
	private IEnemyAI _enemyAI;

	public EnemyAIEmotion(string emotionId, IClassCharacter classCharacter, IEnemyAI enemyAI)
		: base(emotionId, classCharacter)
	{
		_enemyAI = enemyAI;
	}

	public override VfxBase ReceiveOpponentEmotion(ClassCharaPrm.EmotionType emotionType)
	{
		return _enemyAI.GetEmote(AIEmoteCmdType.ON_RECEIVE, null, emotionType);
	}
}
