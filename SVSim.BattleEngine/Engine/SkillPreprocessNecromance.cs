using System;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessNecromance : SkillPreprocessBase
{
	private readonly int _necromanceCount;

	private readonly string _operator;

	private string _countText = string.Empty;

	private SkillBase _skill;

	public bool IsApproximate => _operator == "<:=";

	public SkillPreprocessNecromance(SkillBase skill, string countText, string op)
	{
		if (SkillOptionValue.IsVariableValue(countText))
		{
			_countText = countText;
		}
		else
		{
			_necromanceCount = int.Parse(countText);
		}
		_operator = op;
		_skill = skill;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return playerInfoPair.ReadOnlySelf.SkillInfoCemeterys.Count() >= NecromanceCount(playerInfoPair.ReadOnlySelf.SkillInfoCemeterys.Count(), playerInfoPair, option);
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattleLogManager.GetInstance().AddNecromanceIcon(skill, skill.SkillPrm.ownerCard, skill.SkillPrm.ownerCard.IsSpell);
		bool num = skill.SkillPrm.ownerCard.GetSelectTypeSkill().Any((SkillBase s) => s.IsChoiceType);
		bool flag = skill.SkillPrm.ownerCard.Skills.Any((SkillBase s) => s is Skill_transform);
		if (num && flag)
		{
			return NullVfx.GetInstance();
		}
		int num2 = NecromanceCount(playerPair.Self.CemeteryList.Count, playerPair, checkerOption);
		playerPair.Self.CemeteryConsumption(num2, skill.SkillPrm.ownerCard, skillProcessor, skill.OnWhenFusion != 0);
		if (!skill.SkillPrm.ownerCard.IsSpell && (skill.OnWhenFusion == 0 || skill.SkillPrm.ownerCard.IsPlayer || skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public int NecromanceCount(int cemeteryCount, BattlePlayerReadOnlyInfoPair pair = null, SkillConditionCheckerOption checkerOption = null)
	{
		if (IsApproximate)
		{
			return Math.Max(0, Math.Min(_necromanceCount, cemeteryCount));
		}
		if (_countText != string.Empty)
		{
			if (pair == null)
			{
				pair = new BattlePlayerReadOnlyInfoPair(_skill.SkillPrm.selfBattlePlayer, _skill.SkillPrm.opponentBattlePlayer);
			}
			if (checkerOption == null)
			{
				checkerOption = new SkillConditionCheckerOption();
			}
			SkillCollectionBase.SetupOptionValue(_skill.OptionValue, pair, _skill.SkillPrm.ownerCard, _skill, checkerOption);
			return _skill.OptionValue.ParseInt(_countText);
		}
		return _necromanceCount;
	}
}
