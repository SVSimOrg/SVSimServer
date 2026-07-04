namespace Wizard;

public class PolicyCollectionWithSingleType : PolicyCollectionWithTypeBase
{
	private AIPolicyType _policyType;

	public PolicyCollectionWithSingleType(AIPolicyType policyType, AIPolicyCollection policyCollection)
	{
		_policyType = policyType;
		base.PolicyCollection = policyCollection;
	}

	public override bool IsUnderManagement(AIPolicyType type)
	{
		return _policyType == type;
	}

	public override void AddPolicy(AIPolicyData policy)
	{
		if (_policyType == policy.PolicyType)
		{
			base.PolicyCollection.AddPolicy(policy);
		}
	}

	public override bool HasTag(AIPolicyType type)
	{
		if (IsUnderManagement(type))
		{
			return base.PolicyCollection.HasPolicy;
		}
		return false;
	}

	public override bool RemovePolicyAndCheckIsNoLongerHoldThisType(AIPolicyData policy)
	{
		base.PolicyCollection.RemovePolicy(policy);
		return !base.PolicyCollection.HasPolicy;
	}
}
