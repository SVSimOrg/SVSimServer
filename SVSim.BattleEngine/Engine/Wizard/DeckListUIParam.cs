namespace Wizard;

public class DeckListUIParam
{
	public Format Format { get; private set; }

	public ConventionInfo ConventionInfo { get; private set; }

	public DeckListUIParam(Format format, ConventionInfo conventionInfo)
	{
		Format = format;
		ConventionInfo = conventionInfo;
	}
}
