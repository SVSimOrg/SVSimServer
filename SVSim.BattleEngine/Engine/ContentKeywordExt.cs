internal static class ContentKeywordExt
{
	public static string ToStringCustom(this SkillFilterCreator.ContentKeyword type)
	{
		return type switch
		{
			SkillFilterCreator.ContentKeyword._class => "class", 
			SkillFilterCreator.ContentKeyword._true => "true", 
			SkillFilterCreator.ContentKeyword._false => "false", 
			SkillFilterCreator.ContentKeyword._ref_prev => "<-", 
			_ => type.ToString(), 
		};
	}
}
