using System.Linq;

public class SkillEnvironmentalAttachedTurnFilter : ISkillEnvironmentalFilter
{
	private string _id;

	public SkillEnvironmentalAttachedTurnFilter(string option, SkillBase skill)
	{
		string[] array = option.Split(':');
		_id = array[0];
		if (array.Count() > 1 && array[1] == SkillFilterCreator.ContentKeyword.is_individual.ToString() && skill != null)
		{
			_id += skill.IndividualId;
		}
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GetAttachTurnBySkillId(_id);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
