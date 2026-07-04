namespace Wizard;

public abstract class PolicyCollectionWithTypeBase
{
	public AIPolicyCollection PolicyCollection { get; protected set; }

	public abstract bool IsUnderManagement(AIPolicyType type);

	public abstract void AddPolicy(AIPolicyData policy);

	public abstract bool HasTag(AIPolicyType type);

	public abstract bool RemovePolicyAndCheckIsNoLongerHoldThisType(AIPolicyData policy);
}
