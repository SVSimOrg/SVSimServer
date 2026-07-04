using System.Linq;

public class TurnPlayerInfo
{
	public string TargetPlayer { get; private set; }

	public int TurnOffset { get; private set; }

	public bool IsAllTurn { get; private set; }

	public bool IsSelfPlayer { get; private set; }

	public bool IsOpponentPlayer { get; private set; }

	public bool IsOther { get; private set; }

	public TurnPlayerInfo(string player, int turnOffset, bool isAllTurn = false)
	{
		TargetPlayer = player;
		TurnOffset = (isAllTurn ? (-1) : turnOffset);
		IsAllTurn = isAllTurn;
		IsSelfPlayer = TargetPlayer == SkillFilterCreator.ContentKeyword.me.ToStringCustom();
		IsOpponentPlayer = TargetPlayer == SkillFilterCreator.ContentKeyword.op.ToStringCustom();
		IsOther = TargetPlayer == SkillFilterCreator.ContentKeyword.other.ToStringCustom();
	}

	public TurnPlayerInfo(string option)
	{
		string[] array = option.Split(':');
		array.Count();
		_ = 2;
		TargetPlayer = array[0];
		TurnOffset = -1;
		IsAllTurn = array[1] == SkillFilterCreator.ContentKeyword.all.ToStringCustom();
		IsSelfPlayer = TargetPlayer == SkillFilterCreator.ContentKeyword.me.ToStringCustom();
		IsOpponentPlayer = TargetPlayer == SkillFilterCreator.ContentKeyword.op.ToStringCustom();
		IsOther = TargetPlayer == SkillFilterCreator.ContentKeyword.other.ToStringCustom();
		if (!IsAllTurn && int.TryParse(array[1], out var result))
		{
			TurnOffset = result;
		}
	}
}
