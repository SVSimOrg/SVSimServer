using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cant_attack : SkillBase
{
	private class CantAttack
	{
		public BattleCardBase _target { get; set; }

		public int _cantClass { get; set; }

		public int _cantUnit { get; set; }

		public int _cantUnitNotHasGuard { get; set; }

		public BuffInfo _buffInfo { get; set; }

		public CantAttack(BattleCardBase target, int cantClass, int cantUnit, int cantUnitNotHasGuard, BuffInfo buffInfo)
		{
			_target = target;
			_cantClass = cantClass;
			_cantUnit = cantUnit;
			_cantUnitNotHasGuard = cantUnitNotHasGuard;
			_buffInfo = buffInfo;
		}
	}

	public static readonly int BIT_FLAG_NULL = 0;

	public static readonly int BIT_FLAG_CLASS = 1;

	public static readonly int BIT_FLAG_UNIT = 2;

	public static readonly int BIT_FLAG_UNIT_AND_CLASS = 3;

	public static readonly int BIT_UNIT_NOT_HAS_GUARD = 8;

	private int _baseCardId;

	public Skill_cant_attack(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int num = BIT_FLAG_NULL;
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.cant_attack);
		_baseCardId = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.base_card_id, -1);
		CantAttackType type = CantAttackType.Null;
		switch (text)
		{
		case "class":
			num = BIT_FLAG_CLASS;
			type = CantAttackType.Class;
			break;
		case "unit":
			num = BIT_FLAG_UNIT;
			type = CantAttackType.Unit;
			break;
		case "all":
			num = BIT_FLAG_UNIT_AND_CLASS;
			type = CantAttackType.All;
			break;
		case "unit_not_has_guard":
			num = BIT_UNIT_NOT_HAS_GUARD;
			type = CantAttackType.NotHasGuard;
			break;
		}
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, num, "", null, 0L);
			parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCantAttack(num, _baseCardId));
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillCantAttack(parameter.targetCards.ToList(), this, type);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(base.Stop(skillProcessor));
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveCantAttack(item._intValue, _baseCardId));
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
		}
		CallOnUpdateSkillEffect(list, updateAttackEffect: true);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			VfxBase result = card.SkillApplyInformation.ForceDepriveCantAttack();
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return result;
		};
	}
}
