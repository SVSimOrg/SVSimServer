using System.Collections.Generic;

namespace Wizard;

public class SealedClassInfo
{
	public int ClassId { get; private set; }

	public List<SealedCardInfo> PublishedCardInfoList { get; private set; }

	public SealedClassInfo(int classId, List<SealedCardInfo> publishedCardInfoList)
	{
		ClassId = classId;
		PublishedCardInfoList = publishedCardInfoList;
	}
}
