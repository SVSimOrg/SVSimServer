namespace Wizard;

public class ClassUtil
{
	public static ClassType GetClassType(CardParameter cardParam, Format format, ClassSet classSet)
	{
		if (classSet == null || classSet.SubClass == CardBasePrm.ClanType.NONE || !FormatBehaviorManager.GetDefaultBehaviour(format).UseSubClass)
		{
			return ClassType.None;
		}
		if (format == Format.Crossover)
		{
			if (cardParam.Clan != classSet.SubClass)
			{
				return ClassType.MainClass;
			}
			return ClassType.SubClass;
		}
		return ClassType.None;
	}
}
