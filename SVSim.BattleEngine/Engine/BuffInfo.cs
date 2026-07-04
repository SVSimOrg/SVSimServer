using System.Collections.Generic;
using System.Linq;
using Wizard;

public class BuffInfo
{

	public int BaseCardIDFrom { get; private set; }

	public int CardIDFrom { get; private set; }

	public SkillBase SkillFrom { get; private set; }

	public BattleCardBase OwnerCard { get; private set; }

	public BattleCardBase PreviousOwner { get; private set; }

	public BattleCardBase TargetCard { get; set; }

	public bool IsCopied { get; set; }

	public bool IsPlayer { get; set; }

	public bool IsCopiedEvolutionSkill { get; set; }

	public bool IsEvolutionSkill { get; set; }

	public bool IsSaveBurialRiteSkill { get; set; }

	public bool IsGetonSkill { get; set; }

	public bool IsHiddenClassLogSkill { get; set; }

	public string DivergenceId { get; set; }

	public bool IsReserveTokenDrawSkill { get; set; }

	public BossRushSpecialSkill SpecialSkillInfo { get; set; }

	public BuffInfo(int fromBaseCardID, int fromCardID, SkillBase fromSkill, BattleCardBase ownerCard = null, bool isPlayer = false, string divergenceId = "")
	{
		BaseCardIDFrom = fromBaseCardID;
		CardIDFrom = fromCardID;
		SkillFrom = fromSkill;
		PreviousOwner = null;
		if (SkillFrom != null)
		{
			IsPlayer = SkillFrom.SkillPrm.ownerCard.IsPlayer;
			IsEvolutionSkill = SkillFrom.SkillPrm.ownerCard.EvolutionSkills != null && SkillFrom.SkillPrm.ownerCard.EvolutionSkills.Any((SkillBase skill) => skill == SkillFrom);
			DivergenceId = SkillFrom.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.divergence_id, "_OPT_NULL_");
			OwnerCard = SkillFrom.SkillPrm.ownerCard;
		}
		else
		{
			IsPlayer = isPlayer;
			DivergenceId = divergenceId;
			OwnerCard = ownerCard;
		}
	}

	public void SetPreviousOwner(BattleCardBase card)
	{
		PreviousOwner = card;
	}

	private BuffInfo(BuffInfo buff)
	{
		BaseCardIDFrom = buff.BaseCardIDFrom;
		CardIDFrom = buff.CardIDFrom;
		SkillFrom = buff.SkillFrom;
		OwnerCard = buff.OwnerCard;
		PreviousOwner = buff.PreviousOwner;
		IsCopied = buff.IsCopied;
		IsCopiedEvolutionSkill = buff.IsCopiedEvolutionSkill;
		IsPlayer = buff.IsPlayer;
		IsEvolutionSkill = buff.IsEvolutionSkill;
		IsSaveBurialRiteSkill = buff.IsSaveBurialRiteSkill;
		IsGetonSkill = buff.IsGetonSkill;
		IsHiddenClassLogSkill = buff.IsHiddenClassLogSkill;
		DivergenceId = buff.DivergenceId;
		IsReserveTokenDrawSkill = buff.IsReserveTokenDrawSkill;
	}

	public BuffInfo Clone()
	{
		return new BuffInfo(this);
	}

	public bool IsBuffGaveSkill(SkillBase skill)
	{
		if (SkillFrom is SkillBaseCopy skillBaseCopy && skillBaseCopy.CopiedSkillList.Any((SkillBase s) => skill.IsSameSkill(s)))
		{
			return true;
		}
		return SkillFrom == skill;
	}
}
