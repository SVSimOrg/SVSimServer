using System.Collections.Generic;
using System.Linq;

public class SkillCalcHalfRoundUp : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		int parameter = parameters.First();
		return Filtering(parameter);
	}

	public int Filtering(int parameter)
	{
		return parameter / 2 + parameter % 2;
	}
}
