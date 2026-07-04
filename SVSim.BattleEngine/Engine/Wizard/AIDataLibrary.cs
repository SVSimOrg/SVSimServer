using System.Collections.Generic;

namespace Wizard;

public class AIDataLibrary
{
	private readonly AIDeckData _basicDic;

	private readonly AIDeckData _commonDic;

	private readonly AIDeckData _allyCommonDic;

	private readonly Dictionary<string, AIDeckData> _deckDic;

	private readonly AIStyleData _commonStyle;

	private readonly Dictionary<string, AIStyleData> _deckStyleDic;

	private AISetUpData setupInfoBuf;

	private readonly Dictionary<string, AIEmoteSet> emoteDic;

	public AISetUpData SetupInfoBuf => setupInfoBuf;

	public AIDataLibrary()
	{
		_basicDic = new AIDeckData();
		_commonDic = new AIDeckData();
		_allyCommonDic = new AIDeckData();
		_commonStyle = new AIStyleData();
		_deckStyleDic = new Dictionary<string, AIStyleData>();
		_deckDic = new Dictionary<string, AIDeckData>();
		emoteDic = new Dictionary<string, AIEmoteSet>();
	}

	public void SaveBattleSetUpInfo(int classID, AI_LOGIC_LV logicLv, string deckName, string styleName, string emoteName, bool useEmote, bool useInnerEmote, int enemyAiID, List<int> specialAbilityList)
	{
		setupInfoBuf = new AISetUpData(classID, logicLv, deckName, styleName, emoteName, useEmote, useInnerEmote, enemyAiID, specialAbilityList);
	}

	public AIDeckData SearchDeckData(string name)
	{
		if (!_deckDic.ContainsKey(name))
		{
			return null;
		}
		return _deckDic[name];
	}

	public AIStyleData SearchDeckStyle(string name)
	{
		if (!_deckStyleDic.ContainsKey(name))
		{
			return null;
		}
		return _deckStyleDic[name];
	}

	public AIEmoteSet SearchEmoteSet(string name)
	{
		if (!emoteDic.ContainsKey(name))
		{
			return null;
		}
		return emoteDic[name];
	}

	public AIDeckData GetCommonDic()
	{
		return _commonDic;
	}

	public AIDeckData GetAllyCommonDic()
	{
		return _allyCommonDic;
	}
}
