namespace Wizard;

public class CompetitionBattleFinish : HeaderData
{
	public CompetitionBattleFinishDetail DetailData { get; }

	public CompetitionBattleFinish()
	{
		DetailData = new CompetitionBattleFinishDetail();
	}
}
