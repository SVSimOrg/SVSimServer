namespace Wizard;

public class PlayoutNextTurnTagCollection : TagCollection
{
	public PlayoutNextTurnTagCollection()
		: base(TagCollectionType.PlayoutNextTurn)
	{
	}

	private PlayoutNextTurnTagCollection(PlayoutNextTurnTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayoutNextTurnTagCollection(this);
	}
}
