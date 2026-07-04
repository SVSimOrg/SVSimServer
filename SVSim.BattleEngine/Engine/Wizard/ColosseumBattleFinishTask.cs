namespace Wizard;

public class ColosseumBattleFinishTask : FinishTaskBase
{
	public ColosseumBattleFinishTask()
	{
		if (!Data.ArenaData.ColosseumData.IsRankMatching)
		{
			base.type = ApiType.Type.ColosseumBattleFinish;
		}
		else
		{
			base.type = ApiType.Type.ColosseumBattleFinishRankMatch;
		}
		Data.ColosseumBattleFinish = null;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (IsEffectiveErrorCode(num))
		{
			return num;
		}
		Data.ColosseumBattleFinish = new ColosseumBattleFinish();
		if (!IsResponseDataExist(base.ResponseData))
		{
			return num;
		}
		ColosseumBattleFinishDetail colosseumBattleFinishDetail = new ColosseumBattleFinishDetail();
		Data.ColosseumBattleFinish.data = colosseumBattleFinishDetail;
		new BattleFinishResponsProcessing().Processing(base.ResponseData, colosseumBattleFinishDetail);
		return num;
	}
}
