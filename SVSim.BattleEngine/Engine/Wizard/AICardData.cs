using System.Collections.Generic;

namespace Wizard;

public class AICardData
{
	private List<AIPlayTag> tagList;

	public int CardID { get; private set; }

	public int CardNum { get; private set; }

	public List<AIPlayTag> TagList => tagList;

	public AIPolishConvertedExpression BattleBonusExpr { get; set; }

	public AIPolishConvertedExpression PlayBonusExpr { get; set; }

	public AIPolishConvertedExpression PriorityExpr { get; set; }

	private string BattleBonus { get; set; }

	private string PlayBonus { get; set; }

	private string Priority { get; set; }

	public AICardData(AICardDataAsset asset)
	{
		CardID = asset.CardID;
		CardNum = asset.CardNum;
		BattleBonus = asset.BattleBonus;
		PlayBonus = asset.PlayBonus;
		Priority = asset.Priority;
		tagList = new List<AIPlayTag>();
		foreach (AIPlayTagAsset tag in asset.TagList)
		{
			AIPlayTag aIPlayTag = new AIPlayTag();
			if (aIPlayTag.InitFromTextAsset(tag))
			{
				tagList.Add(aIPlayTag);
			}
			else
			{
				aIPlayTag = null;
			}
		}
		BattleBonusExpr = new AIPolishConvertedExpression(asset.BattleBonus);
		PlayBonusExpr = new AIPolishConvertedExpression(asset.PlayBonus);
		PriorityExpr = new AIPolishConvertedExpression(asset.Priority);
	}
}
