namespace Wizard;

public class AISituationTriggerInformation
{
	public enum TriggerType
	{
		Evolver,
		Damage,
		Banish,
		Bounce,
		Leave,
		Summon,
		WhenPlay,
		Undefined
	}

	public AIVirtualCard TriggerCard { get; private set; }

	public TriggerType Type { get; private set; }

	public AISituationTriggerInformation()
	{
		Type = TriggerType.Undefined;
	}

	public AISituationTriggerInformation(AIVirtualCard trigger, TriggerType type)
	{
		Type = type;
		TriggerCard = trigger;
	}

	public bool IsTriggerCard(AIVirtualCard card)
	{
		if (TriggerCard == null)
		{
			return false;
		}
		return TriggerCard.IsSameCard(card);
	}

	public bool IsTriggerCardAndTriggerType(AIVirtualCard card, TriggerType type)
	{
		if (type != Type || TriggerCard == null)
		{
			return false;
		}
		return TriggerCard.IsSameCard(card);
	}
}
