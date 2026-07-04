using System.Collections.Generic;

namespace Wizard;

public class AIVirtualFusionIngredientsInfo
{
	public List<int> FusionTurnList { get; private set; }

	public List<AIVirtualCard> CardList { get; private set; }

	public int FusionCount
	{
		get
		{
			if (CardList != null)
			{
				return CardList.Count;
			}
			return 0;
		}
	}

	public bool HasFusionIngredients
	{
		get
		{
			if (CardList != null)
			{
				return CardList.Count > 0;
			}
			return false;
		}
	}

	public AIVirtualFusionIngredientsInfo()
	{
		Initialize();
	}

	public void CopyFusionIngredientsFromBattleCard(List<FusionIngredientInfo> fusionIngredients, AIVirtualField field)
	{
		if (fusionIngredients != null && fusionIngredients.Count > 0)
		{
			if (CardList == null || FusionTurnList == null)
			{
				CardList = new List<AIVirtualCard>();
				FusionTurnList = new List<int>();
			}
			for (int i = 0; i < fusionIngredients.Count; i++)
			{
				FusionIngredientInfo fusionIngredientInfo = fusionIngredients[i];
				CardList.Add(new AIVirtualCard(fusionIngredientInfo.Card, field));
				FusionTurnList.Add(fusionIngredientInfo.FusionTurn);
			}
		}
	}

	public AIVirtualFusionIngredientsInfo(AIVirtualFusionIngredientsInfo info)
	{
		if (info.HasFusionIngredients)
		{
			FusionTurnList = new List<int>(info.FusionTurnList);
			CardList = new List<AIVirtualCard>(info.CardList);
		}
		else
		{
			Initialize();
		}
	}

	public void AddFusionIngredient(AIVirtualCard ingredient, int turn)
	{
		CardList = AIParamQuery.AddElementToList(ingredient, CardList);
		FusionTurnList = AIParamQuery.AddElementToList(turn, FusionTurnList);
	}

	public ulong GetHash()
	{
		return (ulong)(0 + (long)FusionCount * 55351L);
	}

	private void Initialize()
	{
		FusionTurnList = null;
		CardList = null;
	}
}
