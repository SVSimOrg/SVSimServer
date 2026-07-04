public class InitializeRoomBattle : HeaderData
{
	public class Result
	{
		public int _classId;

		public EventBattleResult _result;

		public Result(int classId, EventBattleResult result)
		{
			_classId = classId;
			_result = result;
		}
	}

	public class LotteryDataInRoom
	{
	}
}
