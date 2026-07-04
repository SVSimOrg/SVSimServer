public class SkillOrFilter : ISkillOrFilter
{
	private int _or;

	public SkillOrFilter(int or)
	{
		_or = or;
	}

	public int Filtering()
	{
		return _or;
	}
}
