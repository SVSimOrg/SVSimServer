using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_draw : SkillBase
{

	private bool _isActiveChangeShortageDeck;

	public override bool IsAllowDestroyTarget => true;

	public override bool IsVisibleTarget { get; protected set; }

	public bool IsActiveChangeShortageDeck => _isActiveChangeShortageDeck;

	public Skill_draw(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_isActiveChangeShortageDeck = false;
		List<BattleCardBase> list = parameter.targetCards.ToList();
		IsVisibleTarget = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.is_open, string.Empty) == "true";
		BattlePlayerBase battlePlayerBase = ((!(base.ApplyBattlePlayerFilter is SelfBattlePlayerFilter)) ? base.SkillPrm.opponentBattlePlayer : base.SkillPrm.selfBattlePlayer);
		bool isPlayer = battlePlayerBase.IsPlayer;
		bool flag = false;
		flag = base.ApplyCardFilterList.Where((ISkillCardFilter f) => f.GetType() != typeof(SkillNullFilter) && f.GetType() != typeof(SkillAllCardFilter)).Count() < 1 && battlePlayerBase.CheckShortageDeck(base.ApplySelectFilter.CalcCount(base.OptionValue), parameter.skillProcessor, out _isActiveChangeShortageDeck);
		SkillProcessor skillProcessor = parameter.skillProcessor;
		bool isVisibleTarget = IsVisibleTarget;
		SkillResultInfo calledSkillResultInfo = parameter.calledSkillResultInfo;
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = battlePlayerBase.DrawManagement(list, skillProcessor, isVisibleTarget, flag, calledSkillResultInfo, this);
		if (vfxWith.Value.Count() <= 0 && !flag)
		{
			return NullVfxWithLoading.GetInstance();
		}
		List<BattleCardBase> list2 = list.Where((BattleCardBase s) => s.IsInHand).ToList();
		for (int num = 0; num < list2.Count(); num++)
		{
			base.SkillPrm.ownerCard.SkillApplyInformation.AddSkillDrewCard(list2.ElementAt(num));
			BattleLogManager.GetInstance().UpdateFusionedCardSkillDrewCard(base.SkillPrm.ownerCard);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDrawCard(list2, this, IsVisibleTarget, isPlayer, isOverDraw: false);
			BattleLogManager.GetInstance().AddLogSkillDrawCard(list.Where((BattleCardBase s) => !s.IsInHand).ToList(), this, IsVisibleTarget, isPlayer, isOverDraw: true);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToMainVfx(vfxWith.Vfx);
		return vfxWithLoadingSequential;
	}
}
