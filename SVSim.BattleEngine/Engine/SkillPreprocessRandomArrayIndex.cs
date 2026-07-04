using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessRandomArrayIndex : SkillPreprocessBase
{
	private readonly IReadOnlyBattleCardInfo _card;

	public int[] RandomIndexes { get; private set; }

	public SkillPreprocessRandomArrayIndex(IReadOnlyBattleCardInfo card, int[] randomIndexes)
	{
		_card = card;
		RandomIndexes = randomIndexes;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int[] skillRandomArray = _card.SkillApplyInformation.SkillRandomArray;
		if (skillRandomArray != null)
		{
			bool flag = false;
			for (int i = 0; i < RandomIndexes.Length; i++)
			{
				if (RandomIndexes[i] >= 0 && RandomIndexes[i] < skillRandomArray.Length)
				{
					flag = flag || skillRandomArray[RandomIndexes[i]] > 0;
					continue;
				}
				return false;
			}
			return flag;
		}
		return false;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
