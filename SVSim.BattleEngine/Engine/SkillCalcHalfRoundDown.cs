using System.Collections.Generic;
using System.Linq;

public class SkillCalcHalfRoundDown : ISkillCalcFilter
{
	public int Filtering(IEnumerable<int> parameters)
	{
		return Filtering(parameters.First());
	}

	public int Filtering(int parameter)
	{
		return parameter / 2;
	}
}
