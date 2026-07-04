using System.Collections.Generic;

namespace Wizard;

public class AIVirtualCardStatusInfo
{
	public AIVirtualCard BaseCard { get; private set; }

	public int Attack { get; private set; }

	public int Life { get; private set; }

	public float ValueWhenDestroyed { get; private set; }

	public AIVirtualCardStatusInfo(AIVirtualCard card, int attack, int life)
	{
		BaseCard = card;
		Attack = attack;
		Life = life;
		ValueWhenDestroyed = 0f;
	}

	public void CalculateCardValueWhenDestroyed(List<int> playPtn, AISituationInfo situation)
	{
		int attack = BaseCard.Attack;
		int life = BaseCard.Life;
		BaseCard.Attack = Attack;
		BaseCard.Life = Life;
		ValueWhenDestroyed = BaseCard.EvaluateValueOnField(playPtn, situation, useStyle: true) - BaseCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - BaseCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
		BaseCard.Attack = attack;
		BaseCard.Life = life;
	}

	public void ModifyStatus(int attack, int life)
	{
		Attack = attack;
		Life = life;
	}
}
