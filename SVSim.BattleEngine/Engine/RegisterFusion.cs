using System.Collections.Generic;
using System.Linq;

public class RegisterFusion : RegisterAlter
{
	public RegisterFusion(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards)
		: base(ingredientCards, fusionCard.Skills.FirstOrDefault((SkillBase s) => s is Skill_fusion))
	{
		base.IndexList.Add(fusionCard.Index);
		IsSelf = fusionCard.IsPlayer;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		List<int> value = _cardList.Select((BattleCardBase m) => m.Index).ToList();
		dictionary.Add(ActionBaseParameter.ingredients.ToString(), value);
		return MakeAttachTarget(dictionary);
	}

	public override string GetUriMsg()
	{
		return "fusion";
	}
}
