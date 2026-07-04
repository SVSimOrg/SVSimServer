public class FusionIngredientInfo
{
	public readonly int FusionTurn;

	public readonly BattleCardBase Card;

	public FusionIngredientInfo(int turn, BattleCardBase card)
	{
		FusionTurn = turn;
		Card = card;
	}

	public FusionIngredientInfo Clone()
	{
		return new FusionIngredientInfo(FusionTurn, Card);
	}
}
