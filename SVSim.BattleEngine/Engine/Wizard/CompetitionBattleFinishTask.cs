namespace Wizard;

public class CompetitionBattleFinishTask : FinishTaskBase
{
	public CompetitionBattleFinishTask()
	{
		if (Data.ArenaData.CompetitionData.IsRankMatching)
		{
			base.type = ApiType.Type.CompetitionBattleFinishRankMatch;
		}
		else
		{
			base.type = ApiType.Type.CompetitionBattleFinish;
		}
		Data.CompetitionBattleFinish = null;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (IsEffectiveErrorCode(num))
		{
			return num;
		}
		Data.CompetitionBattleFinish = new CompetitionBattleFinish();
		if (!IsResponseDataExist(base.ResponseData))
		{
			return num;
		}
		CompetitionBattleFinishDetail detailData = Data.CompetitionBattleFinish.DetailData;
		new BattleFinishResponsProcessing().Processing(base.ResponseData, detailData);
		detailData.IsChampion = base.ResponseData["data"].GetValueOrDefault("is_champion", defaultValue: false);
		return num;
	}
}
