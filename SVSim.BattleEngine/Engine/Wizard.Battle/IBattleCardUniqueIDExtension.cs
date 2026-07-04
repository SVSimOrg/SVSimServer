namespace Wizard.Battle;

public static class IBattleCardUniqueIDExtension
{
	public static bool EquelsID(this IBattleCardUniqueID lhs, IBattleCardUniqueID rhs)
	{
		if (lhs == null || rhs == null)
		{
			return false;
		}
		if (lhs.IsPlayer == rhs.IsPlayer)
		{
			return lhs.Index == rhs.Index;
		}
		return false;
	}

	public static string GetName(this IBattleCardUniqueID cardId)
	{
		return (cardId.IsPlayer ? "p" : "e") + cardId.Index;
	}
}
