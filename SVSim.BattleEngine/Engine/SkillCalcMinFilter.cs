using System.Collections.Generic;
using System.Linq;

public class SkillCalcMinFilter : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		return parameters.Min();
	}

	public int Filtering(int parameter)
	{
		return parameter;
	}
}
