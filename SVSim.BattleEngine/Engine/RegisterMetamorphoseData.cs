using System.Collections.Generic;
using System.Linq;
using Wizard;

public class RegisterMetamorphoseData : RegisterActionBase
{
	public enum MetamorphoseListParameter
	{
		idx,
		after,
		cardId,
		isSelf,
		isChoice,
		isFirstOnly,
		isFusion
	}

	protected SkillBase _skill;

	public int Index => base.IndexList[0];

	public int AfterId { get; private set; }

	public bool IsChoice { get; private set; }

	public bool IsFirstOnly { get; private set; }

	public bool IsFusion { get; private set; }

	public RegisterMetamorphoseData(int cardId, int index, bool isSelf, SkillBase skill, bool isChoice = false, bool isFirstOnly = false, bool isFusion = false)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
		base.IndexList = new List<int>();
		base.IndexList.Add(index);
		AfterId = cardParameterFromId.CardId;
		IsSelf = isSelf;
		_skill = skill;
		IsChoice = isChoice;
		IsFirstOnly = isFirstOnly;
		IsFusion = isFusion;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add(MetamorphoseListParameter.cardId.ToString(), AfterId);
		dictionary.Add(MetamorphoseListParameter.after.ToString(), dictionary2);
		if (IsChoice)
		{
			dictionary.Add(MetamorphoseListParameter.isChoice.ToString(), 1);
		}
		if (IsFirstOnly)
		{
			dictionary.Add(MetamorphoseListParameter.isFirstOnly.ToString(), 1);
		}
		if (IsFusion)
		{
			dictionary.Add(MetamorphoseListParameter.isFusion.ToString(), 1);
		}
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.metamorphose.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		if (base.IndexList.Count == 0)
		{
			return false;
		}
		if (base.IndexList.All((int x) => lot.IndexList.Contains(x)) && IsSelf == lot.IsSelf)
		{
			return _skill == lot.Skill;
		}
		return false;
	}
}
