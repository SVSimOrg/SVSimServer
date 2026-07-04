namespace Wizard;

public class EvalInstantAttackInformation
{
	public bool IsAttackerDestroyWhenAttack;

	public int AttackerTotalDamage;

	public AIVirtualAttackInfo Situation { get; private set; }

	public int AttackerAttackBuff { get; private set; }

	public int AttackerLifeBuff { get; private set; }

	public int TargetLifeBuff { get; private set; }

	public AIBarrierPseudoSimulationInfo AttackerBarrierInfo { get; private set; }

	public AIBarrierPseudoSimulationInfo TargetBarrierInfo { get; private set; }

	public EvalInstantAttackInformation(AIVirtualAttackInfo situation)
	{
		Situation = situation;
		AttackerAttackBuff = 0;
		AttackerLifeBuff = 0;
		TargetLifeBuff = 0;
		IsAttackerDestroyWhenAttack = false;
		AttackerBarrierInfo = new AIBarrierPseudoSimulationInfo(situation.Actor);
		AttackerTotalDamage = 0;
		TargetBarrierInfo = new AIBarrierPseudoSimulationInfo(situation.AttackTarget);
	}

	public void AddAttackerWhenAttackBuff(int attack, int life)
	{
		AttackerAttackBuff += attack;
		AttackerLifeBuff += life;
	}

	public void AddTargetLifeBuff(int value)
	{
		TargetLifeBuff += value;
	}
}
