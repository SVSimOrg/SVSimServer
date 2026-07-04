using System.Collections.Generic;

public class RegisterShortageDeckWin : RegisterActionBase
{

	public RegisterShortageDeckWin(bool isSelf)
	{
		IsSelf = isSelf;
		base.IndexList = null;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		return base.MakeSendData();
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.reverseDeckOut.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
