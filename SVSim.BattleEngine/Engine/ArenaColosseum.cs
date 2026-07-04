using System.Collections.Generic;
using UnityEngine;
using Wizard;
using Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;
using Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

public class ArenaColosseum : ArenaEntryDataBase
{
	public enum eRound
	{
	}

	public enum eStageNo
	{
}

	public enum eEntryStatus
	{
	}

	public enum eRule
	{
		NONE = 0,
		RotationBo1 = 1,
		UnlimitedBo1 = 2,
		TwoPick = 3,
		TwoPickChaos = 4,
		Crossover = 5,
		MyRotation = 6,
		HOF = 31,
		WindFall = 33,
		Avatar = 39
	}

	public enum eDeckIndex
	{
		First = 0	}

	public struct Detail
	{
	}

	public class TwoPick
	{
		public CandidateClass CandidateClass { get; set; }

		public CandidateCardInfo CandidateCard { get; set; }

		public CandidateChaos CandidateChaos { get; set; }
	}

	public enum eResultEffect
	{
		None,
		GroupA,
		Final,
		Clear
	}

	private bool _isRankMatching;

	public eRule Rule { get; set; }

	public int ChaosNum { get; set; }

	public bool IsRankMatching
	{
		get
		{
			return _isRankMatching;
		}
		set
		{
			if (_isRankMatching != value)
			{
				_isRankMatching = value;
				if (RealTimeNetworkAgent.FinishTaskBase != null)
				{
					RealTimeNetworkAgent.FinishTaskBase = new ColosseumBattleFinishTask();
				}
			}
		}
	}

	public List<DeckData> DeckList { get; set; }

	public List<ReceivedReward> RewardList { get; set; }

	public eResultEffect ResultEffect { get; set; }

	public bool IsFreeEntry
	{
		get
		{
			return Data.MyPageNotifications.data.IsColosseumFreeEntry;
		}
		set
		{
			Data.MyPageNotifications.data.IsColosseumFreeEntry = value;
		}
	}

	public Detail[] DetailData { get; set; }

	public List<bool> BattleResultList { get; set; }

	public List<int> BoxGradeList { get; set; }

	public TwoPick TwoPickData { get; set; }

	public ArenaColosseum()
	{
		base.LootBoxType = PlayerStaticData.LootBoxType.COLOSSEUM;
		DeckList = new List<DeckData>();
		RewardList = new List<ReceivedReward>();
		BattleResultList = new List<bool>();
		BoxGradeList = new List<int>();
		DetailData = new Detail[5];
		Rule = eRule.TwoPick;
		TwoPickData = new TwoPick();
		TwoPickData.CandidateClass = new CandidateClass();
		TwoPickData.CandidateCard = new CandidateCardInfo();
		TwoPickData.CandidateChaos = new CandidateChaos();
	}
}
