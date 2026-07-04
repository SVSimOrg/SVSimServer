using System.Collections.Generic;

namespace Wizard;

public class AILastwordDamageClip : AIFiltersAndSelectTypeArgument
{
	private AIScriptTokenArgType _stopTiming;

	private AIScriptTokenArgType _damageType;

	private AIPolishConvertedExpression _clipAmount;

	protected bool _isDamageTypeDefinedByMaster;

	protected override int SELECT_TYPE_OFFSET => 1 + (_isDamageTypeDefinedByMaster ? 3 : 2);

	public AILastwordDamageClip(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_clipAmount = _exprList[_exprList.Count - 1];
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - 3], out _isDamageTypeDefinedByMaster);
		InitSelectType();
		InitializeFilter();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBarrierSimulationUtility.AddDamageClipToAll(targetsFromField, tagOwner, field, _damageType, _stopTiming, clipAmount);
			}
		}
	}
}
