using System.Collections.Generic;

namespace Wizard;

public class MyRotationAbilityGroup
{
	public List<MyRotationInfo.MyRotationBonus> AbilityList;

	public int StartPackNumber { get; }

	public int LastPackNumber { get; }

	public string StartPackShortName { get; }

	public string LastPackShortName { get; }

	public MyRotationAbilityGroup(int startNumber, int lastNumber, string startPackShorName, string lastPackShortName, List<MyRotationInfo.MyRotationBonus> list)
	{
		StartPackNumber = startNumber;
		LastPackNumber = lastNumber;
		StartPackShortName = startPackShorName;
		LastPackShortName = lastPackShortName;
		AbilityList = list;
	}
}
