using System.Collections.Generic;

namespace Wizard;

public class AIPolicyCollectionContainer
{
	private List<AIPolicyType> _holdingPolicyTypes;

	private List<PolicyCollectionWithTypeBase> _policyCollectionTable;

	public AIPolicyCollectionContainer()
	{
		_holdingPolicyTypes = null;
		_policyCollectionTable = null;
	}

	public bool HasPolicy(AIPolicyType policyType)
	{
		if (_holdingPolicyTypes != null)
		{
			return _holdingPolicyTypes.Contains(policyType);
		}
		return false;
	}

	public AIPolicyCollection GetPolicyCollection(AIPolicyType policyType)
	{
		if (_policyCollectionTable != null)
		{
			for (int i = 0; i < _policyCollectionTable.Count; i++)
			{
				PolicyCollectionWithTypeBase policyCollectionWithTypeBase = _policyCollectionTable[i];
				if (policyCollectionWithTypeBase.HasTag(policyType))
				{
					return policyCollectionWithTypeBase.PolicyCollection;
				}
			}
		}
		return null;
	}

	public void InitializeAllCollections(List<AIPolicyData> policyList)
	{
		for (int i = 0; i < policyList.Count; i++)
		{
			AIPolicyData policy = policyList[i];
			RegisterNewPolicy(policy);
		}
	}

	public void RegisterNewPolicy(AIPolicyData policy)
	{
		PolicyCollectionWithTypeBase policyCollectionWithTypeBase = FindPolicyCollectionWityType(policy.PolicyType);
		if (policyCollectionWithTypeBase != null)
		{
			policyCollectionWithTypeBase.AddPolicy(policy);
			if (_holdingPolicyTypes == null)
			{
				_holdingPolicyTypes = new List<AIPolicyType>();
			}
			if (!_holdingPolicyTypes.Contains(policy.PolicyType))
			{
				_holdingPolicyTypes.Add(policy.PolicyType);
			}
		}
	}

	private PolicyCollectionWithTypeBase FindPolicyCollectionWityType(AIPolicyType policyType, bool isCreateNewCollection = true)
	{
		if (_policyCollectionTable != null)
		{
			for (int i = 0; i < _policyCollectionTable.Count; i++)
			{
				PolicyCollectionWithTypeBase policyCollectionWithTypeBase = _policyCollectionTable[i];
				if (policyCollectionWithTypeBase.IsUnderManagement(policyType))
				{
					return policyCollectionWithTypeBase;
				}
			}
		}
		if (isCreateNewCollection)
		{
			PolicyCollectionWithTypeBase policyCollectionWithTypeBase2 = PolicyCollectionWithTypeCreator.Create(policyType);
			if (policyCollectionWithTypeBase2 != null)
			{
				if (_policyCollectionTable == null)
				{
					_policyCollectionTable = new List<PolicyCollectionWithTypeBase>();
				}
				_policyCollectionTable.Add(policyCollectionWithTypeBase2);
			}
			return policyCollectionWithTypeBase2;
		}
		return null;
	}
}
