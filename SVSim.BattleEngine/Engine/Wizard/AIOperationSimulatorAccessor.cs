using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Operation;

namespace Wizard;

public class AIOperationSimulatorAccessor
{
	private EnemyAI _ai;

	private List<BattleCardBase> _selectedSkillTargetList = new List<BattleCardBase>();

	public AIVirtualField CurrentField { get; private set; }

	public AIGenerateTagOwnerTable GenerateTagOwnerTable { get; private set; }

	public AIBattleInfoReceivedData BattleInfoReceiveDate { get; private set; } = new AIBattleInfoReceivedData();

	public AIOperationSimulatorAccessor(EnemyAI ai)
	{
		CurrentField = null;
		Initialize(ai);
	}

	public AIOperationSimulatorAccessor(EnemyAI ai, AIVirtualField field)
	{
		CurrentField = field;
		Initialize(ai);
	}

	private void Initialize(EnemyAI ai)
	{
		_ai = ai;
		GenerateTagOwnerTable = ai.GenerateTagOwnerTable.Clone();
	}

	public BattlePlayerPair CallPlay(BattlePlayerPair sourcePair, BattleCardBase playCardId, List<BattleCardBase> skillTargets, List<int> playPtn)
	{
		UpdateCurrentField(sourcePair, playPtn);
		Action<BattleCardBase> playCardSkillEvent = delegate(BattleCardBase card)
		{
			BattlePlayerPair pair = new BattlePlayerPair(card.SelfBattlePlayer, card.OpponentBattlePlayer);
			EnemyAIUtil.SetupPlayCardSkillOptionValue(card, pair);
		};
		SetUpBattleInfoReceiver();
		BattlePlayerPair battlePlayerPair = OperationSimulator.Play(sourcePair, playCardId, skillTargets, SetVirtualPairEvent, playCardSkillEvent);
		CleanUpBattleInfoReceiver();
		List<int> list = new List<int>();
		for (int num = 1; num < playPtn.Count; num++)
		{
			BattleCardBase oldCard = sourcePair.Self.HandCardList[playPtn[num]];
			BattleCardBase battleCardBase = battlePlayerPair.Self.HandCardList.FirstOrDefault((BattleCardBase c) => c.Index == oldCard.Index || c.BaseParameter.BaseCardId == oldCard.BaseParameter.BaseCardId);
			if (battleCardBase != null)
			{
				list.Add(battlePlayerPair.Self.HandCardList.IndexOf(battleCardBase));
			}
		}
		UpdateCurrentField(battlePlayerPair, list);
		return battlePlayerPair;
	}

	public BattlePlayerPair CallEvolve(BattlePlayerPair sourcePair, BattleCardBase evolutionCardId, List<BattleCardBase> skillTargets, List<int> playPtn)
	{
		UpdateCurrentField(sourcePair, playPtn);
		SetUpBattleInfoReceiver();
		BattlePlayerPair battlePlayerPair = OperationSimulator.Evolve(sourcePair, evolutionCardId, skillTargets, SetVirtualPairEvent);
		CleanUpBattleInfoReceiver();
		UpdateCurrentField(battlePlayerPair, playPtn);
		return battlePlayerPair;
	}

	public void UpdateCurrentField(BattlePlayerPair sourcePair, List<int> playPtn)
	{
		AIVirtualFieldBuildParameterCollction buildParameters = new AIVirtualFieldBuildParameterCollction(CurrentField);
		CurrentField = AIVirtualField.CreateTemporaryVirtualField(_ai, _ai.ParamQuery, _ai.StyleQuery, sourcePair, playPtn, buildParameters);
		GenerateTagOwnerTable.RegisterAllGenerateTagOwner(CurrentField);
		_selectedSkillTargetList.Clear();
	}

	public void SetVirtualPairEvent(BattlePlayerPair virtualPair)
	{
		AIAttachEventToBattleModuleUtility.SetupVirtualPairEventForOperationSimulator(virtualPair, this);
	}

	private void SetUpBattleInfoReceiver()
	{
		AIBattleInfoReceiver.GetInstance()?.SetUpOprSimAccessor(this);
	}

	private void CleanUpBattleInfoReceiver()
	{
		AIBattleInfoReceiver.GetInstance()?.CleanUpOprSimAccessor();
	}
}
