public class NetworkSkill_unite : Skill_unite
{
	public NetworkSkill_unite(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void CallOnUnite()
	{
		if (_uniteCardPair != null)
		{
			base.SkillPrm.selfBattlePlayer.CallOnUnite(base.SkillPrm.ownerCard, _uniteCardPair._materialCards, _uniteCardPair._uniteCards[0]);
		}
	}
}
