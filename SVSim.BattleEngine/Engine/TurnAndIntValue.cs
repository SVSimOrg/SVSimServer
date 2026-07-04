public class TurnAndIntValue
{

	public int Value { get; private set; }

	public int Turn { get; private set; }

	public bool IsSelfTurn { get; private set; }

	public TurnAndIntValue(int value, int turn, bool isSelfTurn)
	{
		Value = value;
		Turn = turn;
		IsSelfTurn = isSelfTurn;
	}

	public void Increment()
	{
		int value = Value + 1;
		Value = value;
	}

	public void AddValue(int count)
	{
		Value += count;
	}

	public bool IsSpecificTurn(int turn, bool isSelfTurn)
	{
		if (turn != -1 && Turn == turn)
		{
			return IsSelfTurn == isSelfTurn;
		}
		return false;
	}
}
