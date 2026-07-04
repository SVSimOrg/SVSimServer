using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class PuppetAttackTagCollection : TagCollection
{
	public PuppetAttackParam Param;

	public PuppetAttackTagCollection()
		: base(TagCollectionType.PuppetAttack)
	{
	}

	private PuppetAttackTagCollection(PuppetAttackTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.Param != null)
		{
			Param = new PuppetAttackParam(tagCollection.Param.Attack, tagCollection.Param.Life, tagCollection.Param.Times);
		}
	}

	public override TagCollection Clone()
	{
		return new PuppetAttackTagCollection(this);
	}

	public void CreateParam(AIVirtualCard tagOwner, List<int> playPtn)
	{
		Param = GetPuppetAttackParam(tagOwner, playPtn);
	}

	private PuppetAttackParam GetPuppetAttackParam(AIVirtualCard tagOwner, List<int> playPtn)
	{
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, tagOwner.SelfField, null))
			{
				int attack = (int)aIPlayTag.EvalArg(tagOwner, playPtn, tagOwner.SelfField, null);
				int life = (int)aIPlayTag.EvalArg(tagOwner, playPtn, tagOwner.SelfField, null, 1);
				int times = (int)aIPlayTag.EvalArg(tagOwner, playPtn, tagOwner.SelfField, null, 2);
				return new PuppetAttackParam(attack, life, times);
			}
		}
		return null;
	}

	public void ExecutePuppetAttack(AIVirtualCard tagOwner, AIVirtualActionInfo currentAction, List<AIVirtualActionInfo> actionInfoSequence)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		if (!base.HasTag || selfField.AllyInplayCards.Count((AIVirtualCard card) => !card.IsDead) >= 5)
		{
			return;
		}
		if (Param == null)
		{
			CreateParam(tagOwner, selfField.BestPlayPtn);
		}
		PuppetAttackParam param = Param;
		if (param == null || param.Times <= 0)
		{
			return;
		}
		AIVirtualCard puppetToken = selfField.AI.tokenManager.GetPuppetToken(isAlly: true, selfField, needsClone: true);
		puppetToken.AttackableCount = 1;
		puppetToken.IsRush = true;
		puppetToken.IsSummonDrunkenness = false;
		puppetToken.InitAtSummonToken(tagOwner, currentAction, isSkillSummon: false);
		puppetToken.Attack = param.Attack;
		puppetToken.Life = param.Life;
		if (!puppetToken.TagCollectionContainer.HasTag(AIPlayTagType.PuppetAttack))
		{
			return;
		}
		PuppetAttackTagCollection puppetAttackTags = puppetToken.TagCollectionContainer.PuppetAttackTags;
		puppetAttackTags.CreateParam(puppetToken, selfField.BestPlayPtn);
		PuppetAttackParam param2 = puppetAttackTags.Param;
		if (param2 == null || param2.Times <= 0)
		{
			return;
		}
		param2.Times = param.Times - 1;
		selfField.AllyInplayCards.Add(puppetToken);
		selfField.CardListSet.BothClassAndInplayCards.Add(puppetToken);
		if (actionInfoSequence == null || currentAction == null)
		{
			return;
		}
		for (int num = 0; num < actionInfoSequence.Count; num++)
		{
			if (actionInfoSequence[num].Actor.CardIndex == tagOwner.CardIndex)
			{
				actionInfoSequence.Insert(num + 1, new AIVirtualAttackInfo(puppetToken, isAttackFollower: true, currentAction));
				break;
			}
		}
	}
}
