using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class RegisterAttach : RegisterAlter
{
	private int _addAtk = -1;

	private int _addLife = -1;

	private int _setAtk = -1;

	private int _setLife = -1;

	private int _clan = -1;

	private int _tribe = -1;

	private string _tribeChangeType = string.Empty;

	private string _other = string.Empty;

	private bool _isBoard;

	private bool _isDelete;

	private bool _isForHandResident;

	public RegisterAttach(List<BattleCardBase> cardList, SkillBase skill, bool isDelete = false)
		: base(cardList, skill)
	{
		RegisterAttach registerAttach = this;
		base.IndexList = new List<int>();
		cardList.ForEach(delegate(BattleCardBase x)
		{
			registerAttach.IndexList.Add(x.Index);
		});
		if (skill is Skill_powerup)
		{
			Skill_powerup skill_powerup = skill as Skill_powerup;
			_addAtk = skill_powerup.GetAddOffense();
			_addLife = skill_powerup.GetAddLife();
		}
		else if (skill is Skill_power_down)
		{
			Skill_power_down skill_power_down = skill as Skill_power_down;
			_setAtk = skill_power_down.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_offense, -1);
			_setLife = skill_power_down.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_life, -1);
		}
		else if (skill is Skill_change_affiliation)
		{
			Skill_change_affiliation skill_change_affiliation = skill as Skill_change_affiliation;
			if (!isDelete)
			{
				if (skill_change_affiliation.GetClanType() != CardBasePrm.ClanType.NONE)
				{
					_clan = (int)skill_change_affiliation.GetClanType();
				}
				CardBasePrm.TribeChangeType tribeChangeType = skill_change_affiliation.CreateTribeChangeType();
				_tribeChangeType = ((tribeChangeType == CardBasePrm.TribeChangeType.CHANGE) ? "s" : "a");
				List<CardBasePrm.TribeType> list = skill_change_affiliation.CreateChangeTribeList();
				if (list != null && list.Count() > 0)
				{
					_tribe = (int)list[0];
				}
			}
			if (cardList.Any((BattleCardBase s) => s.IsInplay))
			{
				_isBoard = true;
			}
			if (skill_change_affiliation.ApplyCardFilterList.Any((ISkillCardFilter s) => s is SkillParameterCostFilter) && skill_change_affiliation.ApplyingTargetFilter is SkillTargetHandFilter)
			{
				_isForHandResident = true;
			}
		}
		else if (skill is Skill_attach_skill)
		{
			Skill_attach_skill skill_attach_skill = skill as Skill_attach_skill;
			_other = (skill_attach_skill.AttachWhenDestroy ? "l" : "o");
			NetworkBattleManagerBase networkBattleMgr = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
			Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> value = delegate
			{
				RegisterAttach data = new RegisterAttach(cardList, skill, isDelete: true);
				networkBattleMgr.RegisterActionManager.Add(data);
				return NullVfx.GetInstance();
			};
			_skill.OnSkillStopEnd -= value;
			_skill.OnSkillStopEnd += value;
		}
		else
		{
			_other = "o";
		}
		IsSelf = cardList[0].IsPlayer;
		_skill = skill;
		_isDelete = isDelete;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(ActionBaseParameter.type.ToString(), _isDelete ? ActionBaseParameter.del.ToString() : ActionBaseParameter.add.ToString());
		if (_addAtk >= 1)
		{
			dictionary.Add(ActionBaseParameter.atk.ToString(), "a+" + _addAtk);
		}
		if (_addLife >= 1)
		{
			dictionary.Add(ActionBaseParameter.life.ToString(), "a+" + _addLife);
		}
		if (_setAtk >= 1)
		{
			dictionary.Add(ActionBaseParameter.atk.ToString(), "s+" + _setAtk);
		}
		if (_setLife >= 1)
		{
			dictionary.Add(ActionBaseParameter.life.ToString(), "s+" + _setLife);
		}
		if (_clan >= 0)
		{
			dictionary.Add(ActionBaseParameter.clan.ToString(), _clan.ToString());
		}
		if (_tribe != -1)
		{
			dictionary.Add(ActionBaseParameter.tribe.ToString(), _tribeChangeType + _tribe);
		}
		if (_other != string.Empty)
		{
			dictionary.Add(ActionBaseParameter.other.ToString(), _other);
		}
		if (_isBoard)
		{
			dictionary.Add(ActionBaseParameter.isBoard.ToString(), 1);
		}
		if (_isForHandResident)
		{
			dictionary.Add(ActionBaseParameter.forHandResident.ToString(), 1);
		}
		if (_skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase networkBattleManagerBase && networkBattleManagerBase.RegisterActionManager.RegisterDataList.Any((RegisterActionBase r) => r is RegisterMetamorphoseData) && _skill.OnWhenDrawOtherStart != 0 && _skill.IsHaveApplicableTargetFilter<SkillTargetHandFilter>())
		{
			dictionary.Add(ActionBaseParameter.isForce.ToString(), 1);
		}
		return MakeAttachTarget(dictionary);
	}

	private static bool IsPrivateBuffFilter(ISkillTargetFilter targetFilter, bool isOnWhenFusion, bool isOnWhenDraw, bool isOnWhenReturn)
	{
		if (!(targetFilter is SkillTargetHandFilter) && !(targetFilter is SkillTargetDeckFilter) && !(targetFilter is SkillTargetHandOtherSelfFilter) && !(targetFilter is SkillTargetLastTargetFilter) && !(targetFilter is SkillTargetSkillDrewCardFilter) && !(targetFilter is SkillTargetTokenDrawCardFilter) && !(targetFilter is SkillTargetReturnCardFilter) && !(targetFilter is SkillTargetSkillUpdateDeckCardFilter) && (!isOnWhenFusion || !(targetFilter is SkillTargetSelfFilter)) && (!isOnWhenDraw || !(targetFilter is SkillTargetHandSelfFilter)))
		{
			if (isOnWhenReturn)
			{
				return targetFilter is SkillTargetSelfFilter;
			}
			return false;
		}
		return true;
	}

	public static bool IsPrivateBuffChangeCard(SkillBase skill)
	{
		if (!(skill is Skill_powerup) && !(skill is Skill_power_down))
		{
			return false;
		}
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				if (IsPrivateBuffFilter(skill.ApplyAndFilter[i].TargetFilter, skill.OnWhenFusion != 0, skill.OnWhenDraw != 0, skill.OnWhenReturnStart != 0))
				{
					return true;
				}
			}
		}
		else if (IsPrivateBuffFilter(skill.ApplyingTargetFilter, skill.OnWhenFusion != 0, skill.OnWhenDraw != 0, skill.OnWhenReturnStart != 0))
		{
			return true;
		}
		return false;
	}

	public static bool IsAffiliationChangeCard(SkillBase skill)
	{
		if (!(skill is Skill_change_affiliation))
		{
			return false;
		}
		return true;
	}

	public static bool IsAttachUnapprovedCard(SkillBase skill)
	{
		if (!(skill is Skill_attach_skill) && !(skill is Skill_attack_by_life))
		{
			return false;
		}
		if (!NetworkBattleGenericTool.IsUnapprovedTarget(skill) && !(skill.ApplyingTargetFilter is SkillTargetSelectedCardsFilter))
		{
			return false;
		}
		return true;
	}
}
