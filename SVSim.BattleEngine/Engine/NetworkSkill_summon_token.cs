using System.Linq;

public class NetworkSkill_summon_token : Skill_summon_token
{
	public NetworkSkill_summon_token(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void CallOnSummonTokenCards(bool isOwnerEffect)
	{
		if (_summonedCardsList != null && (_summonedCardsList.summonedCards.Count() > 0 || _summonedCardsList.overflowCards.Count() > 0))
		{
			bool isSelf = ((_summonedCardsList.summonedCards.Count() > 0) ? _summonedCardsList.summonedCards.First().IsPlayer : _summonedCardsList.overflowCards.First().IsPlayer);
			_summonPlayer.CallOnSummonTokenCards(base.SkillPrm.ownerCard, _summonedCardsList.summonedCards.ToList(), _summonedCardsList.overflowCards.ToList(), isSelf, isOwnerEffect, _isIgnoreVoice, _isRandomVoice, _isEvoVoice);
		}
	}
}
