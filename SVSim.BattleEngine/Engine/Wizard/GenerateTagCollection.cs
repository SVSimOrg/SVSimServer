namespace Wizard;

public class GenerateTagCollection : TagCollection
{
	public GenerateTagCollection()
		: base(TagCollectionType.GenerateTag)
	{
	}

	private GenerateTagCollection(GenerateTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new GenerateTagCollection(this);
	}
}
