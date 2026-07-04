using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public interface IEnemyAI
{
	bool IsStackAction { get; }

	bool IsConnectNetwork { get; }

	bool IsAIExecution { get; }

	void ExecuteEnemyAI(bool useWait);

	void StopEnemyAI();

	void InitOnGame(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer);

	void Mulligan(List<BattleCardBase> dstList, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer);

	void TurnEnd();

	void Retire();

	void Disconnect();

	void Reconnect();

	void SelectSkillTarget(AIVirtualCard actCard, AIOperationType operationType, AISinglePlayptnRecord playptnRecord);

	void CleanupStackedAction();

	float CalcFieldAdvantage();

	IAIEmoteCtrl EmoteCtrl();

	bool SetUpBattleState(int classId, AI_LOGIC_LV logicLv, string deckName, string styleName, string emoteName, int enemyAiID = -1);

	VfxBase GetEmote(AIEmoteCmdType cmdType, AISituationInfo situation = null, ClassCharaPrm.EmotionType receivedEmoteType = ClassCharaPrm.EmotionType.NULL, int emoteInput = -1);
}
