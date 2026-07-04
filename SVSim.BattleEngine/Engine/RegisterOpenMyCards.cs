using System.Collections.Generic;

public class RegisterOpenMyCards : RegisterActionBase
{
	private bool _notBuff;

	public RegisterOpenMyCards(List<BattleCardBase> cards, bool notBuff = false)
	{
		for (int i = 0; i < cards.Count; i++)
		{
			base.IndexList.Add(cards[i].Index);
		}
		_notBuff = notBuff;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		if (_notBuff)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameter.notBuff.ToString(), 1);
		}
		dictionary.Remove(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf]);
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.openMyCards.ToString();
	}
}
