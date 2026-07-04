using System.Linq;

public class RepeatSkillInfo
{
	public string Timing { get; private set; }

	public string Target { get; private set; }

	public SkillBase Skill { get; private set; }

	public bool IsRemoveReservation { get; set; }

	public RepeatSkillInfo(string timing, string target, SkillBase skill)
	{
		Timing = timing;
		Target = target;
		Skill = skill;
		IsRemoveReservation = false;
	}

	public RepeatSkillInfo CloneAndRebuildSkill(BattleCardBase card)
	{
		SkillBase skillBase = card.CreateSkillCreator(card.SelfBattlePlayer, card.OpponentBattlePlayer, card.ResourceMgr).Create(Skill.SkillPrm.buildInfo);
		skillBase.CloneBuffInfoContainer(card.SelfBattlePlayer.AllCards.ToList(), card.OpponentBattlePlayer.AllCards.ToList(), Skill);
		return new RepeatSkillInfo(Timing, Target, skillBase)
		{
			IsRemoveReservation = IsRemoveReservation
		};
	}
}
