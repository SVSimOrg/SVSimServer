using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
// TODO(engine-cleanup-pass2): 2 of 3 methods unrun in baseline
//   Type: Wizard.Battle.PlayerInnerOptionsBuilder
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle;

public class PlayerInnerOptionsBuilder : IInnerOptionsBuilder
{
	private readonly IBattleResourceMgr _resourceMgr;

	public PlayerInnerOptionsBuilder(IBattleResourceMgr resourceMgr)
	{
		_resourceMgr = resourceMgr;
	}

	public IPlayerEmotion CreatePlayerEmotion(IClassBattleCardView classCardView)
	{
		return new PlayerEmotion("0", classCardView.ClassCharacter, _resourceMgr); // Pre-Phase-5b: player emotion id not reachable headless
	}

	public IEmotion CreateEnemyEmotion(IClassBattleCardView classCardView)
	{
		return new NullEmotion();
	}

	public CardInnerOptionsBase CreateCardOptions()
	{
		return new PlayerCardInnerOptions();
	}
}
