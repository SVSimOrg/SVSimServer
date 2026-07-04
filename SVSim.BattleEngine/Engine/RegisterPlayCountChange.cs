using System.Collections.Generic;

public class RegisterPlayCountChange : RegisterActionBase
{
	public enum PlayCountParameter
	{
		count,
		isSelf
	}

	public int PlayCount { get; private set; }

	public RegisterPlayCountChange(bool isSelf, int playCount)
	{
		IsSelf = isSelf;
		PlayCount = playCount;
		base.IndexList = null;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(PlayCountParameter.count.ToString(), PlayCount);
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.playCount.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
