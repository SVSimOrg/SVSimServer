using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.View;

namespace Wizard.Battle;

public class EnemyAIInnerOptionsBuilder : IInnerOptionsBuilder
{
	public IPlayerEmotion CreatePlayerEmotion(IClassBattleCardView classCardView)
	{
		return new NullPlayerEmotion();
	}

	public IEmotion CreateEnemyEmotion(IClassBattleCardView classCardView)
	{
		return new EnemyAIEmotion("0", classCardView.ClassCharacter, null); // Pre-Phase-5b: emotion id + EnemyAI not reachable headless
	}

	public CardInnerOptionsBase CreateCardOptions()
	{
		return new CardInnerOptionsBase();
	}
}
