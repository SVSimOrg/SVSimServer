using System.Collections.Generic;

public class RegisterChoiceAdd : RegisterToken
{
	public List<int> ChoiceCandidates { get; private set; }

	public List<int> OverFlowIndexList { get; private set; }

	public RegisterChoiceAdd(BattleCardBase card, bool isChoice, NetworkBattleDefine.NetworkCardPlaceState status, SkillBase skill)
		: base(card, isChoice, status, skill)
	{
		IsSelf = card.IsPlayer;
		base.IndexList = new List<int>();
		OverFlowIndexList = new List<int>();
	}

	public void SettingChoiceCandidates(List<int> choiceCardIds)
	{
		ChoiceCandidates = choiceCardIds;
	}

	public void SettingChoiceData(List<int> choiceIndexs, NetworkBattleDefine.NetworkCardPlaceState to, int repeatCount)
	{
		foreach (int choiceIndex in choiceIndexs)
		{
			base.IndexList.Add(choiceIndex);
		}
		if (base.ToPlaceState == NetworkBattleDefine.NetworkCardPlaceState.None)
		{
			SetToPlace(to);
		}
		if (base.ToPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Cemetery && to == NetworkBattleDefine.NetworkCardPlaceState.Cemetery)
		{
			for (int i = 0; i < choiceIndexs.Count; i++)
			{
				OverFlowIndexList.Add(choiceIndexs[i]);
			}
		}
		RepeatCount = repeatCount;
	}

	protected override Dictionary<string, object> MakeCardData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(ActionBaseParameter.candidates.ToString(), ChoiceCandidates);
		if (RepeatCount != -1)
		{
			dictionary.Add(ActionBaseParameter.repeat.ToString(), RepeatCount);
		}
		return dictionary;
	}

	public override bool IsMatchData(RegisterToken baseToken, RegisterToken checkToken)
	{
		if (base.IsMatchData(baseToken, checkToken) && baseToken is RegisterChoiceAdd && checkToken is RegisterChoiceAdd)
		{
			RegisterChoiceAdd obj = baseToken as RegisterChoiceAdd;
			RegisterChoiceAdd registerChoiceAdd = checkToken as RegisterChoiceAdd;
			if (obj.ChoiceCandidates == registerChoiceAdd.ChoiceCandidates)
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
