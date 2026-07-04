namespace Wizard;

public class PuppetAttackParam
{
	public int Times;

	public int Attack { get; private set; }

	public int Life { get; private set; }

	public PuppetAttackParam(int attack, int life, int times)
	{
		Attack = attack;
		Life = life;
		Times = times;
	}
}
