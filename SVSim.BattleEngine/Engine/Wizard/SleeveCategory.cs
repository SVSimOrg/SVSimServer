namespace Wizard;

public class SleeveCategory
{
	public int Id { get; }

	public string Name { get; }

	public SleeveCategory(string[] columns)
	{
		int num = 0;
		Id = int.Parse(columns[num++]);
		Name = ConvertToCategoryText(columns[num++]);
	}

	private string ConvertToCategoryText(string textId)
	{
		return Data.Master.GetSleeveCategoryText(textId);
	}
}
