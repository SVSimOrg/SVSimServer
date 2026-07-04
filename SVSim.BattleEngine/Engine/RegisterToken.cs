using System.Collections.Generic;

public class RegisterToken : RegisterActionBase
{
	public int RepeatCount = -1;

	protected bool _isChoice;

	public BattleCardBase CardObj { get; private set; }

	public NetworkBattleDefine.NetworkCardPlaceState ToPlaceState { get; private set; }

	public SkillBase Skill { get; private set; }

	public RegisterToken(BattleCardBase card, bool isChoice, NetworkBattleDefine.NetworkCardPlaceState status, SkillBase skill, int repeatCount = -1)
	{
		CardObj = card;
		ToPlaceState = status;
		Skill = skill;
		base.IndexList = new List<int>();
		base.IndexList.Add(CardObj.Index);
		IsSelf = CardObj.IsPlayer;
		RepeatCount = repeatCount;
		_isChoice = isChoice;
	}

	public void SetToPlace(NetworkBattleDefine.NetworkCardPlaceState place)
	{
		ToPlaceState = place;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(ActionBaseParameter.card.ToString(), MakeCardData());
		if (_isChoice)
		{
			dictionary.Add(ActionBaseParameter.isChoice.ToString(), "1");
		}
		return dictionary;
	}

	protected virtual Dictionary<string, object> MakeCardData()
	{
		return new Dictionary<string, object> { 
		{
			ActionBaseParameter.cardId.ToString(),
			CardObj.CardId
		} };
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.add.ToString();
	}

	public virtual bool IsMatchData(RegisterToken baseToken, RegisterToken checkToken)
	{
		if (baseToken.CardObj.IsPlayer == checkToken.CardObj.IsPlayer && baseToken.CardObj.CardId == checkToken.CardObj.CardId && baseToken.ToPlaceState == checkToken.ToPlaceState)
		{
			return true;
		}
		return false;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
