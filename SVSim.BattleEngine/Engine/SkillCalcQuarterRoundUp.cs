using System.Collections.Generic;
using System.Linq;

public class SkillCalcQuarterRoundUp : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		int parameter = parameters.First();
		return Filtering(parameter);
	}

	public int Filtering(int parameter)
	{
		return parameter / 4 + ((parameter % 4 > 0) ? 1 : 0);
	}
}
