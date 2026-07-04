using System.Collections.Generic;

namespace Wizard;

public class AISetUpData
{
	public int classID;

	public AI_LOGIC_LV logicLv;

	public string deckName;

	public string styleName;

	public string emoteName;

	public bool doesUseEmote;

	public bool useInnerEmote;

	public int enemyAiID;

	public List<int> specialAbilityList;

	public AISetUpData(int _classID, AI_LOGIC_LV _logicLv, string _deckName, string _styleName, string _emoteName, bool _doesUseEmote, bool _useInnerEmote, int _enemyAiID, List<int> _specialAbilityList = null)
	{
		classID = _classID;
		logicLv = _logicLv;
		deckName = _deckName;
		styleName = _styleName;
		emoteName = _emoteName;
		doesUseEmote = _doesUseEmote;
		useInnerEmote = _useInnerEmote;
		enemyAiID = _enemyAiID;
		specialAbilityList = _specialAbilityList;
	}
}
