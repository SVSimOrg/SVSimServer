using System.Linq;

public class NetworkSkill_summon_card : Skill_summon_card
{
	public NetworkSkill_summon_card(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void CallOnSummonCards()
	{
		if (_summonedCardsList != null && _summonedCardsList.summonedCards.Count() > 0)
		{
			base.SkillPrm.selfBattlePlayer.CallOnSummonCards(base.SkillPrm.ownerCard, _summonedCardsList.summonedCards.ToList(), base.SkillPrm.ownerCard.IsPlayer, base.IsDeckSelfSummon, _isIgnoreVoice);
		}
	}
}
