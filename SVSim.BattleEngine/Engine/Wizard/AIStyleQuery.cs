using System.Collections.Generic;

namespace Wizard;

public class AIStyleQuery
{
	private EnemyAI _enemyAI;

	private AIStyleData _deckStyle;

	private AIStyleData _curStyle;

	private List<AIPolicyData> _curPolicies;

	private List<AIPolicyData> _attachedPolicies;

	private AIPolicyCollectionContainer _policyCollections;

	public AIStyleQuery(EnemyAI ai, AIParamQuery _paramQuery)
	{
		_enemyAI = ai;
		_curStyle = new AIStyleData();
		_curPolicies = new List<AIPolicyData>();
	}

	public void SetDeckStyle(AIStyleData _style)
	{
		_deckStyle = _style;
	}

	public void UpdateStyle()
	{
		AIDataLibrary aIDataLibrary = null; // Pre-Phase-5b: no AI lib headless
		_curStyle = null; // Pre-Phase-5b: no AI style headless
		_curPolicies = _curStyle.ConvertToPolicyList();
		_policyCollections = new AIPolicyCollectionContainer();
		_policyCollections.InitializeAllCollections(_curPolicies);
		if (_attachedPolicies != null)
		{
			_policyCollections.InitializeAllCollections(_attachedPolicies);
		}
		InitializeReferenceIdTable();
		InitializeReferenceTribeTable();
	}

	public void ExecuteAttachStyle(AIPolicyData data)
	{
		if (_attachedPolicies == null)
		{
			_attachedPolicies = new List<AIPolicyData>();
		}
		_attachedPolicies.Add(data);
		_policyCollections.RegisterNewPolicy(data);
	}

	public float GetEpValue(AISituationInfo situation, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.EpValue))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.EpValue) as EpValuePolicyCollection).GetEpValue(situation, playPtn);
	}

	public float GetUnitRate(AIVirtualField field, AIVirtualCard card, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.ModUnitRate))
		{
			return 1f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.ModUnitRate) as ModUnitRatePolicyCollection).GetUnitRate(card, field, playPtn);
	}

	public float GetUnitBonus(AIVirtualField field, AIVirtualCard card, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.UnitBonus))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.UnitBonus) as UnitBonusPolicyCollection).GetUnitBonus(card, field, playPtn);
	}

	public float GetPlayptnBonus(AIVirtualField field, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.PlayptnBonus))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.PlayptnBonus) as PlayptnBonusPolicyCollection).GetPlayptnBonus(field, playPtn);
	}

	public float GetAllyPlayBonus(AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation, ref float currentUseMinValue)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.AllyPlayBonus))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.AllyPlayBonus) as AllyPlayBonusPolicyCollection).GetAllyPlayBonus(playCard, playPtn, situation, ref currentUseMinValue);
	}

	public float GetAllyPlayBonusRate(AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.AllyPlayBonusRate))
		{
			return 1f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.AllyPlayBonusRate) as AllyPlayBonusRatePolicyCollection).GetAllyPlayBonusRate(playCard, playPtn, situation);
	}

	public bool IsPlayBreak(AIVirtualCard card, List<int> playPtn, AISituationInfo situation)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.PlayBreak))
		{
			return false;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.PlayBreak) as PlayBreakPolicyCollection).IsPlayBreak(card, playPtn, situation);
	}

	public float GetBarrierBonus(AIVirtualCard card)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.BarrierBonus))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.BarrierBonus) as BarrierBonusPolicyCollection).GetBarrierBonus(card);
	}

	public bool IsDisableLethalCheck(AIVirtualField field, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.DisableLethalCheck))
		{
			return false;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.DisableLethalCheck) as DisableLethalCheckPolicyCollection).IsDisableLethalCheck(field, playPtn);
	}

	public float GetDelayTurnEndTime(AIVirtualCard owner, List<int> playPtn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.DelayTurnEndTime))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.DelayTurnEndTime) as AIDelayTurnEndTimePolicyCollection).GetDelayTime(owner, playPtn);
	}

	public int GetEmoteOnTurnEnd(bool isAllyTurn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.EmoOnTurnEnd))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.EmoOnTurnEnd) as EmoOnTurnEndPolicyCollection).GetEmoOnTurnEnd(_enemyAI.CurrentVirtualField, isAllyTurn);
	}

	public int GetEmoteOnTurnStart(bool isAllyTurn, AIVirtualField field)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.EmoOnTurnStart))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.EmoOnTurnStart) as EmoOnTurnStartPolicyCollection).GetEmoOnTurnStart(field, isAllyTurn);
	}

	public int GetEmoteOnLeaderDamaged(AIVirtualField field)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.EmoOnLeaderDamaged))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.EmoOnLeaderDamaged) as EmoOnLeaderDamagedPolicyCollection).GetEmoOnLeaderDamaged(field);
	}

	public int GetPlayerEmoteOnTurnEnd(bool isPlayerTurn)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.PlayerEmoOnTurnEnd))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.PlayerEmoOnTurnEnd) as PlayerEmoOnTurnEndPolicyCollection).GetPlayerEmoOnTurnEnd(_enemyAI.CurrentVirtualField, isPlayerTurn);
	}

	public int GetPlayerEmoteOnTurnStart(AIVirtualField field)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.PlayerEmoOnTurnStart))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.PlayerEmoOnTurnStart) as PlayerEmoOnTurnStartPolicyCollection).GetPlayerEmoOnTurnStart(field);
	}

	public int GetPlayerEmoteOnLeaderDamaged(AIVirtualField field)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.PlayerEmoOnLeaderDamaged))
		{
			return -1;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.PlayerEmoOnLeaderDamaged) as PlayerEmoOnLeaderDamagedPolicyCollection).GetPlayerEmoOnLeaderDamaged(field);
	}

	private void InitializeReferenceIdTable()
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.SetReferenceId))
		{
			_enemyAI.ReferenceIdTable = new Dictionary<int, int>();
			return;
		}
		SetReferenceIdPolicyCollection setReferenceIdPolicyCollection = _policyCollections.GetPolicyCollection(AIPolicyType.SetReferenceId) as SetReferenceIdPolicyCollection;
		_enemyAI.ReferenceIdTable = setReferenceIdPolicyCollection.CreateReferenceIdTable();
	}

	private void InitializeReferenceTribeTable()
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.SetReferenceTribe))
		{
			_enemyAI.ReferenceTribeTable = new Dictionary<string, List<int>>();
			return;
		}
		SetReferenceTribePolicyCollection setReferenceTribePolicyCollection = _policyCollections.GetPolicyCollection(AIPolicyType.SetReferenceTribe) as SetReferenceTribePolicyCollection;
		_enemyAI.ReferenceTribeTable = setReferenceTribePolicyCollection.CreateReferenceTribeTable();
	}

	public float GetFirstMoveBonus(AIVirtualCard playCard, List<AIVirtualActionInfo> moves)
	{
		if (!_policyCollections.HasPolicy(AIPolicyType.MoveFirstBonus))
		{
			return 0f;
		}
		return (_policyCollections.GetPolicyCollection(AIPolicyType.MoveFirstBonus) as MoveFirstBonusPolicyCollection).GetMoveFirstBonus(playCard, playCard.SelfField, moves);
	}

	public void ExecuteGameStartAttachTag(AIVirtualField field)
	{
		if (_policyCollections.HasPolicy(AIPolicyType.GameStartAttachTag))
		{
			(_policyCollections.GetPolicyCollection(AIPolicyType.GameStartAttachTag) as GameStartAttachTagPolicyCollection).ExecuteAttachTag(field);
		}
	}
}
