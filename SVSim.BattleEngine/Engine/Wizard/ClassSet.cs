namespace Wizard;

public class ClassSet
{
	public CardBasePrm.ClanType MainClass;

	public CardBasePrm.ClanType SubClass;

	public ClassSet(CardBasePrm.ClanType mainClass, CardBasePrm.ClanType subClass)
	{
		MainClass = mainClass;
		SubClass = subClass;
	}

	public ClassSet(CardBasePrm.ClanType mainClass)
		: this(mainClass, CardBasePrm.ClanType.NONE)
	{
	}

	public ClassSet(int mainClassId, int subClassId)
	{
		MainClass = (CardBasePrm.ClanType)mainClassId;
		SubClass = ((subClassId == 0) ? CardBasePrm.ClanType.NONE : ((CardBasePrm.ClanType)subClassId));
	}

	public void Swap()
	{
		CardBasePrm.ClanType mainClass = MainClass;
		MainClass = SubClass;
		SubClass = mainClass;
	}
}
