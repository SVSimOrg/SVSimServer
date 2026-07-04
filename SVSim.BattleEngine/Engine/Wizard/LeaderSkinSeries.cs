namespace Wizard;

public class LeaderSkinSeries : BaseSeriesData
{
	private int index;

	public string SetName { get; private set; }

	public bool IsLargeImage { get; private set; }

	public LeaderSkinSeries(string[] columns)
	{
		base.Id = int.Parse(columns[index++]);
		base.Name = ConvSeriesText(columns[index++]);
		SetName = ConvSeriesText(columns[index++]);
		base.Introduction = ConvSeriesText(columns[index++]);
		base.TitlePath = columns[index++];
		base.DrumrollPath = columns[index++];
		IsLargeImage = int.Parse(columns[index++]) == 1;
	}

	private string ConvSeriesText(string id)
	{
		return Data.Master.GetLeaderSkinSeriesText(id);
	}
}
