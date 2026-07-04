using Wizard.Battle.View;

namespace Wizard.Battle.Card.InnerOptions;

public class PlayerCardInnerOptions : CardInnerOptionsBase
{
	public override bool CheckMovable(IBattlePlayerView selfPlayerBattleView, IBattleCardView selfBattleCardView, bool isOnDraw, bool isSkipSelecting, bool isRecording)
	{
		if (selfPlayerBattleView == null)
		{
			return false;
		}
		if (!base.CheckMovable(selfPlayerBattleView, selfBattleCardView, isOnDraw, isSkipSelecting, isRecording))
		{
			return false;
		}
		if (isRecording)
		{
			return true;
		}
		BattlePlayerView battlePlayerView = (BattlePlayerView)selfPlayerBattleView;
		if (battlePlayerView.IsSelecting && !isSkipSelecting)
		{
			return false;
		}
		if (battlePlayerView.IsMoving())
		{
			return false;
		}
		if (isOnDraw)
		{
			return false;
		}
		if (selfPlayerBattleView.PlayQueueView.IsCardInQueue(selfBattleCardView))
		{
			return false;
		}
		return true;
	}
}
