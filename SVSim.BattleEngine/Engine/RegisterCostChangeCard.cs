using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class RegisterCostChangeCard : RegisterAlter
{
	public enum CostChangeType
	{
		addCost,
		setCost,
		halfCost,
		halfCostRoundDown
	}

	public int CostVal;

	private Dictionary<string, object> _data;

	private RegisterActionManager _registerActionManager;

	private BattleManagerBase _mgr;

	private ICardCostModifier _costModifier;

	public CostChangeType CostChangeTypeVal { get; private set; }

	public RegisterCostChangeCard(BattleManagerBase mgr, RegisterActionManager registerActionManager, ICardCostModifier costModifier, List<BattleCardBase> cards, SkillBase skill, SkillConditionCheckerOption option, RegisterTargetBase registerTarget = null, bool isNotCheckCard = false)
		: base(cards, skill)
	{
		RegisterCostChangeCard registerCostChangeCard = this;
		if (!isNotCheckCard && cards == null && skill == null)
		{
			return;
		}
		base.IndexList = new List<int>();
		if (cards == null)
		{
			base.IndexList.Add(-1);
		}
		else
		{
			cards.ForEach(delegate(BattleCardBase x)
			{
				if (!registerCostChangeCard.IndexList.Any((int z) => z == x.Index))
				{
					registerCostChangeCard.IndexList.Add(x.Index);
				}
			});
			if (cards.Count() == 0)
			{
				IsSelf = false;
			}
			else
			{
				IsSelf = cards.First().IsPlayer;
			}
		}
		if (isNotCheckCard)
		{
			IsSelf = false;
		}
		_registerActionManager = registerActionManager;
		_mgr = mgr;
		_costModifier = costModifier;
		if (_costModifier != null)
		{
			SettingCostChangeType(_costModifier);
		}
		if (registerTarget != null)
		{
			registerTarget.SettingGroupIndexMsg(this);
		}
		if (_data == null)
		{
			_data = base.MakeSendData();
			_data.Add(ActionBaseParameter.type.ToString(), ActionBaseParameter.add.ToString());
			switch (CostChangeTypeVal)
			{
			case CostChangeType.addCost:
				_data.Add(ActionBaseParameter.cost.ToString(), "a" + CostVal);
				break;
			case CostChangeType.setCost:
				_data.Add(ActionBaseParameter.cost.ToString(), "s" + CostVal);
				break;
			case CostChangeType.halfCost:
				_data.Add(ActionBaseParameter.cost.ToString(), "d" + CostVal);
				break;
			case CostChangeType.halfCostRoundDown:
				_data.Add(ActionBaseParameter.cost.ToString(), "D" + CostVal);
				break;
			}
			if (registerActionManager.RegisterDataList.Any((RegisterActionBase r) => r is RegisterMetamorphoseData))
			{
				List<RegisterActionBase> registerMetamorphoses = registerActionManager.RegisterDataList.Where((RegisterActionBase r) => r is RegisterMetamorphoseData).ToList();
				int i;
				for (i = 0; i < registerMetamorphoses.Count(); i++)
				{
					if (base.IndexList.Any((int index) => index != -1 && index == (registerMetamorphoses[i] as RegisterMetamorphoseData).Index) && IsSelf == registerMetamorphoses[i].IsSelf)
					{
						_data.Add(ActionBaseParameter.isForce.ToString(), 1);
						break;
					}
				}
			}
			_data = MakeAttachTarget(_data);
			Dictionary<string, object> data = new Dictionary<string, object>(_data);
			Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> value = delegate(SkillBase _skill, List<BattleCardBase> _cards, SkillConditionCheckerOption _checkerOption, SkillProcessor _skillProcessor)
			{
				bool flag = _skill.SkillPrm.ownerCard.IsPlayer;
				if (_skill.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter)
				{
					flag = !flag;
				}
				RegisterFilter registerFilter = new RegisterFilter(registerCostChangeCard._registerActionManager, registerCostChangeCard._mgr, flag, _skill, _cards, isStop: true, option);
				registerCostChangeCard._registerActionManager.Add(registerFilter);
				RegisterCostChangeCard registerCostChangeCard2 = new RegisterCostChangeCard(registerCostChangeCard._mgr, registerCostChangeCard._registerActionManager, registerCostChangeCard._costModifier, null, null, option);
				List<string> groupMsgList = registerFilter.GetGroupMsgList();
				if (registerTarget != null && groupMsgList != null && groupMsgList.Count > 0)
				{
					data[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx]] = groupMsgList[0];
				}
				registerCostChangeCard2.SetSendData(data);
				registerCostChangeCard2.SetCostChangeType(ActionBaseParameter.del.ToString());
				registerCostChangeCard._registerActionManager.Add(registerCostChangeCard2);
				return NullVfx.GetInstance();
			};
			_skill.OnSkillStopEnd -= value;
			_skill.OnSkillStopEnd += value;
			_skill.OnSkillStopEnd -= ((NetworkBattleManagerBase)_mgr)._networkBattleSetupCardEventBase.Event_RegisterFilterSkillEnd;
			_skill.OnSkillStopEnd += ((NetworkBattleManagerBase)_mgr)._networkBattleSetupCardEventBase.Event_RegisterFilterSkillEnd;
		}
		if (skill is NetworkSkill_cost_change { IsForHandResident: not false })
		{
			_data.Add("forHandResident", 1);
		}
	}

	public void SetCostChangeType(string type)
	{
		_data[ActionBaseParameter.type.ToString()] = type;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		return _data;
	}

	private void SettingCostChangeType(ICardCostModifier costModifiers)
	{
		if (costModifiers is CostAddModifier)
		{
			CostAddModifier costAddModifier = costModifiers as CostAddModifier;
			if (costAddModifier.Cost != 0)
			{
				CostVal = costAddModifier.Cost;
				CostChangeTypeVal = CostChangeType.addCost;
			}
		}
		else if (costModifiers is CostSetModifier)
		{
			int cost = (costModifiers as CostSetModifier).Cost;
			CostVal = cost;
			CostChangeTypeVal = CostChangeType.setCost;
		}
		else if (costModifiers is CostHalfRoundUpModifier)
		{
			CostVal = 2;
			CostChangeTypeVal = CostChangeType.halfCost;
		}
		else if (costModifiers is CostHalfRoundDownModifier)
		{
			CostVal = 2;
			CostChangeTypeVal = CostChangeType.halfCostRoundDown;
		}
	}

	private void SetSendData(Dictionary<string, object> data)
	{
		_data = data;
	}
}
