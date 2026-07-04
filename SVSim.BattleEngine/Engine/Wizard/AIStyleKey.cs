namespace Wizard;

public class AIStyleKey
{
	public AICategory category;

	public AIPolicyType policy;

	public AIStyleKey(AICategory _category, AIPolicyType _policy)
	{
		category = _category;
		policy = _policy;
	}

	public static bool operator ==(AIStyleKey lhs, AIStyleKey rhs)
	{
		if (lhs.category == rhs.category)
		{
			return lhs.policy == rhs.policy;
		}
		return false;
	}

	public static bool operator !=(AIStyleKey lhs, AIStyleKey rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
