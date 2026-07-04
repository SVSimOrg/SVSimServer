namespace Wizard;

public class CardSetName
{
	public string ID { get; }

	public string LongName { get; }

	public string ShortName { get; }

	public CardSetName(string id, string longName, string shortName)
	{
		ID = id;
		LongName = longName;
		ShortName = shortName;
	}
}
