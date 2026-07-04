public class RegisterSpecialWin : RegisterActionBase
{
	public RegisterSpecialWin(bool isSelf)
	{
		IsSelf = isSelf;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.specialWin.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
