using System.Collections.Generic;
using System.Linq;

public class SkillCalcMaxFilter : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		return parameters.Max();
	}

	public int Filtering(int parameter)
	{
		return parameter;
	}
}
