using System;
using System.Collections.Generic;

namespace Wizard;

public class AIVirtualCardParameter
{
	public int BaseId;

	public string CardName;

	public int Attack;

	public int Life;

	public int DefaultAttack;

	public int DefaultLife;

	public int MaxLife;

	public int DefLife;

	public int LastLife;

	public int MaxAttackableCount;

	public int DefaultMaxAttackableCount;

	public int EvoAttackPlus;

	public int EvoLifePlus;

	public AIVirtualCardParameter(BattleCardBase origin)
	{
		ISkillApplyInformation skillApplyInformation = origin.SkillApplyInformation;
		BaseId = origin.BaseParameter.BaseCardId;
		CardName = origin.BaseParameter.CardName;
		Attack = origin.Atk;
		Life = origin.Life;
		DefaultAttack = origin.BaseParameter.Atk;
		DefaultLife = origin.BaseParameter.Life;
		MaxLife = origin.MaxLife;
		DefLife = Life;
		LastLife = skillApplyInformation.GetLastLife();
		if (origin.IsUnit)
		{
			EvoAttackPlus = GetAtkFromSkillApplyInfo(skillApplyInformation.OffenseModifierList, origin.BaseParameter.EvoAtk) - GetAtkFromSkillApplyInfo(skillApplyInformation.OffenseModifierList, origin.BaseParameter.Atk);
			EvoLifePlus = GetLifeFromSkillApplyInfo(skillApplyInformation.LifeModifierList, origin.BaseParameter.EvoLife) - GetLifeFromSkillApplyInfo(skillApplyInformation.LifeModifierList, origin.BaseParameter.Life);
			MaxAttackableCount = origin.MaxAttackableCount;
			DefaultMaxAttackableCount = origin.MaxAttackableCount;
		}
		else
		{
			EvoAttackPlus = 0;
			EvoLifePlus = 0;
			MaxAttackableCount = 0;
			DefaultMaxAttackableCount = 0;
		}
	}

	private AIVirtualCardParameter(AIVirtualCardParameter cloneOriginal)
	{
		BaseId = cloneOriginal.BaseId;
		CardName = cloneOriginal.CardName;
		Attack = cloneOriginal.Attack;
		Life = cloneOriginal.Life;
		DefaultAttack = cloneOriginal.DefaultAttack;
		DefaultLife = cloneOriginal.DefaultLife;
		MaxLife = cloneOriginal.MaxLife;
		DefLife = cloneOriginal.DefLife;
		LastLife = cloneOriginal.LastLife;
		EvoAttackPlus = cloneOriginal.EvoAttackPlus;
		EvoLifePlus = cloneOriginal.EvoLifePlus;
		MaxAttackableCount = cloneOriginal.MaxAttackableCount;
		DefaultMaxAttackableCount = cloneOriginal.MaxAttackableCount;
	}

	public AIVirtualCardParameter(BattleCardBase origin, BattleCardBase baseParamCard, AIBuffRecorderCollection buffRecord)
	{
		ISkillApplyInformation skillApplyInformation = baseParamCard.SkillApplyInformation;
		(int, int) simulateBuff = buffRecord.GetSimulateBuff();
		BaseId = origin.CardId;
		CardName = origin.BaseParameter.CardName;
		Attack = GetAtkFromSkillApplyInfo(skillApplyInformation.OffenseModifierList, origin.BaseParameter.Atk, simulateBuff.Item1);
		Life = GetLifeFromSkillApplyInfo(skillApplyInformation.LifeModifierList, origin.BaseParameter.Life, simulateBuff.Item2);
		DefaultAttack = origin.BaseParameter.Atk;
		DefaultLife = origin.BaseParameter.Life;
		MaxLife = origin.MaxLife + simulateBuff.Item2;
		DefLife = Life;
		LastLife = skillApplyInformation.GetLastLife() + simulateBuff.Item2;
		if (origin.IsUnit)
		{
			EvoAttackPlus = GetAtkFromSkillApplyInfo(skillApplyInformation.OffenseModifierList, origin.BaseParameter.EvoAtk, simulateBuff.Item1) - Attack;
			EvoLifePlus = GetLifeFromSkillApplyInfo(skillApplyInformation.LifeModifierList, origin.BaseParameter.EvoLife, simulateBuff.Item2) - Life;
			MaxAttackableCount = origin.MaxAttackableCount;
			DefaultMaxAttackableCount = origin.MaxAttackableCount;
		}
		else
		{
			EvoAttackPlus = 0;
			EvoLifePlus = 0;
			MaxAttackableCount = 0;
			DefaultMaxAttackableCount = 0;
		}
	}

	public AIVirtualCardParameter(int attack, int life, int attackableCount)
	{
		Attack = attack;
		Life = life;
		DefaultAttack = attack;
		DefaultLife = life;
		MaxLife = life;
		DefLife = life;
		LastLife = life;
		MaxAttackableCount = attackableCount;
		DefaultMaxAttackableCount = attackableCount;
		BaseId = 0;
		CardName = "Dummy";
		EvoAttackPlus = 0;
		EvoLifePlus = 0;
	}

	public AIVirtualCardParameter Clone()
	{
		return new AIVirtualCardParameter(this);
	}

	private int GetAtkFromSkillApplyInfo(List<ICardOffenseModifier> modifiers, int baseAtk, int simulateBuffValue = 0)
	{
		int num = baseAtk;
		int count = modifiers.Count;
		for (int i = 0; i < count; i++)
		{
			num = modifiers[i].CalcOffense(num);
		}
		return Math.Max(0, num + simulateBuffValue);
	}

	private int GetLifeFromSkillApplyInfo(List<ICardLifeModifier> modifiers, int baseMaxLife, int simulateBuffValue = 0)
	{
		int num = baseMaxLife;
		int num2 = baseMaxLife;
		int count = modifiers.Count;
		for (int i = 0; i < count; i++)
		{
			ICardLifeModifier cardLifeModifier = modifiers[i];
			num2 = cardLifeModifier.CalcMaxLife(num2);
			num = cardLifeModifier.CalcLife(num);
			num = Math.Min(num, num2);
		}
		return num + simulateBuffValue;
	}
}
