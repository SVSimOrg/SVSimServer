using Wizard;

public class RewardObjectInfo
{
	public UserGoods.Type GoodsType { get; private set; }

	public NguiObjs ObjectData { get; private set; }

	public RewardObjectInfo(UserGoods.Type goodsType, NguiObjs objectData)
	{
		ObjectData = objectData;
		GoodsType = goodsType;
	}
}
