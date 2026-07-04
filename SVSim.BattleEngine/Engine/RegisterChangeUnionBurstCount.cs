using System.Collections.Generic;
using System.Linq;

public class RegisterChangeUnionBurstCount : RegisterAlter
{
	private int _gainValue;

	private readonly string UnionBurstCountParameter = "unionburst";

	public RegisterChangeUnionBurstCount(List<BattleCardBase> cardList, SkillBase skill, int gainCount)
		: base(cardList, skill)
	{
		base.IndexList = new List<int>();
		if (cardList != null && cardList.Count > 0)
		{
			cardList.ForEach(delegate(BattleCardBase x)
			{
				if (!base.IndexList.Any((int z) => z == x.Index))
				{
					base.IndexList.Add(x.Index);
				}
			});
		}
		_gainValue = gainCount;
		IsSelf = skill.SkillPrm.ownerCard.IsPlayer;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(ActionBaseParameter.type.ToString(), ActionBaseParameter.add.ToString());
		dictionary.Add(UnionBurstCountParameter, "a+" + _gainValue);
		return MakeAttachTarget(dictionary);
	}
}
