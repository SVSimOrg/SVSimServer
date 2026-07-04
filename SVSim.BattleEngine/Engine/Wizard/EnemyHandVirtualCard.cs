using System.Collections.Generic;

namespace Wizard;

public class EnemyHandVirtualCard : AIVirtualCard
{
	public EnemyHandVirtualCard(BattleCardBase card, AIVirtualField field)
	{
		_field = field;
		base.IsInHand = true;
		base.IsOnField = false;
		base.IsAlly = false;
		InitializeFromBattleCardBase(card);
		base.TagCollectionContainer = new AIEnemyHandTagCollectionContainer();
		base.FusionIngredients = new AIVirtualFusionIngredientsInfo();
		if (card.SkillApplyInformation.FusionIngredients != null && card.SkillApplyInformation.FusionIngredients.Count > 0)
		{
			base.FusionIngredients.CopyFusionIngredientsFromBattleCard(card.SkillApplyInformation.FusionIngredients, _field);
		}
	}

	public EnemyHandVirtualCard(EnemyHandVirtualCard original, AIVirtualField field)
	{
		base.BaseCard = original.BaseCard;
		_field = field;
		base.IsInHand = true;
		base.IsOnField = false;
		base.IsAlly = false;
		base.CardIndex = original.CardIndex;
		base.BaseId = original.BaseId;
		base.Clan = original.Clan;
		IsPlayer = original.IsPlayer;
		IsUnit = original.IsUnit;
		IsSpell = original.IsSpell;
		IsAmulet = original.IsAmulet;
		IsCountdownAmulet = original.IsCountdownAmulet;
		base.TagCollectionContainer = new AIEnemyHandTagCollectionContainer(original.TagCollectionContainer, this);
		if (original._tribeList != null)
		{
			_tribeList = new List<CardBasePrm.TribeType>(original._tribeList);
		}
		if (original.FusionIngredients != null)
		{
			base.FusionIngredients = new AIVirtualFusionIngredientsInfo(original.FusionIngredients);
		}
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		Cost = origin.Cost;
		base.CardIndex = origin.Index;
		IsPlayer = origin.IsPlayer;
		base.BattleSkills = origin.Skills;
		base.BattleSkillHashList = CardSkillHashUtility.GetSingleSkillHashStringList(base.BattleSkills);
		IsUnit = origin.IsUnit;
		IsSpell = origin.IsSpell;
		IsAmulet = origin.IsField;
		IsCountdownAmulet = origin.IsChantField;
		if (origin.Tribe != null && origin.Tribe.Count > 0)
		{
			for (int i = 0; i < origin.Tribe.Count; i++)
			{
				AppendTribe(origin.Tribe[i]);
			}
		}
	}

	public override void InitializeTags(AIParamQuery query, AIAttachedTagCollection attachedTagCollection, AIRemovedTagCollection removedTagCollection)
	{
		base.TagCollectionContainer.InitTags(this, query);
	}

	public override bool IsFollower(List<int> playPtn)
	{
		if (IsUnit)
		{
			if (!this.IsAccelerated(_field, null))
			{
				return !this.IsCrystalize(_field, null);
			}
			return false;
		}
		return false;
	}
}
