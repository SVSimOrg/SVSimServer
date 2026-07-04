using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public abstract class SkillBaseCopy : SkillBase
{
	public string SkillType = "";

	public override bool IsAllowDestroyTarget => true;

	public List<SkillBase> CopiedSkillList { get; private set; } = new List<SkillBase>();

	public SkillBaseCopy(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public abstract bool IsRemain();

	public override VfxWithLoading Start(CallParameter parameter)
	{
		SkillType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.ability, "NONE");
		bool isRemain = IsRemain();
		BattleCardBase ownerCard = base.SkillPrm.ownerCard;
		if (SkillType == "NONE")
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (IsSkillMasterCopy(SkillType))
			{
				parallelVfxPlayer.Register(CopyMaster(ownerCard, targetCard, SkillType, isRemain));
			}
			else
			{
				parallelVfxPlayer.Register(CopySkill(ownerCard, targetCard, SkillType, isRemain));
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		return vfxWithLoadingSequential;
	}

	private VfxBase CopyMaster(BattleCardBase owner, BattleCardBase target, string skillType, bool isRemain)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattleCardBase targetCard = target;
		IEnumerable<BattleCardBase> previousCopiedCardList = owner.GetCopiedCardList;
		SkillBase skillBase = target.NormalSkills.Where((SkillBase s) => s is SkillBaseCopy).FirstOrDefault();
		BattleCardBase.CopySkillInfo copySkillInfo = base.SkillPrm.ownerCard.CopySkill(targetCard, skillType, isRemain);
		parallelVfxPlayer.Register(copySkillInfo.Vfx);
		if (!isRemain)
		{
			if (!targetCard.Skills.Any((SkillBase s) => s.SkillPrm.buildInfo._previousSkillOwner != null))
			{
				targetCard.RemoveBuffInfo((BuffInfo b) => b.IsCopied);
			}
			BuffInfo buffLose = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoLose = new BuffInfoContainer(targetCard, buffLose, -1, "", null, 0L);
			copySkillInfo.NewCopySkill.buffInfoContainer.Add(buffInfoLose);
			targetCard.OnRemoveFromInPlayAfterOneTime += delegate
			{
				targetCard.RemoveBuffInfo(buffLose);
				copySkillInfo.NewCopySkill.buffInfoContainer.Remove(buffInfoLose);
				return NullVfx.GetInstance();
			};
		}
		CopiedSkillList.AddRange(copySkillInfo.CopiedSkillList);
		IEnumerable<BattleCardBase> source = owner.GetCopiedCardList.Where((BattleCardBase b) => !previousCopiedCardList.Contains(b));
		if (source.Any())
		{
			BattleCardBase previousOwner = source.First();
			BuffInfo buffCopy = AddBuffInfoIfNeeded(owner, previousOwner);
			buffCopy.IsCopied = true;
			buffCopy.SetPreviousOwner(previousOwner);
			BuffInfoContainer buffInfoContainer = skillBase?.GetBuffInfoContainer().FirstOrDefault((BuffInfoContainer b) => b._buffInfo.PreviousOwner != null && b._buffInfo.PreviousOwner.CardId == previousOwner.CardId);
			if (previousOwner.CardId != target.CardId && skillBase != null && buffInfoContainer != null)
			{
				buffCopy.IsCopiedEvolutionSkill = buffInfoContainer._buffInfo.IsCopiedEvolutionSkill;
			}
			else
			{
				buffCopy.IsCopiedEvolutionSkill = copySkillInfo.IsEvolutionSkill;
			}
			BuffInfoContainer buffInfoRob = new BuffInfoContainer(owner, buffCopy, -1, "", null, 0L);
			copySkillInfo.NewCopySkill.buffInfoContainer.Add(buffInfoRob);
			buffCopy.IsPlayer = previousOwner.IsPlayer;
			owner.OnRemoveFromInPlayAfterOneTime += delegate
			{
				owner.RemoveBuffInfo(buffCopy);
				copySkillInfo.NewCopySkill.buffInfoContainer.Remove(buffInfoRob);
				return NullVfx.GetInstance();
			};
			owner.BuffInfoList.RemoveAll((BuffInfo b) => copySkillInfo.AttachBuffs.Contains(b));
			owner.BuffInfoList.AddRange(copySkillInfo.AttachBuffs);
		}
		if (IsBattleLog && (copySkillInfo.AttachBuffs.Any() || copySkillInfo.CopiedSkillList.Any()))
		{
			BattleLogManager.GetInstance().AddLogCopiedSkill(target, this, isRemain);
		}
		return parallelVfxPlayer;
	}

	private bool IsBuffSkill<SkillType>(SkillBase skill)
	{
		if (skill is SkillType || IsCopySkill(skill, this.SkillType))
		{
			return true;
		}
		if (skill is Skill_attach_skill skill_attach_skill && skill_attach_skill.GetAttachSkills().Any((SkillBase s) => s is SkillType))
		{
			return true;
		}
		return false;
	}

	private bool IsBaseSkill<SkillType>(SkillBase skill)
	{
		if (!(skill is SkillType))
		{
			return false;
		}
		if (!base.IsAllResidentTiming)
		{
			return false;
		}
		if (!(skill.ApplyingTargetFilter is SkillTargetSelfFilter))
		{
			return false;
		}
		return true;
	}

	private VfxBase CopySkill(BattleCardBase owner, BattleCardBase target, string skillType, bool isRemain)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		_ = owner.GetCopiedCardList;
		target.NormalSkills.Where((SkillBase s) => s is SkillBaseCopy).FirstOrDefault();
		bool flag = false;
		BuffInfo buffInfo = null;
		SkillBase skillBase = null;
		switch (skillType)
		{
		case "drain":
			if (target.SkillApplyInformation.IsDrain)
			{
				parallelVfxPlayer.Register(owner.SkillApplyInformation.GiveDrain());
				flag = true;
				buffInfo = target.BuffInfoList.FirstOrDefault((BuffInfo b) => IsBuffSkill<Skill_drain>(b.SkillFrom));
				skillBase = target.Skills.FirstOrDefault((SkillBase s) => IsBaseSkill<Skill_drain>(s));
			}
			break;
		case "killer":
			if (target.SkillApplyInformation.IsKiller)
			{
				parallelVfxPlayer.Register(owner.SkillApplyInformation.GiveKiller());
				flag = true;
				buffInfo = target.BuffInfoList.FirstOrDefault((BuffInfo b) => IsBuffSkill<Skill_killer>(b.SkillFrom));
				skillBase = target.Skills.FirstOrDefault((SkillBase s) => IsBaseSkill<Skill_killer>(s));
			}
			break;
		case "rush":
			if (target.SkillApplyInformation.IsRush)
			{
				parallelVfxPlayer.Register(owner.SkillApplyInformation.GiveRush(new RushInfo(target, string.Empty)));
				flag = true;
				buffInfo = target.BuffInfoList.FirstOrDefault((BuffInfo b) => IsBuffSkill<Skill_rush>(b.SkillFrom));
				skillBase = target.Skills.FirstOrDefault((SkillBase s) => IsBaseSkill<Skill_rush>(s));
			}
			break;
		case "quick":
			if (target.SkillApplyInformation.IsQuick)
			{
				parallelVfxPlayer.Register(owner.SkillApplyInformation.GiveQuick());
				flag = true;
				buffInfo = target.BuffInfoList.FirstOrDefault((BuffInfo b) => IsBuffSkill<Skill_quick>(b.SkillFrom));
				skillBase = target.Skills.FirstOrDefault((SkillBase s) => IsBaseSkill<Skill_quick>(s));
			}
			break;
		case "guard":
			if (target.SkillApplyInformation.IsGuard)
			{
				parallelVfxPlayer.Register(owner.SkillApplyInformation.GiveGuard(new GuardInfo(target, string.Empty)));
				flag = true;
				buffInfo = target.BuffInfoList.FirstOrDefault((BuffInfo b) => IsBuffSkill<Skill_guard>(b.SkillFrom));
				skillBase = target.Skills.FirstOrDefault((SkillBase s) => IsBaseSkill<Skill_guard>(s));
			}
			break;
		}
		if (flag)
		{
			BattleCardBase battleCardBase = null;
			bool flag2;
			bool isPlayer;
			if (buffInfo != null)
			{
				flag2 = buffInfo.IsEvolutionSkill;
				SkillBase skillFrom = buffInfo.SkillFrom;
				if (skillFrom is SkillBaseCopy)
				{
					battleCardBase = buffInfo.PreviousOwner;
					isPlayer = buffInfo.IsPlayer;
				}
				else
				{
					battleCardBase = skillFrom.SkillPrm.ownerCard;
					isPlayer = skillFrom.SkillPrm.ownerCard.IsPlayer;
				}
			}
			else
			{
				bool flag3 = target.IsEvolution;
				if (skillBase != null)
				{
					flag3 = target.EvolutionSkills.Contains(skillBase);
				}
				flag2 = flag3;
				battleCardBase = target;
				isPlayer = target.IsPlayer;
			}
			BuffInfo buffCopy = AddBuffInfoIfNeeded(owner, battleCardBase);
			buffCopy.IsCopied = true;
			if (skillBase != null && !(skillBase is Skill_attach_skill) && !(skillBase is SkillBaseCopy))
			{
				skillBase.SetOnLoseEvent(owner, buffCopy, null);
			}
			else
			{
				owner.OnLoseSkillOneTime += (SkillBase loseSkill, SkillProcessor skillProcessOneTime, BattleCardBase card) => card.SkillApplyInformation.AllSkillEffectStop();
			}
			BuffInfo buffInfo2 = buffCopy;
			bool isCopiedEvolutionSkill = (buffCopy.IsEvolutionSkill = flag2);
			buffInfo2.IsCopiedEvolutionSkill = isCopiedEvolutionSkill;
			buffCopy.SetPreviousOwner(battleCardBase);
			buffCopy.IsPlayer = isPlayer;
			owner.OnRemoveFromInPlayAfterOneTime += delegate
			{
				owner.RemoveBuffInfo(buffCopy);
				return NullVfx.GetInstance();
			};
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogCopiedSkill(target, this, isRemain);
			}
		}
		return parallelVfxPlayer;
	}

	private static bool IsCopySkill(SkillBase skill, string type)
	{
		if (skill is SkillBaseCopy skillBaseCopy)
		{
			return skillBaseCopy.SkillType == type;
		}
		return false;
	}

	private static bool IsSkillMasterCopy(string skillType)
	{
		switch (skillType)
		{
		case "when_destroy":
		case "when_fight":
		case "when_attack":
			return true;
		default:
			return false;
		}
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
