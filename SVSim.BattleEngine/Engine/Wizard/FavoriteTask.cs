namespace Wizard;

public class FavoriteTask : BaseTask
{
	public enum Kind
	{
		SLEEVE,
		EMBLEM,
		DEGREE
	}

	public class FavoriteTaskParam : BaseParam
	{
	}

	public FavoriteTask(Kind kind)
	{
		switch (kind)
		{
		case Kind.SLEEVE:
			base.type = ApiType.Type.SleeveFavorite;
			break;
		case Kind.EMBLEM:
			base.type = ApiType.Type.EmblemFavorite;
			break;
		case Kind.DEGREE:
			base.type = ApiType.Type.DegreeFavorite;
			break;
		}
	}

	protected override int Parse()
	{
		int result = base.Parse();
		_ = 1;
		return result;
	}
}
