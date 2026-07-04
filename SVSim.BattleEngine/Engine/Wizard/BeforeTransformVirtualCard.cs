namespace Wizard;

public class BeforeTransformVirtualCard : AIVirtualCard
{
	public BeforeTransformVirtualCard(BattleCardBase card, AIVirtualField field)
	{
		Initialize(card, field);
		base.FusionIngredients = new AIVirtualFusionIngredientsInfo();
		if (card.SkillApplyInformation.FusionIngredients != null && card.SkillApplyInformation.FusionIngredients.Count > 0)
		{
			base.FusionIngredients.CopyFusionIngredientsFromBattleCard(card.SkillApplyInformation.FusionIngredients, _field);
		}
	}

	public BeforeTransformVirtualCard(BeforeTransformVirtualCard virtualCard, AIVirtualField field)
	{
		Initialize(virtualCard.BaseCard, field);
		if (virtualCard.FusionIngredients != null)
		{
			base.FusionIngredients = new AIVirtualFusionIngredientsInfo(virtualCard.FusionIngredients);
		}
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		Cost = origin.Cost;
		base.CardIndex = origin.Index;
		IsPlayer = origin.IsPlayer;
		if (base.BaseCard.Tribe != null && base.BaseCard.Tribe.Count > 0)
		{
			for (int i = 0; i < base.BaseCard.Tribe.Count; i++)
			{
				AppendTribe(base.BaseCard.Tribe[i]);
			}
		}
	}

	private void Initialize(BattleCardBase card, AIVirtualField field)
	{
		_field = field;
		base.IsAlly = true;
		InitializeFromBattleCardBase(card);
	}
}
