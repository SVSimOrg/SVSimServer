using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.View;
// TODO(engine-cleanup-pass2): 2 of 3 methods unrun in baseline
//   Type: Wizard.Battle.NetworkOpponentInnerOptionsBuilder
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle;

public class NetworkOpponentInnerOptionsBuilder : IInnerOptionsBuilder
{
	public IPlayerEmotion CreatePlayerEmotion(IClassBattleCardView classCardView)
	{
		return new NullPlayerEmotion();
	}

	public IEmotion CreateEnemyEmotion(IClassBattleCardView classCardView)
	{
		return new NetworkOpponentEmotion("0", classCardView.ClassCharacter); // Pre-Phase-5b: enemy chara id not reachable headless
	}

	public CardInnerOptionsBase CreateCardOptions()
	{
		return new CardInnerOptionsBase();
	}
}
