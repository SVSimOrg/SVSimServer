namespace Wizard;

public interface IAIEmoteCtrl
{
	void SetUpEmoteEvent(BattlePlayerBase self, BattlePlayerBase opponent, OperateMgr operateManager);

	AIEmoteCmd OnOpponentEmotion(ClassCharaPrm.EmotionType emoteType);

	AIEmoteCmd OnAllyTurnStart(AISituationInfo situation);

	AIEmoteCmd OnOpponentTurnStart(AISituationInfo situation);

	AIEmoteCmd OnAllyTurnEnd();

	AIEmoteCmd OnOpponentTurnEnd();

	AIEmoteCmd OnCardPlay(AISituationInfo situation);

	AIEmoteCmd OnCardDestroy(AISituationInfo situation);

	AIEmoteCmd OnAllyEvolution(AISituationInfo situation);
}
