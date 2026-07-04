using System.Collections.Generic;

public class BattleLifeTimeSharedObject
{
	private Dictionary<string, SkillCreator.SkillBuildInfo> _skillBuildInfoSharedObject;

	public BattleLifeTimeSharedObject()
	{
		_skillBuildInfoSharedObject = new Dictionary<string, SkillCreator.SkillBuildInfo>();
	}

	public void SetSkillBuildInfo(string key, SkillCreator.SkillBuildInfo value)
	{
		if (!_skillBuildInfoSharedObject.ContainsKey(key))
		{
			_skillBuildInfoSharedObject.Add(key, value);
		}
	}

	public SkillCreator.SkillBuildInfo GetSkillBuildInfo(string key)
	{
		return _skillBuildInfoSharedObject.GetValueOrDefault(key, null);
	}
}
