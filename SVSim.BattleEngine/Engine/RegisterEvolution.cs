public class RegisterEvolution : RegisterActionBase
{
	public RegisterEvolution(bool isSelf, int idx)
	{
		IsSelf = isSelf;
		base.IndexList.Add(idx);
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.evolution.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}

	public void AddIndex(int idx)
	{
		base.IndexList.Add(idx);
	}
}
