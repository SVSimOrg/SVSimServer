using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class SpellBattleCard : BattleCardBase
{
	private bool _isActionCard;

	private static SkillCreator.SkillBuildInfo _sharedSpellChargeBuildInfo;

	public override bool IsActionCard => _isActionCard;

	public override bool IsEvolution => false;

	public override bool IsInplay => false;

	public override bool IsSpell => true;

	public override bool IsDead => false;

	public override bool Movable(bool isCheckOnDraw = true, bool isSkipSelecting = false, CHECK_CONDITION_MUTATIONSKILL_TYPE type = CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE, bool isRecording = false)
	{
		if (!base.Movable(isCheckOnDraw, isSkipSelecting, CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE, isRecording))
		{
			return false;
		}
		return IsSelectableSkillTarget();
	}

	public bool IsSelectableSkillTarget()
	{
		bool flag = false;
		IEnumerable<SkillBase> source = base.Skills.Skip(1);
		IEnumerable<SkillBase> source2 = source.Where((SkillBase s) => s.IsWhenPlaySkill && s.IsUserSelectType && !s.IsEmptyHandedUserSelectType);
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SelfBattlePlayer, base.OpponentBattlePlayer);
		List<SkillBase> source3 = new List<SkillBase>();
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		if (source2.Any())
		{
			source3 = source2.Where((SkillBase s) => s.CheckCondition(playerInfoPair, checkerOption, isPrePlay: true)).ToList();
			if (source3.Count() <= 0)
			{
				flag = false;
			}
		}
		bool flag2 = base.Skills.CheckWhenPlayCondition(playerInfoPair, isPrePlay: true);
		if (flag2)
		{
			if (source3.Any())
			{
				int targetCount = 0;
				flag = source3.All((SkillBase s) => s.CalcApplyTargets(s.SkillPrm.CreateInfoPair(), checkerOption, ref targetCount, isCheckInHand: true).Any());
			}
		}
		else if (!flag2 && source2.Any())
		{
			flag = false;
		}
		if (!flag && source.Any((SkillBase s) => !s.IsUserSelectType && s.CheckCondition(playerInfoPair, checkerOption, isPrePlay: true) && s.ConditionCheckerList.Any((ISkillConditionChecker c) => c is SkillPreprocessDontSelectStart)))
		{
			return true;
		}
		if (source.Any((SkillBase s) => s.IsUserSelectType && !s.IsEmptyHandedUserSelectType && !(s is Skill_fusion)))
		{
			return flag;
		}
		return true;
	}

	public SpellBattleCard(BuildInfo buildInfo, bool isChoiceBrave)
		: base(buildInfo)
	{
		if (!isChoiceBrave)
		{
			SkillBase skill = CreateSkillCreator(buildInfo.SelfBattlePlayer, buildInfo.OpponentBattlePlayer, buildInfo.ResourceMgr).Create(SpellSkillInfoCreate());
			_normalSkillCollection.FirstAdd(skill);
		}
		_isActionCard = false;
	}

	public static SkillCreator.SkillBuildInfo SpellSkillInfoCreate()
	{
		if (_sharedSpellChargeBuildInfo == null)
		{
			_sharedSpellChargeBuildInfo = new SkillCreator.SkillBuildInfo("spell_charge", "when_play", "card_type=spell", "character=me&target=hand_other_self&card_type=all", "add_charge=1", "none");
		}
		return _sharedSpellChargeBuildInfo;
	}

	public override void UpdateSkillCollection()
	{
	}

	protected override SkillCollectionBase CreateSkillCollection()
	{
		return new SpellSkillCollection(this);
	}

	protected override VfxBase StartPlayCard()
	{
		VfxBase vfxBase = base.StartPlayCard();
		base.BattleCardView.HideCanPlayEffect();
		_isActionCard = true;
		return SequentialVfxPlayer.Create(vfxBase, NullVfx.GetInstance());
	}


	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		if (isNullView)
		{
			return new NullBattleCardView(buildInfo);
		}
		return new SpellBattleCardView(buildInfo);
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer)
	{
		VirtualSpellBattleCard virtualSpellBattleCard = new VirtualSpellBattleCard(_buildInfo.VirtualClone(virtualSelfBattlePlayer, virtualOpponentBattlePlayer), isChoiceBrave: false);
		virtualSpellBattleCard._isActionCard = _isActionCard;
		CopyToVirtualCardBase(virtualSpellBattleCard);
		return virtualSpellBattleCard;
	}
}
