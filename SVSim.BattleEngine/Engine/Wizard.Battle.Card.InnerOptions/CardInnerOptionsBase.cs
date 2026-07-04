using Wizard.Battle.View;
// TODO(engine-cleanup-pass2): 1 of 2 methods unrun in baseline
//   Type: Wizard.Battle.Card.InnerOptions.CardInnerOptionsBase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Card.InnerOptions;

public class CardInnerOptionsBase
{
	public virtual bool CheckMovable(IBattlePlayerView selfPlayerBattleView, IBattleCardView selfBattleCardView, bool isOnDraw, bool isSkipSelecting, bool isRecording)
	{
		return true;
	}

	public virtual CardInnerOptionsBase VirtualClone()
	{
		return new CardInnerOptionsBase();
	}
}
