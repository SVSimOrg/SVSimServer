namespace Wizard;

public class AIAttachedTagInformation
{
	public AIPlayTag Tag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	public int ProviderIndex { get; private set; }

	public bool IsProviderAlly { get; private set; }

	public int ProviderCardId { get; private set; }

	public int ReceiverIndex { get; private set; }

	public bool IsReceiverAlly { get; private set; }

	public ulong Hash { get; private set; }

	public AIAttachedTagInformation(AIPlayTag attachTag, AIScriptTokenArgType removeTiming, AIVirtualCard provider, AIVirtualCard receiver)
	{
		Tag = attachTag;
		RemoveTiming = removeTiming;
		ProviderIndex = provider.CardIndex;
		IsProviderAlly = provider.IsAlly;
		ProviderCardId = provider.BaseId;
		ReceiverIndex = receiver.CardIndex;
		IsReceiverAlly = receiver.IsAlly;
		Hash = GetHash();
	}

	public AIAttachedTagInformation(AIPlayTag attachTag, AIScriptTokenArgType removeTiming, int providerCardId, int providerIndex, bool providerIsAlly, int receiverIndex, bool receiverIsAlly)
	{
		Tag = attachTag;
		RemoveTiming = removeTiming;
		ProviderIndex = providerIndex;
		IsProviderAlly = providerIsAlly;
		ProviderCardId = providerCardId;
		ReceiverIndex = receiverIndex;
		IsReceiverAlly = receiverIsAlly;
		Hash = GetHash();
	}

	public ulong GetHash()
	{
		return (ulong)(((long)(Tag.Hash * 6323) + (long)RemoveTiming.GetHashCode() * 15187L + (long)ProviderIndex * 682009L + (long)ProviderCardId * 733L + (long)ReceiverIndex * 192149L) * (IsProviderAlly ? 641 : 43) * (IsReceiverAlly ? 503 : 4211));
	}
}
