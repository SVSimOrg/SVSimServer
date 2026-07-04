using System.Collections.Generic;

public class RegisterExtraTurn : RegisterActionBase
{
	private int _extraTurnNum;

	public RegisterExtraTurn(int turnNum, bool isSelf)
	{
		_extraTurnNum = turnNum;
		IsSelf = isSelf;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		return new Dictionary<string, object>
		{
			{
				RegisterTool.OrderListParameter.count.ToString(),
				_extraTurnNum
			},
			{
				NetworkBattleDefine.NetworkParameter.isSelf.ToString(),
				IsSelf ? 1 : 0
			}
		};
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.exTurn.ToString();
	}
}
