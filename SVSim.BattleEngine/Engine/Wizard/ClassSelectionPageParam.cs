using System.Collections.Generic;

namespace Wizard;

public class ClassSelectionPageParam
{
	public ClassSelectionPage.eMode Mode { get; private set; }

	public Format Format { get; private set; }

	public ConventionInfo ConventionInfo { get; private set; }

	public List<int> UsedClassIdList { get; private set; }

	public static ClassSelectionPageParam CreateStorySelect()
	{
		return new ClassSelectionPageParam(ClassSelectionPage.eMode.StorySelect, Format.Max, null, new List<int>());
	}

	private ClassSelectionPageParam(ClassSelectionPage.eMode mode, Format format, ConventionInfo conventionInfo, List<int> usedClassIdList)
	{
		Mode = mode;
		Format = format;
		ConventionInfo = conventionInfo;
		UsedClassIdList = usedClassIdList;
	}
}
