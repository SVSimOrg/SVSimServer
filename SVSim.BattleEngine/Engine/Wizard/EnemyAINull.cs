using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class EnemyAINull : IEnemyAI
{
	public bool IsStackAction => false;

	public bool IsConnectNetwork => false;

	public bool IsAIExecution => false;

	public void ExecuteEnemyAI(bool useWait)
	{
	}

	public void StopEnemyAI()
	{
	}

	public void InitOnGame(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
	}

	public void Mulligan(List<BattleCardBase> dstList, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
	}

	public void SelectSkillTarget(AIVirtualCard actCard, AIOperationType operationType, AISinglePlayptnRecord playptnRecord)
	{
	}

	public float CalcFieldAdvantage()
	{
		return 0f;
	}

	public IAIEmoteCtrl EmoteCtrl()
	{
		return new AIEmoteCtrlNull();
	}

	public void TurnEnd()
	{
	}

	public void Retire()
	{
	}

	public void Disconnect()
	{
	}

	public void Reconnect()
	{
	}

	public void CleanupStackedAction()
	{
	}

	public bool SetUpBattleState(int classId, AI_LOGIC_LV logicLv, string deckName, string styleName, string emoteName, int enemyAiID = -1)
	{
		return true;
	}

	public VfxBase GetEmote(AIEmoteCmdType cmdType, AISituationInfo situation = null, ClassCharaPrm.EmotionType receivedEmoteType = ClassCharaPrm.EmotionType.NULL, int emoteInput = -1)
	{
		return NullVfx.GetInstance();
	}
}
