using System.Collections.Generic;

namespace Wizard;

public class AISinglePlayptnRecord
{
	public List<int> PlayPtn;

	public ulong PlayPtnHash;

	public List<PlayedCardInfo> PlayedCardList;

	public bool IsValid;

	public int LastRestPp;

	public int TotalDrawCount;

	public float FirstPlayCardPriority;

	public AIVirtualCard FirstSummonedAllyFollower;

	public int ReferenceFieldIndex { get; private set; }

	public List<AIVirtualCard> UsableHandCardList { get; private set; }

	public List<AIVirtualCard> RestHandCardList { get; private set; }

	public List<AIVirtualCard> AllDiscardedCardList { get; private set; }

	public int PlayPtnCount
	{
		get
		{
			if (PlayPtn != null)
			{
				return PlayPtn.Count;
			}
			return 0;
		}
	}

	public AISinglePlayptnRecord(List<int> playPtn, AIVirtualField field, int fieldIndex)
	{
		PlayPtn = playPtn;
		PlayPtnHash = AIHandPtnCalculator.CalculatePlayPtnHash(field, playPtn);
		IsValid = true;
		LastRestPp = field.AllyPp;
		ReferenceFieldIndex = fieldIndex;
		TotalDrawCount = 0;
		InitializeHandCardList(field);
		AllDiscardedCardList = null;
		PlayedCardList = new List<PlayedCardInfo>();
		for (int i = 0; i < playPtn.Count; i++)
		{
			int num = playPtn[i];
			AIVirtualCard card = field.AllyHandCards[num];
			PlayedCardList.Add(new PlayedCardInfo(num, card));
			if (i == 0)
			{
				FirstPlayCardPriority = card.GetPriority(playPtn);
			}
		}
	}

	public void UpdatePlayPtnRecord(PlayedCardInfo info)
	{
		TotalDrawCount += info.DrawCount;
		LastRestPp = info.RestPp;
		if (!info.IsPlayable)
		{
			IsValid = false;
		}
		RestHandCardList.Remove(info.Card);
		info.IsProcessed = true;
	}

	public bool IsToBeRegister()
	{
		return PlayPtn.Count == PlayedCardList.Count;
	}

	public AIVirtualCard FindRealActor(AIVirtualCard originalCard)
	{
		List<PlayedCardInfo> playedCardList = PlayedCardList;
		for (int i = 0; i < playedCardList.Count; i++)
		{
			PlayedCardInfo playedCardInfo = playedCardList[i];
			if (originalCard.IsSameCard(playedCardInfo.Card) && playedCardInfo.TransformCard != null)
			{
				return playedCardInfo.TransformCard;
			}
		}
		return originalCard;
	}

	public bool IsMatchedPattern(List<int> playPtn)
	{
		for (int i = 0; i < PlayPtn.Count; i++)
		{
			int num = PlayPtn[i];
			int num2 = playPtn[i];
			if (num != num2)
			{
				return false;
			}
		}
		return true;
	}

	private void InitializeHandCardList(AIVirtualField field)
	{
		UsableHandCardList = new List<AIVirtualCard>();
		RestHandCardList = new List<AIVirtualCard>();
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			AIVirtualCard item = field.AllyHandCards[i];
			RestHandCardList.Add(item);
			if (!PlayPtn.Contains(i))
			{
				UsableHandCardList.Add(item);
			}
		}
	}

	public bool IsAllTargetsUsableHandCard(List<AIVirtualCard> targets)
	{
		if (UsableHandCardList == null)
		{
			return false;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard item = targets[i];
			if (!UsableHandCardList.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsUsableHandCard(AIVirtualCard card)
	{
		if (UsableHandCardList == null)
		{
			return false;
		}
		for (int i = 0; i < UsableHandCardList.Count; i++)
		{
			if (UsableHandCardList[i].IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public void CheckRegisteredDiscardInfo(AIDiscardInfo discardInfo)
	{
		if (discardInfo.IsNGByAI)
		{
			IsValid = false;
		}
		else if (discardInfo.IsSuccess)
		{
			List<AIVirtualCard> targetList = discardInfo.TargetList;
			UpdateHandCardList(targetList);
			AllDiscardedCardList = AIParamQuery.AddRangeToList(targetList, AllDiscardedCardList);
		}
	}

	public void UpdateHandCardList(List<AIVirtualCard> usedHandCardList)
	{
		if (usedHandCardList != null && usedHandCardList.Count > 0)
		{
			for (int i = 0; i < usedHandCardList.Count; i++)
			{
				AIVirtualCard item = usedHandCardList[i];
				UsableHandCardList.Remove(item);
				RestHandCardList.Remove(item);
			}
		}
	}

	public void RegisterPreprocess(PlayedCardInfo info, AIVirtualTargetSelectAction situation, AIVirtualField field)
	{
		AISimulationPreprocessRecorder preprocessRecorder = situation.PreprocessRecorder;
		info.RegisterPreprocess(preprocessRecorder);
		AIVirtualCard card = info.Card;
		if (preprocessRecorder.TotalBurialCount <= 0)
		{
			return;
		}
		AIVirtualTargetSelectInfo burialSelectInfo = card.GetBurialSelectInfo(field, situation);
		bool flag = false;
		bool isBreakPlayptn = false;
		if (burialSelectInfo != null)
		{
			AISelectedTargetInfo burialSelectTargets = burialSelectInfo.GetBurialSelectTargets(situation, field, burialSelectInfo, this, out isBreakPlayptn);
			if (burialSelectTargets != null)
			{
				info.RegisterPreDecidedPreprocessTarget(burialSelectTargets);
				UpdateHandCardList(burialSelectTargets.Targets);
				flag = true;
			}
		}
		if (flag)
		{
			IsValid = !isBreakPlayptn;
		}
		else
		{
			preprocessRecorder.ClearBurialCount();
		}
	}

	public PlayedCardInfo FindPlayedCardInfo(AIVirtualCard actor)
	{
		if (PlayedCardList == null || PlayedCardList.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < PlayedCardList.Count; i++)
		{
			PlayedCardInfo playedCardInfo = PlayedCardList[i];
			if (actor.IsSameCard(playedCardInfo.Card))
			{
				return playedCardInfo;
			}
		}
		return null;
	}
}
