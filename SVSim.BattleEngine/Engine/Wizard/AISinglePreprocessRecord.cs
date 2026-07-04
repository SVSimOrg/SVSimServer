using System.Collections.Generic;

namespace Wizard;

public class AISinglePreprocessRecord
{
	public int NecromanceCount;

	public int BurialRiteCount;

	public AIVirtualCard RealActor { get; private set; }

	public AIVirtualCard OriginalCard { get; private set; }

	public AIScriptTokenArgType Timing { get; private set; }

	public int EarthRiteCount { get; private set; }

	public List<EarthRiteRecordContainer> EarthRiteContainer { get; private set; }

	public AISinglePreprocessRecord(AIVirtualCard realActor, AIVirtualCard originalCard, AIScriptTokenArgType timing)
	{
		RealActor = realActor;
		OriginalCard = originalCard;
		Timing = timing;
		NecromanceCount = 0;
		BurialRiteCount = 0;
		EarthRiteCount = 0;
		EarthRiteContainer = new List<EarthRiteRecordContainer>();
	}

	public void AddConsumedEarthRite(AIVirtualCard consumedTarget, int consumedStack)
	{
		if (consumedTarget == null)
		{
			AIConsoleUtility.LogError("AISinglePreprocessRecord.AddConsumedEarthRite(): Consumed stack target is null.");
			return;
		}
		if (consumedStack <= 0)
		{
			AIConsoleUtility.LogError("AISinglePreprocessRecord.AddConsumedEarthRite(): Consumed stack count is less than 1");
			return;
		}
		EarthRiteCount += consumedStack;
		EarthRiteContainer.Add(new EarthRiteRecordContainer(consumedTarget, consumedStack));
	}

	public void RestoreAllEarthRiteCount(AIVirtualField field)
	{
		if (field == null)
		{
			AIConsoleUtility.LogError("RestoreAllEarthRiteCount(): field is null.");
			return;
		}
		List<AIVirtualCard> list = (RealActor.IsAlly ? field.AllyInplayCards : field.EnemyInplayCards);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (RestoreEarthRiteCount(list[i]))
			{
				num++;
			}
		}
		if (EarthRiteContainer.Count != num)
		{
			AIConsoleUtility.LogError("RestoreAllEarthRiteCount(): Failed to restore all stacks!!!! ");
		}
	}

	private bool RestoreEarthRiteCount(AIVirtualCard card)
	{
		if (card == null)
		{
			AIConsoleUtility.LogError("RestoreEarthRiteCount(): card is null.");
			return false;
		}
		if (!card.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL))
		{
			return false;
		}
		for (int i = 0; i < EarthRiteContainer.Count; i++)
		{
			EarthRiteRecordContainer earthRiteRecordContainer = EarthRiteContainer[i];
			if (card.IsSameCard(earthRiteRecordContainer.ConsumedTarget))
			{
				card.ResetEarthRite(earthRiteRecordContainer.ConsumedStack);
				return true;
			}
		}
		return false;
	}
}
