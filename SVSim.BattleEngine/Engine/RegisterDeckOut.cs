public class RegisterDeckOut : RegisterActionBase
{

	public RegisterDeckOut(bool isSelf)
	{
		IsSelf = isSelf;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.deckOut.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
