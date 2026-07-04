namespace Wizard;

public class AIVirtualFieldRollBackBasicProcessor
{
	protected AIVirtualField _field;

	private readonly AIVirtualFieldRollBackRecord _startRollBackRecord;

	public AIVirtualFieldRollBackBasicProcessor(AIVirtualField targetField)
	{
		_field = targetField;
		_startRollBackRecord = new AIVirtualFieldRollBackRecord(targetField);
	}

	public virtual void ResetVirtualFieldToStart()
	{
		RollBackFromOneRecord(_startRollBackRecord);
	}

	protected void RollBackFromOneRecord(AIVirtualFieldRollBackRecord record)
	{
		for (int i = 0; i < _field.CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = _field.CardListSet.AllReferableCards[i];
			AIVirtualFieldRollBackRecord.CardParamRecord record2 = record.CardRecordList[i];
			aIVirtualCard.RollBackFromOneRecord(record2);
		}
		_field.VirtualCemetery.RollBackFromOneRecord(record.Cemetery);
		_field.PlayedCardContainer.RollBackFromOneRecord(record.PlayedCardContainer);
		_field.IsNoInstantAttack = record.IsNotInstantAttack;
		_field.AllyNecromancedCountInGame = record.AllyNecromancedCountInGame;
		int num = _field.AllyGameAddUpdateDeckCards.Count - record.AllyAddedDeckCountInGame;
		if (0 < num)
		{
			_field.AllyGameAddUpdateDeckCards.RemoveRange(record.AllyAddedDeckCountInGame, num);
		}
		_field.SummonedCardContainer.RollBackSummonedCard(record.AllySummonedCardCount);
	}
}
