using System;
using System.Collections.Generic;
using System.Linq;

public class RegisterActionManager
{
	private bool _isStopAdd;

	private int _nowEffectMovementCount;

	private NetworkBattleManagerBase _battleManager;

	public List<RegisterActionBase> RegisterDataList { get; private set; }

	public int TargetGroupNum { get; private set; }

	public BattleCardBase PlayActionCardBase { get; private set; }

	public List<BattleCardBase> BeforeOpponentHandCardList { get; private set; }

	public bool IsPlayActionEvol { get; private set; }

	public RegisterActionManager(NetworkBattleManagerBase mgr)
	{
		_battleManager = mgr;
		RegisterDataList = new List<RegisterActionBase>();
	}

	public void Add(RegisterActionBase data)
	{
		if (!_battleManager.IsVirtualBattle && !_isStopAdd)
		{
			RegisterDataList.Add(data);
			if (_nowEffectMovementCount != -1)
			{
				data.EffectSkillMovement = _nowEffectMovementCount;
			}
		}
	}

	public bool Any(Func<RegisterActionBase, bool> check)
	{
		return RegisterDataList.Any((RegisterActionBase s) => check(s));
	}

	public RegisterActionBase GetLast(Func<RegisterActionBase, bool> check)
	{
		return RegisterDataList.LastOrDefault((RegisterActionBase s) => check(s));
	}

	public RegisterActionBase Last()
	{
		if (RegisterDataList.Count > 0)
		{
			return RegisterDataList.Last();
		}
		return null;
	}

	public void Remove(RegisterActionBase data)
	{
		if (!_battleManager.IsVirtualBattle)
		{
			RegisterDataList.Remove(data);
		}
	}

	public void Clear()
	{
		if (!_battleManager.IsVirtualBattle)
		{
			TargetGroupNum = 0;
			RegisterDataList.Clear();
			PlayActionCardBase = null;
			_nowEffectMovementCount = -1;
		}
	}

	public void SetStopAdd(bool flag)
	{
		if (!_battleManager.IsVirtualBattle)
		{
			_isStopAdd = flag;
		}
	}

	public void AddTargetGroupeNum()
	{
		TargetGroupNum++;
	}

	public void SettingRecodeSkillCountData(SkillBase skill)
	{
		if (!_battleManager.IsVirtualBattle)
		{
			_nowEffectMovementCount = NetworkBattleGenericTool.GetSkillMovementNum(skill);
		}
	}

	public void SetPlayActionCardBase(BattleCardBase card, bool isEvol)
	{
		PlayActionCardBase = card;
		IsPlayActionEvol = isEvol;
	}

	public void SetBeforeOpponentHandCardList(BattleManagerBase mgr)
	{
		BeforeOpponentHandCardList = new List<BattleCardBase>(mgr.BattleEnemy.HandCardList);
	}

	public bool HasSkillConditionCheckCallCount(SkillBase skill)
	{
		for (int i = 0; i < RegisterDataList.Count; i++)
		{
			if (RegisterDataList[i] is RegisterSkillConditionCheck registerSkillConditionCheck && registerSkillConditionCheck.Skill == skill && registerSkillConditionCheck.ConditionType == RegisterSkillConditionCheck.SkillConditionType.callCount)
			{
				return true;
			}
		}
		return false;
	}
}
