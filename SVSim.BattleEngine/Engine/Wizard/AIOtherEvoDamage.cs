using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoDamage : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _damage;

	private AIPolishConvertedExpression _count;

	protected override int SELECT_TYPE_ARG_OFFSET => 3;

	public AIOtherEvoDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damage = _exprList[_exprList.Count - 2];
		_count = _exprList[_exprList.Count - 1];
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent)
		{
			return false;
		}
		AIVirtualField selfField = owner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		if (!IsCertainlyIncludeTarget(owner, candidate, situation))
		{
			return false;
		}
		int damage = GetDamage(owner, selfField, bestPlayPtn, situation);
		int count = GetCount(owner, selfField, bestPlayPtn, situation);
		return candidate.SimulateDamageAmount(damage, isSkillDamage: true, owner.IsSpell) * count >= candidate.Life;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		int damage = GetDamage(tagOwner, field, playPtn, situation);
		int count = GetCount(tagOwner, field, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int j = 0; j < count; j++)
			{
				AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int i = 0; i < count; i++)
			{
				AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
			}
			break;
		}
		}
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Damage;
	}

	private int GetDamage(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damage == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoDamage error!! _damage is null");
			return 0;
		}
		return (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
	}

	private int GetCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_count == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoDamage error!! _count is null");
			return 0;
		}
		return (int)_count.EvalArg(tagOwner, playPtn, field, situation);
	}
}
