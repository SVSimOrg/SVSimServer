using LitJson;

namespace Wizard.Scripts.Network.Data.TaskData.Arena;

public class Reward
{
	public int num;

	public int _effectId;

	public string _name;

	public UserGoods UserGoodsData { get; private set; }

	public Reward(JsonData data)
	{
		num = data["reward_number"].ToInt();
		if (data.Keys.Contains("effect_id"))
		{
			_effectId = data["effect_id"].ToInt();
		}
		if (data.Keys.Contains("name"))
		{
			_name = data["name"].ToString();
		}
		UserGoodsData = new UserGoods((UserGoods.Type)data["reward_type"].ToInt(), data["reward_detail_id"].ToLong());
	}
}
