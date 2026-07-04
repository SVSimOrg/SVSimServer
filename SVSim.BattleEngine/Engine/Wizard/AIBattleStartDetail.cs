using System.Collections.Generic;

namespace Wizard;

public class AIBattleStartDetail
{
	public class UserInfo
	{
		public Dictionary<string, object> DataDictionary;

		public UserInfo()
		{
			DataDictionary = new Dictionary<string, object>();
		}
	}

	public UserInfo SelfInfo;

	public UserInfo OppoInfo;

	public AIBattleStartDetail()
	{
		SelfInfo = new UserInfo();
		OppoInfo = new UserInfo();
	}
}
