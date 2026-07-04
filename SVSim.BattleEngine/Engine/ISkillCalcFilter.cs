using System.Collections.Generic;

public interface ISkillCalcFilter
{
	int Filtering(IEnumerable<int> parameters);

	int Filtering(int parameter);
}
