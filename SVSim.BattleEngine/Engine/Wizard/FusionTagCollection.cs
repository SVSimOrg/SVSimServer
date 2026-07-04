using System;
using System.Collections.Generic;

namespace Wizard;

public class FusionTagCollection : TagCollection, IAITargetSelectTagCollection
{
	public FusionTagCollection()
		: base(TagCollectionType.Fusion)
	{
	}

	private FusionTagCollection(FusionTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new FusionTagCollection(this);
	}

	public void AddSelectInfoToSelectInfoList(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, ref List<AIVirtualTargetSelectInfo> selectInfoList)
	{
		throw new NotImplementedException();
	}

	public AIVirtualTargetSelectInfo GetSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType selectGroup = AIScriptTokenArgType.TARGET_SELECT)
	{
		List<AIVirtualCard> materials = (owner.IsAlly ? field.AllyHandCards : field.GetEnemyHandCardList());
		List<AIVirtualCard> selectableCards = GetSelectableCards(situation, field, materials, EnemyAI.EmptyPlayPtn);
		if (selectableCards == null)
		{
			return null;
		}
		return new AIVirtualTargetSelectInfo(-1, selectableCards, TargetSelectType.Default, isForbiddenSelectedTarget: true);
	}

	private List<AIVirtualCard> GetSelectableCards(AISituationInfo situation, AIVirtualField field, List<AIVirtualCard> materials, List<int> playPtn)
	{
		AIVirtualCard actor = situation.Actor;
		if (!base.HasTag || actor.IsOnField || actor.IsDead)
		{
			return null;
		}
		List<AIVirtualCard> list = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.CheckCondition(actor, playPtn, field, situation) || !(aIPlayTag.ArgumentExpressions is AIFiltersArgument aIFiltersArgument))
			{
				continue;
			}
			List<AIVirtualCard> filteredTargets = aIFiltersArgument.GetFilteredTargets(materials, actor, playPtn, situation);
			if (filteredTargets != null && filteredTargets.Count > 0)
			{
				filteredTargets.Remove(actor);
				if (list == null)
				{
					list = new List<AIVirtualCard>();
				}
				list.AddRange(filteredTargets);
			}
		}
		return list;
	}

	public bool InitializeFusionSituationParameter(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AIFusionSituationInfo fusion)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, fusion))
			{
				if (aIPlayTag.ArgumentExpressions is AIFusion aIFusion)
				{
					aIFusion.SetFusionSituationParameter(fusion);
					return true;
				}
				AIConsoleUtility.LogError("FusionTagCollection.InitializeFusionSituationParameter() Error!! arg is not AIFusion!!!!!");
			}
		}
		return false;
	}
}
