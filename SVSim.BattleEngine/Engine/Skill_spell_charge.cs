using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class Skill_spell_charge : SkillBase
{
	public static int SPELL_CHARGE_SUMMARY_COUNT = 10;

	public static readonly float SPELL_CHARGE_INTERVAL = 0.3f;

	public static readonly float SPELL_CHARGE_SUMMARY_INTERVAL = 0.4f;

	protected List<BattleCardBase> _targetCards;

	protected List<int> _addList;

	public override bool IsTargetIndicate => false;

	public int AddCount { get; private set; }

	public int DiffAddCount { get; private set; }

	public Skill_spell_charge(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_targetCards = new List<BattleCardBase>();
		_addList = new List<int>();
		AddCount = GetAddBoostCount();
		DiffAddCount = GetDiffBoostCount();
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer);
		IEnumerable<BattleCardBase> enumerable = parameter.targetCards.Where((BattleCardBase s) => s.IsInHand || s.IsInDeck);
		if (enumerable.Count() == 0 || (AddCount == 0 && DiffAddCount == 0))
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase item in enumerable)
		{
			if (DiffAddCount > 0)
			{
				AddCount = DiffAddCount - item.SpellChargeCount;
				if (AddCount <= 0)
				{
					continue;
				}
			}
			if (item.HasSpellCharge)
			{
				_targetCards.Add(item);
				_addList.Add(AddCount);
				parameter.skillProcessor.Register(item.Skills.CreateWhenSpellChargeInfo(parameter.skillProcessor, playerInfoPair, AddCount));
				if ((item.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch) && !item.IsInDeck)
				{
					parallelVfxPlayer.Register(item.GetSpellChargeLoopEffect(AddCount));
				}
			}
			else
			{
				item.AddSpellChargeCount(AddCount);
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, enumerable));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		VfxWithLoadingSequential vfxWithLoadingSequential2 = base.SkillPrm.selfBattlePlayer.AddSpellChargeCountVfx(_targetCards, _addList);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoadingSequential2.LoadingVfx);
		vfxWithLoadingSequential.RegisterToMainVfx(vfxWithLoadingSequential2.MainVfx);
		return vfxWithLoadingSequential;
	}

	public int GetAddBoostCount()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_charge, 0);
	}

	public int GetDiffBoostCount()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.diff_charge, 0);
	}
}
