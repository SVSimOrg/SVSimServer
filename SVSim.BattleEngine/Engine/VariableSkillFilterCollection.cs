using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class VariableSkillFilterCollection : SkillFilterCollectionBase
{
	public SkillCardCountFilter CardCountFilter { get; set; }

	public ISkillCardCountExtensionsFilter CardCountExtensionsFilter { get; set; }

	public ISkillEnvironmentalFilter EnvCountFilter { get; set; }

	public ISkillParameterSelectFilter ParameterSelectFilter { get; set; }

	public ISkillCalcFilter CalcFilter { get; set; }

	public ISkillOrFilter OrFilter { get; set; }

	public VariableSkillFilterCollection(bool isSkipPrivateCardCheck = false)
		: base(isSkipPrivateCardCheck)
	{
	}

	public int Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, bool isPrePlay)
	{
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = FilteringBase(playerInfoPair, checkerOption, new SkillOptionValue(""));
		if (CardCountFilter != null)
		{
			if (enumerable.Any())
			{
				int num = CardCountFilter.Filtering(enumerable);
				if (CardCountExtensionsFilter != null)
				{
					num = CardCountExtensionsFilter.Filtering(num);
				}
				if (CalcFilter != null)
				{
					num = CalcFilter.Filtering(num);
					if (CardCountExtensionsFilter != null)
					{
						num = CardCountExtensionsFilter.Filtering(num);
					}
				}
				return num;
			}
			return 0;
		}
		if (EnvCountFilter != null)
		{
			IBattlePlayerReadOnlyInfo battlePlayer = GetBattlePlayer(playerInfoPair);
			if (battlePlayer != null)
			{
				int num2 = ((!isPrePlay) ? EnvCountFilter.Filtering(battlePlayer, checkerOption) : EnvCountFilter.FilteringPrePlay(battlePlayer, checkerOption));
				if (CalcFilter != null)
				{
					num2 = CalcFilter.Filtering(num2);
				}
				if (CardCountExtensionsFilter != null)
				{
					num2 = CardCountExtensionsFilter.Filtering(num2);
				}
				return num2;
			}
		}
		if (!enumerable.Any())
		{
			if (OrFilter != null)
			{
				return OrFilter.Filtering();
			}
			return -1;
		}
		IEnumerable<int> enumerable2 = ParameterSelectFilter.Filtering(enumerable, checkerOption);
		if (CalcFilter == null)
		{
			int num3 = enumerable2.First();
			if (CardCountExtensionsFilter != null)
			{
				num3 = CardCountExtensionsFilter.Filtering(num3);
			}
			return num3;
		}
		if (enumerable2.Count() > 0)
		{
			int num4 = CalcFilter.Filtering(enumerable2);
			if (CardCountExtensionsFilter != null)
			{
				num4 = CardCountExtensionsFilter.Filtering(num4);
			}
			return num4;
		}
		return -1;
	}

	private IBattlePlayerReadOnlyInfo GetBattlePlayer(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		if (base.BattlePlayerFilter != null)
		{
			return base.BattlePlayerFilter.Filtering(playerInfoPair).First();
		}
		return null;
	}
}
