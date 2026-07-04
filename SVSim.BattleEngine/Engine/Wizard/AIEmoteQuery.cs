using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIEmoteQuery
{
	public enum Category
	{
		Awake = 401	}

	private EnemyAI enemyAI;

	private bool _isActivate = true;

	private AIEmoteSet _curEmoteSet;

	private bool _emotePermission;

	private bool _useInnerEmote;

	private Queue<int> _previousEmoteQueue;

	private Queue<int> _previousPlayerEmoteQueue;

	private Dictionary<int, int> _categoryIntervalDic;

	public bool UseInnerEmote => _useInnerEmote;

	public AIEmoteQuery(EnemyAI ai)
	{
		enemyAI = ai;
		_emotePermission = true;
		_useInnerEmote = false;
		_previousEmoteQueue = new Queue<int>(3);
		_previousPlayerEmoteQueue = new Queue<int>(3);
		_categoryIntervalDic = new Dictionary<int, int>();
	}

	public void SetOnOffEmote(bool isActivate, bool useInnerEmote)
	{
		_isActivate = isActivate;
		_useInnerEmote = useInnerEmote;
	}

	public void SetEmoteSet(AIEmoteSet emoteSet)
	{
		_curEmoteSet = emoteSet;
	}

	public void UpdateCategoryInterval()
	{
		foreach (int item in new List<int>(_categoryIntervalDic.Keys))
		{
			int num = _categoryIntervalDic[item];
			if (num > 0)
			{
				_categoryIntervalDic[item] = num - 1;
			}
		}
	}

	public void SetInterval(int emoteKey, int turnCount)
	{
		if (_categoryIntervalDic.ContainsKey(emoteKey))
		{
			_categoryIntervalDic[emoteKey] = turnCount;
		}
		else
		{
			_categoryIntervalDic.Add(emoteKey, turnCount);
		}
	}

	public int GetCategoryInterval(int emoteKey)
	{
		if (!_categoryIntervalDic.ContainsKey(emoteKey))
		{
			return 0;
		}
		return _categoryIntervalDic[emoteKey];
	}

	public AIEmoteCmd SearchEmoteAtRandom(int emoteKey, bool isAlly = true)
	{
		if (!_isActivate)
		{
			return null;
		}
		if (_curEmoteSet == null)
		{
			return null;
		}
		if (!_curEmoteSet.EmoteCmds.Any((AIEmoteCmd c) => c.CategoryKey == emoteKey))
		{
			return null;
		}
		if (!_emotePermission)
		{
			return null;
		}
		if (!_useInnerEmote && emoteKey <= 1000)
		{
			return null;
		}
		AIEmoteCmd aIEmoteCmd = null;
		IList<AIEmoteCmd> list = FilterEmoteByPreviousPlayed(_curEmoteSet.EmoteCmds.Where((AIEmoteCmd c) => c.CategoryKey == emoteKey), isAlly);
		int count = list.Count;
		if (count <= 0)
		{
			return null;
		}
		if (count > 1)
		{
			int index = enemyAI.AIStableRandom() % count;
			aIEmoteCmd = list[index];
		}
		else
		{
			aIEmoteCmd = list[0];
		}
		if (aIEmoteCmd != null)
		{
			Queue<int> queue = (isAlly ? _previousEmoteQueue : _previousPlayerEmoteQueue);
			if (queue.Count == 3)
			{
				queue.Dequeue();
			}
			queue.Enqueue(aIEmoteCmd.ID);
			aIEmoteCmd.isAI = isAlly;
		}
		return aIEmoteCmd;
	}

	public AIEmoteCmd SearchEmoteByID(int emoteID)
	{
		if (_curEmoteSet == null)
		{
			return null;
		}
		if (!_curEmoteSet.EmoteCmds.Any((AIEmoteCmd c) => c.ID == emoteID))
		{
			return null;
		}
		return _curEmoteSet.EmoteCmds.First((AIEmoteCmd c) => c.ID == emoteID);
	}

	public IEnumerable<AIEmoteCmd> SearchEmoteByCategory(int emoteCategory)
	{
		if (_curEmoteSet == null)
		{
			return null;
		}
		if (!_curEmoteSet.EmoteCmds.Any((AIEmoteCmd c) => c.CategoryKey == emoteCategory))
		{
			return null;
		}
		return _curEmoteSet.EmoteCmds.Where((AIEmoteCmd c) => c.CategoryKey == emoteCategory);
	}

	public void OnOperation()
	{
		_emotePermission = true;
	}

	public void OnCardPlayEmotion()
	{
		_emotePermission = false;
	}

	private IList<AIEmoteCmd> FilterEmoteByPreviousPlayed(IEnumerable<AIEmoteCmd> emoteCmds, bool isAlly)
	{
		IList<AIEmoteCmd> list = new List<AIEmoteCmd>();
		Queue<int> queue = (isAlly ? _previousEmoteQueue : _previousPlayerEmoteQueue);
		foreach (AIEmoteCmd emoteCmd in emoteCmds)
		{
			if (queue.Contains(emoteCmd.ID))
			{
				if (enemyAI.AIStableRandom() % 4 == 0)
				{
					list.Add(emoteCmd);
				}
			}
			else
			{
				list.Add(emoteCmd);
			}
		}
		return list;
	}
}
