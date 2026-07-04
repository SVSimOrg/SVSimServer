namespace Wizard;

public class AIEmoteCtrlNull : IAIEmoteCtrl
{
	public void SetUpEmoteEvent(BattlePlayerBase self, BattlePlayerBase opponent, OperateMgr operateManager)
	{
	}

	public AIEmoteCmd OnOpponentEmotion(ClassCharaPrm.EmotionType emoteType)
	{
		return null;
	}

	public AIEmoteCmd OnAllyTurnStart(AISituationInfo situation)
	{
		return null;
	}

	public AIEmoteCmd OnAllyTurnEnd()
	{
		return null;
	}

	public AIEmoteCmd OnOpponentTurnStart(AISituationInfo situation)
	{
		return null;
	}

	public AIEmoteCmd OnOpponentTurnEnd()
	{
		return null;
	}

	public AIEmoteCmd OnCardPlay(AISituationInfo situation)
	{
		return null;
	}

	public AIEmoteCmd OnCardDestroy(AISituationInfo situation)
	{
		return null;
	}

	public AIEmoteCmd OnAllyEvolution(AISituationInfo situation)
	{
		return null;
	}
}
