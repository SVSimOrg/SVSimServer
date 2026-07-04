using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class SkillProcessor
{
	private struct InplayCardInfo
	{
		public int _index;

		public bool _isPlayer;
	}

	public enum ProcessCallType
	{
		Start,
		ResidentStop
	}

	public abstract class ProcessInfo
	{
		protected SkillProcessor _skillProcessor;

		protected BattlePlayerReadOnlyInfoPair _playerInfoPair;

		protected SkillConditionCheckerOption _checkerOption;

		public BattleCardBase OwnerCard { get; private set; }

		public bool NeedOwnerDeadCheck { get; set; }

		public bool IsImmediate { get; set; }

		public abstract bool IsContainDeckSkill { get; }

		public abstract List<SkillBase> GetDeckSkills { get; }

		public ProcessInfo(BattleCardBase ownerCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
		{
			OwnerCard = ownerCard;
			IsImmediate = false;
			_skillProcessor = skillProcessor;
			_playerInfoPair = playerInfoPair;
			_checkerOption = checkerOption;
		}

		public abstract VfxBase CallStart();

		public abstract List<SkillBase> GetActiveSkill();
	}

	public class ProcessInfoCollection : ProcessInfo
	{
		protected SkillCollectionBase _skillCollection;

		protected List<SkillBase> _activeSkill;

		public override bool IsContainDeckSkill => _activeSkill.Any((SkillBase s) => s.IsDeckSelfSkill);

		public override List<SkillBase> GetDeckSkills => _activeSkill.Where((SkillBase s) => s.IsDeckSelfSkill).ToList();

		public ProcessInfoCollection(BattleCardBase ownerCard, SkillCollectionBase skills, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, List<SkillBase> activeSkill)
			: base(ownerCard, skillProcessor, playerInfoPair, checkerOption)
		{
			_skillCollection = skills;
			_activeSkill = activeSkill;
		}

		public override VfxBase CallStart()
		{
			return _skillCollection.CallStart(_skillProcessor, _playerInfoPair, _checkerOption, _activeSkill);
		}

		public override List<SkillBase> GetActiveSkill()
		{
			return _activeSkill;
		}
	}

	public class ProcessInfoSkill : ProcessInfo
	{
		protected SkillBase _skill;

		public override bool IsContainDeckSkill => _skill.IsDeckSelfSkill;

		public override List<SkillBase> GetDeckSkills
		{
			get
			{
				List<SkillBase> list = new List<SkillBase>();
				if (_skill.IsDeckSelfSkill)
				{
					list.Add(_skill);
				}
				return list;
			}
		}

		public ProcessInfoSkill(BattleCardBase ownerCard, SkillBase skill, bool isPlayer, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
			: base(ownerCard, skillProcessor, playerInfoPair, checkerOption)
		{
			_skill = skill;
		}

		public override VfxBase CallStart()
		{
			SkillBase.CallParameter callParameter = new SkillBase.CallParameter();
			callParameter.skillProcessor = _skillProcessor;
			callParameter.calledSkillResultInfo = new SkillBase.SkillResultInfo();
			return _skill.CallStart(_skillProcessor, _playerInfoPair, _checkerOption, callParameter);
		}

		public override List<SkillBase> GetActiveSkill()
		{
			return new List<SkillBase> { _skill };
		}
	}

	public class StopProcessInfoResidentSkill : ProcessInfoSkill
	{
		public StopProcessInfoResidentSkill(BattleCardBase ownerCard, SkillBase skill, bool isPlayer, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
			: base(ownerCard, skill, isPlayer, skillProcessor, playerInfoPair, checkerOption)
		{
		}

		public override VfxBase CallStart()
		{
			SkillBase.CallParameter callParameter = new SkillBase.CallParameter();
			callParameter.skillProcessor = _skillProcessor;
			callParameter.calledSkillResultInfo = new SkillBase.SkillResultInfo();
			return _skill.CallStart(_skillProcessor, _playerInfoPair, _checkerOption, callParameter, ProcessCallType.ResidentStop);
		}
	}

	protected List<SkillBase> _processSkillList = new List<SkillBase>();

	private List<SkillBase> _inactiveSkillList = new List<SkillBase>();

	public Func<VfxBase> OnSkillProcedureEnd;

	private Queue<ProcessInfo> _collectionQueue = new Queue<ProcessInfo>();

	private bool m_needOwnerDeadCheck;

	public event Func<BattleCardBase, VfxBase> OnSkillStart;

	public void Register(ProcessInfo info, bool ignoreOwnerDeadCheck = false)
	{
		if (info != null)
		{
			int needOwnerDeadCheck2;
			if (!ignoreOwnerDeadCheck)
			{
				bool flag = (info.NeedOwnerDeadCheck = m_needOwnerDeadCheck);
				needOwnerDeadCheck2 = (flag ? 1 : 0);
			}
			else
			{
				needOwnerDeadCheck2 = 0;
			}
			info.NeedOwnerDeadCheck = (byte)needOwnerDeadCheck2 != 0;
			_collectionQueue.Enqueue(info);
		}
	}

	private InplayCardInfo[] CreateInplayCardInfo(IEnumerable<BattleCardBase> inplayCards)
	{
		int num = 0;
		InplayCardInfo[] array = new InplayCardInfo[inplayCards.Count()];
		foreach (BattleCardBase inplayCard in inplayCards)
		{
			array[num++] = new InplayCardInfo
			{
				_index = inplayCard.Index,
				_isPlayer = inplayCard.IsPlayer
			};
		}
		return array;
	}

	public void AddProcessSkilList(SkillBase skill)
	{
		_processSkillList.Add(skill);
	}

	public void ClearProcessSkillList()
	{
		_processSkillList.Clear();
	}

	public List<SkillBase> GetProcessSkillList()
	{
		return _processSkillList;
	}

	public void AddInactiveSkilList(List<SkillBase> skills)
	{
		_inactiveSkillList.AddRange(skills);
	}

	private void ClearInactiveSkillList()
	{
		_inactiveSkillList.Clear();
	}

	public List<SkillBase> GetDeckSkils()
	{
		List<SkillBase> list = new List<SkillBase>();
		for (int i = 0; i < _collectionQueue.Count; i++)
		{
			list.AddRange(_collectionQueue.ElementAt(i).GetDeckSkills);
		}
		return list;
	}

	private void SortDeckSkillProcess(List<ProcessInfo> tmpList, BattlePlayerPair battlePlayerPair, bool isNotCheckUlist)
	{
		int i = 0;
		for (int count = tmpList.Count; i < count; i++)
		{
			int num = 0;
			ProcessInfo processInfo = null;
			if (isNotCheckUlist)
			{
				num = battlePlayerPair.Self.BattleMgr.StableRandomOnlySelf(tmpList.Count());
				processInfo = tmpList[num];
			}
			else
			{
				NetworkBattleReceiver.ReceiveData receivedData = (battlePlayerPair.Self.BattleMgr as NetworkBattleManagerBase).networkBattleData.GetReceiveData();
				int k;
				for (k = 0; k < receivedData.unapprovedList.Count; k++)
				{
					ProcessInfo processInfo2 = tmpList.FirstOrDefault((ProcessInfo t) => t.OwnerCard.Index == receivedData.unapprovedList[k].Index);
					if (processInfo2 != null)
					{
						processInfo = processInfo2;
						break;
					}
				}
			}
			if (processInfo != null)
			{
				tmpList.Remove(processInfo);
				_collectionQueue.Enqueue(processInfo);
			}
		}
	}

	public VfxBase Process(BattlePlayerPair battlePlayerPair, bool isImmediate = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		InplayCardInfo[] clonedSelf = CreateInplayCardInfo(battlePlayerPair.Self.InPlayCards.Where((BattleCardBase s) => battlePlayerPair.Self.SummonedCards.Count == 0 || battlePlayerPair.Self.SummonedCards.Any((BattleCardBase a) => s != a)));
		InplayCardInfo[] clonedOp = CreateInplayCardInfo(battlePlayerPair.Opponent.InPlayCards.Where((BattleCardBase s) => battlePlayerPair.Opponent.SummonedCards.Count == 0 || battlePlayerPair.Opponent.SummonedCards.Any((BattleCardBase a) => s != a)));
		m_needOwnerDeadCheck = true;
		bool isNotCheckUlist = !battlePlayerPair.Self.BattleMgr.GameMgr.IsUseUnapprovedList(battlePlayerPair.Self.IsPlayer);
		if (_collectionQueue.Where((ProcessInfo p) => p.IsContainDeckSkill).ToList().Count() >= 2)
		{
			List<ProcessInfo> list = _collectionQueue.ToList();
			List<ProcessInfo> list2 = new List<ProcessInfo>();
			_collectionQueue.Clear();
			int num = 0;
			for (int count = list.Count; num < count; num++)
			{
				ProcessInfo processInfo = list[num];
				if (processInfo.IsContainDeckSkill)
				{
					list2.Add(processInfo);
					continue;
				}
				SortDeckSkillProcess(list2, battlePlayerPair, isNotCheckUlist);
				_collectionQueue.Enqueue(processInfo);
			}
			SortDeckSkillProcess(list2, battlePlayerPair, isNotCheckUlist);
		}
		for (int num2 = 0; num2 < _inactiveSkillList.Count; num2++)
		{
			_inactiveSkillList[num2].CallOnInactiveSkill();
		}
		while (_collectionQueue.Count > 0)
		{
			ProcessInfo info = _collectionQueue.Dequeue();
			VfxBase vfx = ProcessOneSkill(info, battlePlayerPair);
			sequentialVfxPlayer.Register(vfx);
		}
		PlayVoiceOnCharacterDeath(clonedSelf, clonedOp, battlePlayerPair);
		VfxBase allFuncVfxResults = OnSkillProcedureEnd.GetAllFuncVfxResults();
		sequentialVfxPlayer.Register(allFuncVfxResults);
		if (!isImmediate)
		{
			battlePlayerPair.Self.SkillsEndProcess();
			battlePlayerPair.Opponent.SkillsEndProcess();
			if (battlePlayerPair.Self.BattleView != null && !(battlePlayerPair.Self.BattleView is NullBattlePlayerView))
			{
				sequentialVfxPlayer.Register(battlePlayerPair.Self.BattleView.GetSideLogControl(isSkillTargetSelect: false).ClearLastShowLogCard());
				battlePlayerPair.Self.BattleMgr.OperateMgr.CallOnClearSideLog(battlePlayerPair.Self.IsPlayer);
			}
			ClearProcessSkillList();
			ClearInactiveSkillList();
		}
		return sequentialVfxPlayer;
	}

	private VfxBase ProcessOneSkill(ProcessInfo info, BattlePlayerPair battlePlayerPair)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool flag = true;
		if (info.NeedOwnerDeadCheck)
		{
			flag = !info.OwnerCard.IsDead;
		}
		if (flag)
		{
			battlePlayerPair.Self.BattleMgr.OperateMgr.CallOnSkillProcessStart();
			VfxBase vfxBase = info.CallStart();
			if (vfxBase.IsVfxNonEmpty())
			{
				VfxBase vfxCollection = this.OnSkillStart.GetAllFuncVfxResults(info.OwnerCard);
				InstantVfx vfx = InstantVfx.Create(delegate
				{
				});
				sequentialVfxPlayer.Register(vfx);
			}
			battlePlayerPair.Self.BattleMgr.OperateMgr.CallOnSkillProcessEnd();
			sequentialVfxPlayer.Register(vfxBase);
		}
		battlePlayerPair.Self.OnCallOneSkillProcess();
		battlePlayerPair.Opponent.OnCallOneSkillProcess();
		this.OnSkillStart = null;
		return sequentialVfxPlayer;
	}

	private List<BattleCardBase> GetDestroyedCard(InplayCardInfo[] inplayInfo, List<BattleCardBase> cemeteryList)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		BattleCardBase battleCardBase = null;
		int i = 0;
		for (int num = inplayInfo.Length; i < num; i++)
		{
			battleCardBase = GetCardInCemetery(ref inplayInfo[i], cemeteryList);
			if (battleCardBase != null)
			{
				list.Add(battleCardBase);
			}
		}
		return list;
	}

	private BattleCardBase GetCardInCemetery(ref InplayCardInfo inplayInfo, List<BattleCardBase> cemeteryList)
	{
		int i = 0;
		for (int count = cemeteryList.Count; i < count; i++)
		{
			if (cemeteryList[i].IsPlayer == inplayInfo._isPlayer && cemeteryList[i].Index == inplayInfo._index)
			{
				return cemeteryList[i];
			}
		}
		return null;
	}

	private void PlayVoiceOnCharacterDeath(InplayCardInfo[] clonedSelf, InplayCardInfo[] clonedOp, BattlePlayerPair actualPair)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(GetDestroyedCard(clonedSelf, actualPair.Self.CemeteryList));
		list.AddRange(GetDestroyedCard(clonedOp, actualPair.Opponent.CemeteryList));
		list.AddRange(GetDestroyedCard(clonedSelf, actualPair.Self.NecromanceZoneList));
		list.AddRange(GetDestroyedCard(clonedOp, actualPair.Opponent.NecromanceZoneList));
		if (!list.Any())
		{
			return;
		}
		BattleCardBase battleCardBase = SelectCardToHaveDestroyVoicePlay(list, actualPair.Self.BattleMgr.IsRecovery);
		if (battleCardBase != null)
		{
			if (!actualPair.Self.BattleMgr.IsRecovery || !(battleCardBase.BattleCardView is NullBattleCardView))
			{
				battleCardBase.BattleCardView.playVoiceOnDeath = true;
				battleCardBase.BattleCardView.VoiceInfo.SetDestroyCardId(-1);
			}
			battleCardBase.SelfBattlePlayer.CallOnPlayVoiceOnDeath(battleCardBase);
		}
	}

	private BattleCardBase SelectCardToHaveDestroyVoicePlay(List<BattleCardBase> destroyedCards, bool isRecovery)
	{
		destroyedCards.FisherYatesShuffle();
		for (int i = 0; i < destroyedCards.Count; i++)
		{
			BattleCardBase battleCardBase = destroyedCards[i];
			if (!isRecovery)
			{
				_ = battleCardBase.BattleCardView.VoiceInfo;
			}
			else
			{
				CardVoiceInfoCache.GetCardVoiceInfoForBattle(battleCardBase.CardId);
			}
			if (!string.IsNullOrEmpty(battleCardBase.BattleCardView.VoiceInfo.GetDestroyVoice(battleCardBase.IsEvolution, battleCardBase.IsExecutedEarthRite).Voice) || (isRecovery && !string.IsNullOrEmpty(CardVoiceInfoCache.GetCardVoiceInfoForBattle(battleCardBase.CardId).GetDestroyVoice(battleCardBase.IsEvolution, battleCardBase.IsExecutedEarthRite).Voice)))
			{
				return battleCardBase;
			}
		}
		return null;
	}
}
