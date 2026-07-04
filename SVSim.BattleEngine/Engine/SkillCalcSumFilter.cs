using System.Collections.Generic;
using System.Linq;

public class SkillCalcSumFilter : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		return parameters.Sum();
	}

	public int Filtering(int parameter)
	{
		return parameter;
	}
}
