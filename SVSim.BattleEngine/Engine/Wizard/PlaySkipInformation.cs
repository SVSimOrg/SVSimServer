namespace Wizard;

public class PlaySkipInformation
{
	public AIPlayTagType TagType;

	public bool IsEvolutionPermittedTag { get; protected set; }

	public PlaySkipInformation()
	{
		TagType = AIPlayTagType.PlaySkip;
		IsEvolutionPermittedTag = false;
	}

	public virtual bool IsEvoCardLegal(AIVirtualCard evoCard)
	{
		if (evoCard == null)
		{
			return true;
		}
		return false;
	}
}
