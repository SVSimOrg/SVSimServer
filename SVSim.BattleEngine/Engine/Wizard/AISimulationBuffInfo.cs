namespace Wizard;

public class AISimulationBuffInfo
{
	public int TempAttackBuff;

	public int TempLifeBuff;

	public int TotalAttackBuff;

	public int TotalLifeBuff;

	public AISimulationBuffInfo(int tempAttack, int tempLife, int totalAttack, int totalLife)
	{
		TempAttackBuff = tempAttack;
		TempLifeBuff = tempLife;
		TotalAttackBuff = totalAttack;
		TotalLifeBuff = totalLife;
	}

	public static AISimulationBuffInfo operator +(AISimulationBuffInfo a, AISimulationBuffInfo b)
	{
		return new AISimulationBuffInfo(a.TempAttackBuff + b.TempAttackBuff, a.TempLifeBuff + b.TempLifeBuff, a.TotalAttackBuff + b.TotalAttackBuff, a.TotalLifeBuff + b.TotalLifeBuff);
	}
}
