using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.View;

namespace Wizard.Battle;

public interface IInnerOptionsBuilder
{
	IPlayerEmotion CreatePlayerEmotion(IClassBattleCardView classCardView);

	IEmotion CreateEnemyEmotion(IClassBattleCardView classCardView);

	CardInnerOptionsBase CreateCardOptions();
}
