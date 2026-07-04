using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class EntryInfo
{
	public int rewardScheduleId;

	public int maxBattleCount;

	public bool isRetire;

	public EntryInfo()
	{
	}

	public EntryInfo(JsonData data)
	{
		if (data != null)
		{
			rewardScheduleId = data["reward_schedule_id"].ToInt();
			maxBattleCount = data["max_battle_count"].ToInt();
			isRetire = data["is_retire"].ToInt() == 1;
		}
	}
}
