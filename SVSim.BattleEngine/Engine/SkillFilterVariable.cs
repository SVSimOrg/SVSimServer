using Wizard;
using Wizard.Battle;

public class SkillFilterVariable
{
	private readonly BattlePlayerReadOnlyInfoPair m_playerInfoPair;

	private readonly IReadOnlyBattleCardInfo m_ownerCardInfo;

	private readonly SkillConditionCheckerOption _checkerOption;

	private readonly bool _isPrePlay;

	private readonly SkillBase _skill;

	public SkillFilterVariable(BattlePlayerReadOnlyInfoPair playerInfoPair, IReadOnlyBattleCardInfo ownerCardInfo, bool isPrePlay, SkillBase skill, SkillConditionCheckerOption checkerOption = null)
	{
		m_playerInfoPair = playerInfoPair;
		m_ownerCardInfo = ownerCardInfo;
		_checkerOption = ((checkerOption != null) ? checkerOption : new SkillConditionCheckerOption());
		_isPrePlay = isPrePlay;
		_skill = skill;
	}

	public int Parse(string text)
	{
		VariableSkillFilterCollection variableSkillFilterCollection = new VariableSkillFilterCollection(_checkerOption.IsSkipPrivateCardCheck);
		SkillFilterCreator.SetupVariable(variableSkillFilterCollection, text, m_ownerCardInfo, _skill);
		return variableSkillFilterCollection.Filtering(m_playerInfoPair, _checkerOption, _isPrePlay);
	}
}
