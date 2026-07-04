namespace Wizard;

public class AIVirtualCemetery
{
	private class CemeteryInfo
	{
		public int OriginalCemetery { get; private set; }

		public int CurrentCemetery { get; private set; }

		public CemeteryInfo(int cemetery)
		{
			OriginalCemetery = cemetery;
			CurrentCemetery = OriginalCemetery;
		}

		public void ResetNecromance(int count)
		{
			CurrentCemetery += count;
		}

		public void Necromance(int necromance, bool isPseudo)
		{
			if (necromance <= CurrentCemetery)
			{
				CurrentCemetery -= necromance;
				if (!isPseudo)
				{
					OriginalCemetery = CurrentCemetery;
				}
			}
		}

		public void Add(int cemetery)
		{
			CurrentCemetery += cemetery;
		}

		public void RollBackCemetery(int cemetery)
		{
			CurrentCemetery = cemetery;
		}
	}

	private CemeteryInfo _allyCemetery;

	private CemeteryInfo _enemyCemetery;

	public AIVirtualCemetery(int allyCemetery, int enemyCemetery)
	{
		_allyCemetery = new CemeteryInfo(allyCemetery);
		_enemyCemetery = new CemeteryInfo(enemyCemetery);
	}

	public void ResetNecromance(bool isAlly, int count)
	{
		(isAlly ? _allyCemetery : _enemyCemetery).ResetNecromance(count);
	}

	public int GetCemeteryCount(bool isAlly)
	{
		return (isAlly ? _allyCemetery : _enemyCemetery).CurrentCemetery;
	}

	public void ExecuteNecromance(int count, bool isAlly, bool isPseudo)
	{
		(isAlly ? _allyCemetery : _enemyCemetery).Necromance(count, isPseudo);
	}

	public void AddCemetery(int count, bool isAlly)
	{
		(isAlly ? _allyCemetery : _enemyCemetery).Add(count);
	}

	public void RollBackFromOneRecord(AIVirtualFieldRollBackRecord.CemeteryRecord record)
	{
		_allyCemetery.RollBackCemetery(record.AllyCemetery);
		_enemyCemetery.RollBackCemetery(record.EnemyCemetery);
	}
}
