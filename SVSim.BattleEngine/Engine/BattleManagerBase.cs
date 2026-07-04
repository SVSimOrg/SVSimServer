using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Mulligan;
using Wizard.Battle.Phase;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

public class BattleManagerBase
{
	public class CardParameterListInfo
	{
		public IList<GameObject> HolderCards { get; set; }

		public CardParameterListInfo()
		{
			HolderCards = new List<GameObject>();
		}

		public void Clear()
		{
			HolderCards.Clear();
		}
	}

	public class XorShift
	{
		private int w = 111111111;

		private int x = 123456789;

		private int y = 987654321;

		private int z = 555555555;

		public bool IsActive { get; private set; }

		public XorShift(int seed = -1)
		{
			IsActive = seed != -1;
			w = seed;
		}

		public int GetChangeInt(double val)
		{
			double num = Math.Floor((double)Next() / 2147483647.0 * 10000000000.0) / 10000000000.0;
			return (int)Math.Floor(val * num);
		}

		private int Next()
		{
			int num = x ^ (x << 11);
			x = y;
			y = z;
			z = w;
			return w = w ^ (w >> 19) ^ (num ^ (num >> 8));
		}
	}

	public class IndexInfo
	{
		public int AddIndex { get; private set; }

		public int TargetIndex { get; private set; }

		public bool IsSkillCopy { get; private set; }

		public int CopySkillSelectIndex { get; private set; }

		public IndexInfo(int addIndex = -1, int targetIndex = -1, bool skillCopy = false, int copySelectIndex = -1)
		{
			AddIndex = addIndex;
			TargetIndex = targetIndex;
			IsSkillCopy = skillCopy;
			CopySkillSelectIndex = copySelectIndex;
		}
	}

	public class MissionNecessaryInformation
	{
		private Dictionary<string, SkillOptionValue> NecessaryTargetDictionary;

		private Dictionary<string, string> _originalTargetDictionary;

		public MissionNecessaryInformation(Dictionary<string, string> targetDictionary)
		{
			_originalTargetDictionary = targetDictionary;
			NecessaryTargetDictionary = new Dictionary<string, SkillOptionValue>();
			foreach (KeyValuePair<string, string> item in targetDictionary)
			{
				SkillOptionValue value = new SkillOptionValue(SkillCreator.ParseContentInfos("mission_info=" + item.Value));
				NecessaryTargetDictionary.Add(item.Key, value);
			}
		}

		public Dictionary<string, string> GetOriginalTargetDictionary()
		{
			return _originalTargetDictionary;
		}
	}

	public class CalledCreateFilterPair
	{
		private readonly IReadOnlyBattleCardInfo _ownerCard;

		private readonly string _checkText;

		public CalledCreateFilterPair(IReadOnlyBattleCardInfo ownerCard, string checkText)
		{
			_ownerCard = ownerCard;
			_checkText = checkText;
		}

		public bool Equal(CalledCreateFilterPair piar)
		{
			if (piar._ownerCard == _ownerCard)
			{
				return piar._checkText == _checkText;
			}
			return false;
		}

		public bool HasOwnerCard()
		{
			if (_ownerCard != null)
			{
				return _ownerCard.SkillApplyInformation != null;
			}
			return false;
		}

		// Fusion-list parameter: headless has no fusion animation, so the outer mgr's list is
		// always empty and Contains(...) always returns false — the block below always runs.
		// Passed as a parameter (not read via BattleLogManager.GetInstance()) so concurrent
		// battles resolve against their own instance's list rather than a process singleton.
		public bool IsOwnerCardDead(List<BattleCardBase> enemyFusionCard)
		{
			if (!enemyFusionCard.Contains(_ownerCard))
			{
				if (!_ownerCard.IsDead)
				{
					if (!_ownerCard.IsInHand)
					{
						return !_ownerCard.IsInplay;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public class CardCreateInfo
	{
		public readonly bool IsPlayer;

		public readonly NetworkBattleDefine.NetworkCardPlaceState PlaceStatus;

		public readonly IndexInfo IndexInfo;

		public readonly bool IsChoice;

		public readonly bool IsReferenceOpponenCard;

		public int Id { get; private set; }

		public SkillBase Skill { get; private set; }

		public int Cost { get; private set; } = -1;

		public CardCreateInfo(int id, bool isPlayer, bool isChoice, NetworkBattleDefine.NetworkCardPlaceState placeStatus, bool isReferenceOpponentCard = false, SkillBase skill = null)
		{
			Id = id;
			IsPlayer = isPlayer;
			PlaceStatus = placeStatus;
			IsChoice = isChoice;
			IsReferenceOpponenCard = isReferenceOpponentCard;
			Skill = skill;
		}

		public void SetId(int id)
		{
			Id = id;
		}

		public void SetCost(int cost)
		{
			Cost = cost;
		}
	}

	public enum BATTLE_RESULT_TYPE
	{
		NONE,
		WIN,
		LOSE,
		CONSISTENCY
	}

	public enum FINISH_TYPE
	{
		NORMAL,
		RETIRE,
		SPECIAL_WIN
	}

	protected class AttachInfo
	{
		public BattleCardBase _classCard;

		public SkillBase _attachSkill;

		public SkillCreator.SkillBuildInfo _targetSkillBuildInfo;

		public string _myRotationBonusId;

		public AttachInfo(BattleCardBase classCard, SkillBase attachSkill, SkillCreator.SkillBuildInfo targetSkillBuildInfo, string myRotationBonusId = "")
		{
			_classCard = classCard;
			_attachSkill = attachSkill;
			_targetSkillBuildInfo = targetSkillBuildInfo;
			_myRotationBonusId = myRotationBonusId;
		}
	}

	public class ResourceInfo
	{
		public string ObjectPath { get; private set; }

		public string SePath { get; private set; }

		public string ObjectFullPath { get; private set; }

		public bool IsEffectBattleInfoDictionary { get; set; }

		public ResourceInfo(string objectPath, string sePath)
		{
			ObjectPath = objectPath;
			SePath = sePath;
			ObjectFullPath = Toolbox.ResourcesManager.GetAssetTypePath(objectPath, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true);
		}

		public ResourceInfo(string objectFullPath)
		{
			ObjectPath = string.Empty;
			SePath = string.Empty;
			ObjectFullPath = objectFullPath;
		}
	}

	private Dictionary<CalledCreateFilterPair, ISkillCardFilter> _calledCreateFilterDictionary = new Dictionary<CalledCreateFilterPair, ISkillCardFilter>();

	private Dictionary<CalledCreateFilterPair, SkillCardLimitUpperCountFilter> _calledCreateLimitFilterDictionary = new Dictionary<CalledCreateFilterPair, SkillCardLimitUpperCountFilter>();

	private Dictionary<CalledCreateFilterPair, SkillOrFilter> _calledCreateOrFilterDictionary = new Dictionary<CalledCreateFilterPair, SkillOrFilter>();

	public List<BattleCardBase> EnemyFusionCard = new List<BattleCardBase>();

	public static readonly int FIRST_PLAYER_EP_NUM = 2;

	public static readonly int SECOND_PLAYER_EP_NUM = 3;

	public SBattleLoad SBattleLoad;

	protected IBattleMgrContentsCreator _contentsCreator;

	public EmoteUI EmoteUI;

	public BattleEnemy BattleEnemy;

	public BattlePlayer BattlePlayer;

	public GameObject BtlContainer;

	public GameObject BtlUIContainer;

	public GameObject CutInContainer;

	public GameObject Battle3DContainer;

	public GameObject SubParticleContainer;

	protected BattleCamera _battleCamera;

	protected BackGroundBase _backGround;

	public DetailMgr DetailMgr;

	public PanelMgr PanelMgr;

	public IMulliganMgr MulliganMgr;

	protected Camera _subParticleCamera;

	public IEnemyAI EnemyAI;

	public GameObject CardHolder;

	public GameObject ECardHolder;

	public GameObject PCardPlace;

	public GameObject ChoiceCardHolder;

	public GameObject EvolveCardHolder;

	public GameObject Arrow;

	public ArrowControl ArrowControl;

	public GameObject AttackArrowHead;

	public GameObject EvolutionArrowHead;

	public GameObject AlertDialogue;

	public UILabel AlertDialogueLabel;

	public GameObject PSideLog;

	public SideLogControl PSideLogControl;

	public GameObject ESideLog;

	public SideLogControl ESideLogControl;

	public GameObject ESelectSkillSideLog;

	public SideLogControl ESelectSkillSideLogControl;

	public ITurnPanelControl TurnPanelControl;

	public GameObject BattleResult;

	public BattleResultUIController BattleResultControl;

	public GameObject BattleStart;

	public BattleStartControl BattleStartControl;

	public CardParameterListInfo PlayerCardParameterListInfo = new CardParameterListInfo();

	public CardParameterListInfo EnemyCardParameterListInfo = new CardParameterListInfo();

	public List<BattleCardBase> TransformCardList = new List<BattleCardBase>();

	public bool IsFirst;

	public Transform SubUI;

	public TweenAlpha SubUIOverLayBG;

	public GameObject MenuButtonObject;

	public int isStorySuccessful;

	protected int _allPublishedActiveSkillCount;

	protected int _temporaryPublishedAddCount;

	protected int _lethalPublishedActiveSkillCount = -1;

	protected int _lethalMovementCount;

	protected int _allPublishedDamageModifierCount;

	protected int _allPublishedHealModifierCount;

	public int FirstTurn;

	public int SecondTurn;

	private GameObject _unityEventAgentObject;

	private UnityEventAgent _unityEventAgent;

	protected IPhase _phase;

	// Instance-backed IsRandomDraw / IsForecast / RecoveryInfo (Phase 5a, 2026-07-02).
	// Static accessors (this.InstanceIsForecast, Wizard.Data.BattleRecoveryInfo, etc.)
	// route through the ambient's current Mgr — same lookup shape as GetIns() — but the
	// authoritative state lives on the mgr instance itself, not in separate ambient slots.
	// Defaults mirror the ambient's historical defaults.
	public bool InstanceIsRandomDraw { get; set; } = true;
	public bool InstanceIsForecast { get; set; } = true;
	public Wizard.BattleRecoveryInfo InstanceRecoveryInfo { get; set; }

	// Instance-backed ViewerId. Default 1001 matches EngineGlobalInit.ThisViewerId (the historical
	// PlayerStaticData / Certification default when neither has been assigned by a session).
	public int InstanceViewerId { get; set; } = 1001;

	// Instance-backed RealTimeNetworkAgent. Owner: mgr; nullable — headless mostly runs without an
	// agent, and existing readers guard with `?.`.
	public RealTimeNetworkAgent InstanceNetworkAgent { get; set; }

	// Phase-5 chunk 42 (2026-07-03): ambient bridge dropped. All engine per-mgr readers and writers
	// now use `mgr.InstanceIsRandomDraw` / `mgr.InstanceIsForecast` directly. The residual static
	// setter/getter keeps the shape only for pre-mgr fixture/node writes — no ambient-slot fallback.
	// If GetIns() is null (no scope), the setter silently no-ops and the getter returns the default.
	public static bool IsRandomDraw {
		get => GetIns()?.InstanceIsRandomDraw ?? true;
		set { if (GetIns() is { } m) m.InstanceIsRandomDraw = value; }
	}

	public static bool IsForecast {
		get => GetIns()?.InstanceIsForecast ?? true;
		set { if (GetIns() is { } m) m.InstanceIsForecast = value; }
	}

	// Phase-5 chunk 45 (2026-07-03): pure per-instance GameMgr assigned in ctor. The property has
	// no field initializer so the ctor can set it before the base-ctor chain reads it. Subclasses
	// that need a pre-seeded GameMgr (test fixtures with chara-id / net-user data) call the
	// (contentsCreator, gameMgr) overload; the parameterless overload keeps the historical ambient
	// bridge so pre-existing TestBattleScope seeders still work while the fixture rewrite lands.
	public GameMgr GameMgr { get; }

	public BattleLifeTimeSharedObject BattleLifeTimeSharedObject;

	private BattleUIContainer _battleUIContainer;

	protected System.Random _stableRandom;

	protected System.Random _stableRandomOnlySelf;

	private int stableRandomCount;

	protected XorShift _selfXorShiftRandom;

	protected XorShift _oppXorShiftRandom;

	public static bool IsTutorial
	{
		get
		{
			if (Data.Load.data._userTutorial.tutorial_step == 100)
			{
				return false;
			}
			return true;
		}
	}

	protected virtual bool DisableCustomMouse => false;

	public static bool UseCustomMouse
	{
		get
		{
			bool flag = GetIns()?.DisableCustomMouse ?? false;
			if (InputMgr.MouseControl)
			{
				return !flag;
			}
			return false;
		}
	}

	public int BackgroundId { get; private set; }

	public string BgmId { get; private set; }

	public BattleCamera Camera => _battleCamera;

	public BackGroundBase BackGround => _backGround;

	public int AllPublishedActiveSkillCount
	{
		get
		{
			return _allPublishedActiveSkillCount;
		}
		set
		{
			_allPublishedActiveSkillCount = value;
		}
	}

	public int TemporaryPublishedAddCount
	{
		get
		{
			return _temporaryPublishedAddCount;
		}
		protected set
		{
			_temporaryPublishedAddCount = value;
		}
	}

	public List<SkillBase> PublishedSkillList { get; protected set; }

	public int LethalPublishedActiveSkillCount
	{
		get
		{
			return _lethalPublishedActiveSkillCount;
		}
		set
		{
			_lethalPublishedActiveSkillCount = value;
		}
	}

	public int LethalMovementCount
	{
		get
		{
			return _lethalMovementCount;
		}
		set
		{
			_lethalMovementCount = value;
		}
	}

	public int AllPublishedDamageModifierCount
	{
		get
		{
			return _allPublishedDamageModifierCount;
		}
		set
		{
			_allPublishedDamageModifierCount = value;
		}
	}

	public int AllPublishedHealModifierCount
	{
		get
		{
			return _allPublishedHealModifierCount;
		}
		set
		{
			_allPublishedHealModifierCount = value;
		}
	}

	public int NextIndividualId { get; private set; } = 1;

	public int CurrentTurn
	{
		get
		{
			if (!BattlePlayer.IsSelfTurn)
			{
				return BattleEnemy.Turn;
			}
			return BattlePlayer.Turn;
		}
	}

	public virtual TouchControl TouchControl { get; protected set; }

	public virtual OperateMgr OperateMgr { get; protected set; }

	public IPhaseCreator PhaseCreator { get; private set; }

	public bool IsRecovery { get; set; }

	public bool IsPuzzleMgr => this is PuzzleBattleManager;

	public virtual bool IsBattleEnd
	{
		get
		{
			if (BattlePlayer != null && BattleEnemy != null && !BattlePlayer.Class.IsDead)
			{
				return BattleEnemy.Class.IsDead;
			}
			return true;
		}
	}

	public IBattleResourceMgr BattleResourceMgr { get; private set; }

	public VfxMgr VfxMgr { get; protected set; }

	public virtual bool IsStopOperate => false;

	public BattleUIContainer BattleUIContainer
	{
		get
		{
			return _battleUIContainer;
		}
		set
		{
			_battleUIContainer = value;
			if (Prediction == null || !(_battleUIContainer != null))
			{
				return;
			}
			_battleUIContainer.ShowPrediction = delegate(bool isPress)
			{
				if (isPress)
				{
					Prediction.TurnEnd();
				}
				else
				{
					Prediction.Clear();
				}
			};
		}
	}

	public bool HasFocus => _unityEventAgent.HasFocus;

	public double randomResult { get; protected set; }

	public bool IsMulliganEnd { get; set; }

	public bool IsTurnEnd { get; protected set; }

	public virtual bool IsVirtualBattle => this.InstanceIsForecast;

	public virtual bool IsVirtualBattleEnemyTurn
	{
		get
		{
			if (EnemyAI is EnemyAI)
			{
				return BattleEnemy.IsSelfTurn;
			}
			return false;
		}
	}

	public bool IsPlayerRetire { get; protected set; }

	public Prediction Prediction { get; private set; }

	public event Action<bool> OnStartOpening;

	public event Action OnWin;

	public event Action OnBattleSettingInfoClear;

	public event Action<bool> OnBattleFinish;

	public event Func<VfxBase> OnSubmitMulligan;

	public void AddPublishedSkillList(SkillBase skill)
	{
		PublishedSkillList.Add(skill);
	}

	public void IncrementIndividualId()
	{
		NextIndividualId++;
	}

	public virtual int GetMaxDeckCount(bool isSelf)
	{
		return 40;
	}

	public XorShift XorShiftRandom(bool isSelf)
	{
		if (!isSelf)
		{
			return _oppXorShiftRandom;
		}
		return _selfXorShiftRandom;
	}

	public static BattleManagerBase GetIns()
	{
		// Phase-5 chunk 46: pure per-instance world. GetIns() no longer routes through the ambient
		// (which is being deleted). All direct engine callers were converted to per-mgr reads in
		// chunks 38-42; residual callers are the 3 façades (Certification.ViewerId, Data.BattleRecoveryInfo,
		// ToolboxGame.RealTimeNetworkAgent) and the two static flags (IsForecast, IsRandomDraw),
		// each of which returns a null-tolerant default when GetIns() is null — matching the "no scope"
		// semantics the ambient used to provide.
		return null;
	}

	protected BattleManagerBase(IBattleMgrContentsCreator contentsCreator)
		: this(contentsCreator, new GameMgr())
	{
	}

	protected BattleManagerBase(IBattleMgrContentsCreator contentsCreator, GameMgr gameMgr)
	{
		GameMgr = gameMgr;
		BattleLifeTimeSharedObject = new BattleLifeTimeSharedObject();
		PublishedSkillList = new List<SkillBase>();
		_contentsCreator = contentsCreator;
		BattleResourceMgr = _contentsCreator.CreateResourceMgr();
		_stableRandom = new System.Random(_contentsCreator.RandomSeed);
		_stableRandomOnlySelf = new System.Random(_contentsCreator.RandomSeed);
		BackgroundId = CreateBackgroundId();
		BgmId = CreateBgmId();
		CreateManager();
		BattlePlayer = CreateBattlePlayer();
		BattleEnemy = CreateBattleEnemy();
		BattlePlayer.ClassAndInPlayCardList[0].Setup();
		BattleEnemy.ClassAndInPlayCardList[0].Setup();
		PhaseCreator = _contentsCreator.CreatePhaseCreator(this);
		PhaseCreator.CreateFirstPhase().Setup();
		EmoteUI = null;
		PanelMgr = null;
		SBattleLoad = null;
		IsFirst = false;
		BattlePlayer.EvolveWaitTurnCount = 0;
		BattleEnemy.EvolveWaitTurnCount = 0;
		FirstTurn = (SecondTurn = 0);
		EnemyAI = null;
		BtlContainer = null;
		BtlUIContainer = null;
		CutInContainer = null;
		Battle3DContainer = null;
		SubParticleContainer = null;
		CardHolder = null;
		PCardPlace = null;
		ECardHolder = null;
		Arrow = null;
		ArrowControl = null;
		AttackArrowHead = null;
		EvolutionArrowHead = null;
		AlertDialogue = null;
		TurnPanelControl = null;
		BattleResult = null;
		BattleResultControl = null;
		BattleStart = null;
		BattleStartControl = null;
		SubUI = null;
		SubUIOverLayBG = null;
		MenuButtonObject = null;
		this.GameMgr.GetPrefabMgr().Load("Prefab/Game/UnityEventAgent");
		_unityEventAgentObject = UnityEngine.Object.Instantiate(this.GameMgr.GetPrefabMgr().Get("Prefab/Game/UnityEventAgent"));
		_unityEventAgent = _unityEventAgentObject.GetComponent<UnityEventAgent>();
		_unityEventAgent.SetBattleMgr(this);
		Prediction = new Prediction(BattleResourceMgr, GetBattlePlayerPair(isPlayer: true));
		TouchControl = new TouchControl(this, _battleCamera, _backGround);
		OperateMgr = CreateOperateMgr();
		VfxMgr = _contentsCreator.CreateVfxMgr();
		VfxBase vfx = ChangePhase(PhaseCreator.CreateFirstPhase());
		VfxMgr.RegisterSequentialVfx(vfx);
		SetupEvent();
		FirstRecoverySetting();
		FirstReplaySetting();
		OnBattleSettingInfoClear += delegate
		{
			this.GameMgr.GetDataMgr().SetStoryBgmID("NONE");
		};
		LocalLog.AccumulateSettingLog();
	}

	protected virtual void FirstRecoverySetting()
	{
		StartRecoveryRecording();
	}

	public void StartRecoveryRecording()
	{
		_contentsCreator.RecoveryRecordManager.SetupRecording(this, this.GameMgr.GetDataMgr().m_BattleType, _contentsCreator.RandomSeed, BackgroundId, BgmId);
	}

	protected virtual void FirstReplaySetting()
	{
		StartReplayRecording();
	}

	public void StartReplayRecording()
	{
		_contentsCreator.ReplayRecordManager.SetupRecording(this);
	}

	public void CreateXorShift(int selfIdxSeed, int oppIdxSeed = -1)
	{
		if (selfIdxSeed != -1)
		{
			_selfXorShiftRandom = new XorShift(selfIdxSeed);
		}
		if (oppIdxSeed != -1)
		{
			_oppXorShiftRandom = new XorShift(oppIdxSeed);
		}
	}

	public void SetBattleMenuBtn()
	{
		MenuButtonObject = BtlUIContainer.transform.Find("BattleMenuBtn").gameObject;
		SetBattleMenuBtnVisibility();
	}

	public virtual void SetBattleMenuBtnVisibility()
	{
		MenuButtonObject.SetActive(value: false);
	}

	protected virtual int CreateBackgroundId()
	{
		int backGroundId = _contentsCreator.RecoveryManager.BackGroundId;
		if (backGroundId >= 0)
		{
			return backGroundId;
		}
		int result = 1;
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		if (dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest && PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SIMPLE_STAGE))
		{
			result = 9;
		}
		else if (dataMgr.m_BattleType == DataMgr.BattleType.Story || dataMgr.IsQuestBattleType() || this.GameMgr.IsPuzzleQuest)
		{
			result = dataMgr.GetSoroPlay3DFieldID();
		}
		else if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SIMPLE_STAGE))
		{
			result = 9;
		}
		else if (dataMgr.m_BattleType == DataMgr.BattleType.Practice && dataMgr.GetSoroPlay3DFieldID() != 0)
		{
			result = CalculationRandomStage();
		}
		else if (!IsTutorial)
		{
			result = UnityEngine.Random.Range(1, 8);
		}
		return result;
	}

	protected int CalculationRandomStage()
	{
		if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SIMPLE_STAGE))
		{
			return 9;
		}
		List<int> list = new List<int>();
		if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.USE_OFF_STAGE))
		{
			list = PlayerPrefsWrapper.CreateServerSendStageOffList();
		}
		List<int> list2 = new List<int>();
		for (int i = 0; i < Data.Load.data.OpenBattleFieldIdList.Count; i++)
		{
			int item = int.Parse(Data.Load.data.OpenBattleFieldIdList[i]);
			if (!list.Contains(item))
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			return 9;
		}
		return list2[UnityEngine.Random.Range(0, list2.Count)];
	}

	protected virtual string CreateBgmId()
	{
		string bgmId = _contentsCreator.RecoveryManager.BgmId;
		if (bgmId != "NONE")
		{
			return bgmId;
		}
		string result = "NONE";
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		if (dataMgr.m_BattleType == DataMgr.BattleType.Story || dataMgr.IsQuestBattleType())
		{
			result = dataMgr.GetStoryBgmID();
		}
		return result;
	}

	protected void CreateManager()
	{
		_battleCamera = new BattleCamera();
		this.GameMgr.GetInputMgr().SetBattleCamera(_battleCamera);
		DetailMgr = new DetailMgr();
		switch (BackgroundId)
		{
		case 1:
			_backGround = new ForestField(BgmId);
			break;
		case 2:
			_backGround = new CastleField(BgmId);
			break;
		case 3:
			_backGround = new VolcanoField(BgmId);
			break;
		case 4:
			_backGround = new RoyalPalaceField(BgmId);
			break;
		case 5:
			_backGround = new TempleField(BgmId);
			break;
		case 6:
			_backGround = new ChateauField(BgmId);
			break;
		case 7:
			_backGround = new LaboratoryField(BgmId);
			break;
		case 8:
			_backGround = new GateField(BgmId);
			break;
		case 9:
			_backGround = new ArenaField(BgmId);
			break;
		case 10:
			_backGround = new PlazField(BgmId);
			break;
		case 11:
			_backGround = new ForestNightField(BgmId);
			break;
		case 14:
			_backGround = new RoyalPalaceNightField(BgmId);
			break;
		case 15:
			_backGround = new TempleNightField(BgmId);
			break;
		case 17:
			_backGround = new LaboratoryNightField(BgmId);
			break;
		case 18:
			_backGround = new YuwanField(BgmId);
			break;
		case 20:
			_backGround = new PlazRiotingField(BgmId);
			break;
		case 21:
			_backGround = new HillField(BgmId);
			break;
		case 22:
			_backGround = new AlleyField(BgmId);
			break;
		case 23:
			_backGround = new HillRiotingField(BgmId);
			break;
		case 30:
			_backGround = new IronField(BgmId);
			break;
		case 31:
			_backGround = new NateField(BgmId);
			break;
		case 32:
			_backGround = new Nat2Field(BgmId);
			break;
		case 33:
			_backGround = new Nat3Field(BgmId);
			break;
		case 34:
			_backGround = new Nat4Field(BgmId);
			break;
		case 41:
			_backGround = new RivayleField(BgmId);
			break;
		case 42:
			_backGround = new RivayleBackalleyField(BgmId);
			break;
		case 43:
			_backGround = new VellsarDesertField(BgmId);
			break;
		case 51:
			_backGround = new Field51(BgmId);
			break;
		case 52:
			_backGround = new Field52(BgmId);
			break;
		case 61:
			_backGround = new Field61(BgmId);
			break;
		case 62:
			_backGround = new Field62(BgmId);
			break;
		case 71:
			_backGround = new Field71(BgmId);
			break;
		case 72:
			_backGround = new Field72(BgmId);
			break;
		case 73:
			_backGround = new Field73(BgmId);
			break;
		case 74:
			_backGround = new Field74(BgmId);
			break;
		case 75:
			_backGround = new Field75(BgmId);
			break;
		case 76:
			_backGround = new Field76(BgmId);
			break;
		case 1001:
			_backGround = new SpecialArenaField(BgmId);
			break;
		case 1002:
			_backGround = new llField();
			break;
		case 1003:
			_backGround = new PriConnField(BgmId);
			break;
		case 1004:
			_backGround = new StageField(BgmId);
			break;
		case 1005:
			_backGround = new Field1005(BgmId);
			break;
		case 1006:
			_backGround = new Field1006(BgmId);
			break;
		case 1007:
			_backGround = new Field1007(BgmId);
			break;
		case 1008:
			_backGround = new Field1008(BgmId);
			break;
		case 1009:
			_backGround = new Field1009(BgmId);
			break;
		case 1010:
			_backGround = new Field1010(BgmId);
			break;
		case 1011:
			_backGround = new Field1011(BgmId);
			break;
		case 1012:
			_backGround = new Field1012(BgmId);
			break;
		}
	}

	protected virtual OperateMgr CreateOperateMgr()
	{
		return new OperateMgr(this, TouchControl);
	}

	protected virtual BattlePlayer CreateBattlePlayer()
	{
		return new BattlePlayer(this, _battleCamera, _backGround, CreatePlayerInnerOptionsBuilder());
	}

	protected virtual BattleEnemy CreateBattleEnemy()
	{
		return new BattleEnemy(this, _battleCamera, _backGround, CreateEnemyInnerOptionsBuilder());
	}

	public virtual IInnerOptionsBuilder CreatePlayerInnerOptionsBuilder()
	{
		return new PlayerInnerOptionsBuilder(BattleResourceMgr);
	}

	public virtual IInnerOptionsBuilder CreateEnemyInnerOptionsBuilder()
	{
		return NullInnerOptionsBuilder.GetInstance();
	}

	public virtual void StartOpening(int FirstAttack)
	{
		LocalLog.AccumulateLastTraceLog("StartOpening");
		FirstAttack = GetFirstAttack(FirstAttack);
		bool doesPlayerGoFirst = FirstAttack == 0;
		SetupInitialGameState(doesPlayerGoFirst, areCardsRandomlyDrawn: true, 20, 20);
		VfxBase vfx = ChangePhase(PhaseCreator.CreateOpeningPhase());
		SkillProcessor skillProcessor = new SkillProcessor();
		VfxBase vfx2 = BattlePlayer.StartSkillWhenBattleStart(skillProcessor);
		VfxMgr.RegisterSequentialVfx(vfx);
		VfxMgr.RegisterSequentialVfx(vfx2);
		this.OnStartOpening.Call(IsFirst);
	}

	public virtual void SetupInitialGameState(bool doesPlayerGoFirst, bool areCardsRandomlyDrawn, int playerMaxLife, int enemyMaxLife)
	{
		IsFirst = doesPlayerGoFirst;
		IsRandomDraw = areCardsRandomlyDrawn;
		InitializeClassLife(playerMaxLife, enemyMaxLife);
		SetUpMyRotationBattle(playerMaxLife, enemyMaxLife);
		SetupAvatarBattle(doesPlayerGoFirst);
		TurnPanelControl.Initialize();
		SetupEvolCount(doesPlayerGoFirst);
	}

	public void SetupEvolCount(bool doesPlayerGoFirst)
	{
		BattlePlayerBase battlePlayerBase;
		BattlePlayerBase battlePlayerBase2;
		if (doesPlayerGoFirst)
		{
			battlePlayerBase = BattlePlayer;
			battlePlayerBase2 = BattleEnemy;
		}
		else
		{
			battlePlayerBase = BattleEnemy;
			battlePlayerBase2 = BattlePlayer;
		}
		SetPlayerInitialEp(battlePlayerBase, FIRST_PLAYER_EP_NUM, FIRST_PLAYER_EP_NUM, 5);
		SetPlayerInitialEp(battlePlayerBase2, SECOND_PLAYER_EP_NUM, SECOND_PLAYER_EP_NUM, 4);
	}

	public void SetPlayerInitialEp(BattlePlayerBase battlePlayerBase, int usableEp, int maxEp, int turnsLeftUntilCanEvolve)
	{
		battlePlayerBase.SetCurrentEpCount(usableEp);
		battlePlayerBase.EpTotal = maxEp;
		battlePlayerBase.EvolveWaitTurnCount = turnsLeftUntilCanEvolve;
	}

	private void InitializeClassLife(int playerMaxLife, int enemyMaxLife)
	{
		((ClassBattleCardBase)BattlePlayer.Class).InitBaseMaxLife(playerMaxLife);
		((ClassBattleCardBase)BattleEnemy.Class).InitBaseMaxLife(enemyMaxLife);
	}

	protected virtual void SetupEvent()
	{
		BattlePlayer.OnTurnEndFinish += delegate
		{
			BattleResourceMgr.UnloadEffectBattle();
			BattlePlayer.PlayerBattleView.ResetTouchable();
			return NullVfx.GetInstance();
		};
		BattleEnemy.OnTurnEndFinish += delegate
		{
			BattleResourceMgr.UnloadEffectBattle();
			return NullVfx.GetInstance();
		};
	}

	public VfxBase ControlTurnStartPlayer()
	{
		return ControlTurnStart(BattleEnemy, BattlePlayer, IsFirst);
	}

	public VfxBase ControlTurnStartOpponent()
	{
		return ControlTurnStart(BattlePlayer, BattleEnemy, !IsFirst);
	}

	private VfxBase ControlTurnStart(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer, bool didPlayerGoFirst)
	{
		if (IsBattleEnd)
		{
			return NullVfx.GetInstance();
		}
		int num = 0;
		VfxBase result;
		if (selfBattlePlayer.IsExtraTurn)
		{
			result = selfBattlePlayer.StartTurnControl("ExtraTurn");
			selfBattlePlayer.DecreasesExtraTurnCount();
			num = selfBattlePlayer.Turn;
		}
		else
		{
			result = opponentBattlePlayer.StartTurnControl("Normal");
			opponentBattlePlayer.DecreasesExtraTurnCount();
			num = opponentBattlePlayer.Turn;
		}
		if (num >= 1)
		{
			LocalLog.SetLastTraceLogTurn(num);
		}
		return result;
	}

	public virtual void SetUpOperateEvent(OperateMgr operateMgr)
	{
		SetupEndTurnButtonEvents(operateMgr);
	}

	public virtual void SetupBattlePlayersEvent()
	{
		BattlePlayer.OnSetupCardEvent += SetupCardEvent;
		BattleEnemy.OnSetupCardEvent += SetupCardEvent;
		BattlePlayer.OnSetupClassEvent += SetupPlayerClassEvent;
		BattleEnemy.OnSetupClassEvent += SetupOpponentClassEvent;
		BattlePlayer.Setup(BattleEnemy);
		BattleEnemy.Setup(BattlePlayer);
		BattlePlayer.OnTurnEnd += delegate
		{
			VfxMgr.Cancel();
			return NullVfx.GetInstance();
		};
	}

	protected virtual void DelayLoadCompleteOpponentResources()
	{
		SetupBattlePlayersEvent();
	}

	public virtual void SetupActionProcessorEvent(ActionProcessor processor, bool isPlayer)
	{
		GetBattlePlayer(isPlayer).SetupActionProcessorEvent(processor);
		processor.OnAfterPlayCard += delegate
		{
			BattlePlayer.UpdateHandCardsPlayability();
			BattleEnemy.UpdateHandCardsPlayability();
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(BattlePlayer.UpdateInPlayBattleCardIconLabel());
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
		processor.OnBeforeAttack += JudgeBattleResult;
		processor.OnAfterAttack += delegate
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(BattlePlayer.UpdateInPlayBattleCardIconLabel());
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
		processor.OnAfterEvolution += delegate
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(BattlePlayer.UpdateInPlayBattleCardIconLabel());
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
		processor.OnAfterFusion += delegate
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(BattlePlayer.UpdateInPlayBattleCardIconLabel());
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
	}

	protected virtual void SetupEndTurnButtonEvents(OperateMgr operateMgr)
	{
		SetupInstantEndTurnConditions();
		BattlePlayer.OnTurnStartAfterDraw += () => InstantVfx.Create(delegate
		{
			BattlePlayer.PlayerBattleView.UpdateTurnEndPulseEffect();
		});
		operateMgr.OnPlayerAttack += delegate
		{
			BattlePlayer.PlayerBattleView.UpdateTurnEndPulseEffect();
			BattlePlayer.UpdateHandCardsPlayability();
			return NullVfx.GetInstance();
		};
		operateMgr.OnPlayerEvolve += delegate
		{
			BattlePlayer.PlayerBattleView.UpdateTurnEndPulseEffect();
			return NullVfx.GetInstance();
		};
		operateMgr.OnPlayerFusion += delegate
		{
			BattlePlayer.PlayerBattleView.UpdateTurnEndPulseEffect();
			return NullVfx.GetInstance();
		};
		BattlePlayer.OnTurnEnd += delegate
		{
			BattlePlayer.PlayerBattleView.HideTurnEndPulseEffect();
			Prediction.Clear();
			return NullVfx.GetInstance();
		};
	}

	protected virtual void SetupInstantEndTurnConditions()
	{
		BattlePlayer.PlayerBattleView.OnCheckImmediateTurnEnd += delegate
		{
			if (BattlePlayer.CheckPlayableCards())
			{
				return false;
			}
			return !BattlePlayer.CheckAttackableCards();
		};
	}

	public virtual VfxBase LoadTurnPanelResource()
	{
		return TurnPanelControl.LoadResource();
	}

	public virtual void Update(float dt)
	{
		VfxWith<IPhase> vfxWith = _phase.Update(dt);
		VfxMgr.RegisterSequentialVfx(SequentialVfxPlayer.Create(vfxWith.Vfx, ChangePhase(vfxWith.Value)));
		VfxMgr.Update(Time.deltaTime);
		if (_backGround != null)
		{
			_backGround.UpdateFieldRandom();
		}
		if (BattlePlayer.BattleView.PlayQueueView != null)
		{
			BattlePlayer.BattleView.PlayQueueView.UpdatePlayQueuePositions(Time.deltaTime);
		}
		if (BattleEnemy.BattleView.PlayQueueView != null)
		{
			BattleEnemy.BattleView.PlayQueueView.UpdatePlayQueuePositions(Time.deltaTime);
		}
		LocalLog.SubmitAccumulateLastTraceLog();
	}

	public IPhase GetCurrentPhase()
	{
		return _phase;
	}

	public virtual void FinishBattle()
	{
	}

	public virtual void DisposeBattleGameObj()
	{
		BattleResourceMgr.Dispose();
		VfxMgr.Dispose();
		DetailMgr.Dispose();
		this.InstanceRecoveryInfo = null;
		BattleLifeTimeSharedObject = null;
		this.OnBattleSettingInfoClear.Call();
		BattleLogItem.ClearHeaderTextureCache();
		CardVoiceInfoCache.ClearCardVoiceInfo();
		NullBattleCardView.ReleaseSharedDummy();
		NullBattleCard.ReleaseSharedDummy();
		this.GameMgr.GetEffectMgr().ClearBattleFeildEffect();
		this.GameMgr.GetEffectMgr().RestUnneededEffect();
		_backGround.Dispose();
		_battleCamera.Dispose();
		Toolbox.ResourcesManager.RemoveAssetGroup(Toolbox.ResourcesManager.BattleListAssetPathList);
		Toolbox.ResourcesManager.BattleListAssetPathList.Clear();
		RenderSettings.fog = false;
		this.GameMgr.GetEffectMgr().ImmediateDestroyBattleEffectContainer();
		if (PanelMgr != null)
		{
			DisposeBattleGameObj_DestroyImmediate(PanelMgr.gameObject);
		}
		DisposeBattleGameObj_DestroyImmediate(Battle3DContainer);
		DisposeBattleGameObj_DestroyImmediate(BtlUIContainer);
		DisposeBattleGameObj_DestroyImmediate(CutInContainer);
		DisposeBattleGameObj_DestroyImmediate(SubParticleContainer);
		DisposeBattleGameObj_DestroyImmediate(_unityEventAgentObject);
		if (SubUI != null)
		{
			DisposeBattleGameObj_DestroyImmediate(SubUI.gameObject);
		}
		SBattleLoad.Dispoose();
		SBattleLoad = null;
		BattlePlayer.Clear();
		BattleEnemy.Clear();
		BattlePlayer = null;
		BattleEnemy = null;
		EmoteUI = null;
		_unityEventAgentObject = null;
		PanelMgr = null;
		PlayerCardParameterListInfo.Clear();
		EnemyCardParameterListInfo.Clear();
		PlayerCardParameterListInfo = null;
		EnemyCardParameterListInfo = null;
		TransformCardList.Clear();
		PublishedSkillList.Clear();
		Prediction.Dispose();
		Prediction = null;
		TouchControl.Dispose();
		TouchControl = null;
		EnemyAI = null;
		BtlContainer = null;
		BtlUIContainer = null;
		CutInContainer = null;
		CardHolder = null;
		PCardPlace = null;
		ECardHolder = null;
		Arrow = null;
		ArrowControl = null;
		AttackArrowHead = null;
		EvolutionArrowHead = null;
		AlertDialogue = null;
		TurnPanelControl = null;
		BattleResult = null;
		BattleResultControl = null;
		BattleStart = null;
		BattleStartControl = null;
		MenuButtonObject = null;
		Battle3DContainer = null;
		SubUI = null;
		SubParticleContainer = null;
		UIManager.GetInstance().DestroyView(UIManager.ViewScene.Battle);
		this.GameMgr.GetPrefabMgr().DisposeAllClonedObject();
		this.GameMgr.GetGameObjMgr().DisposeUIGameObj();
		this.GameMgr.GetPrefabMgr().AllUnLoad();
	}

	private void DisposeBattleGameObj_DestroyImmediate(GameObject obj)
	{
		if (obj != null)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public VfxBase TurnEnd(bool isPlayer)
	{
		IsTurnEnd = true;
		VfxBase result = GetBattlePlayer(isPlayer).TurnEnd();
		IsTurnEnd = false;
		return result;
	}

	public virtual VfxBase StartBattle()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		LocalLog.SetLastTraceLogTurn(1);
		sequentialVfxPlayer.Register(ChangePhase(PhaseCreator.CreateMainPhase()));
		if (IsRecovery)
		{
			return sequentialVfxPlayer;
		}
		if (IsFirst)
		{
			sequentialVfxPlayer.Register(BattlePlayer.StartTurnControl());
		}
		else
		{
			sequentialVfxPlayer.Register(BattleEnemy.StartTurnControl());
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase ChangePhase(IPhase phase)
	{
		if (phase == null)
		{
			return NullVfx.GetInstance();
		}
		LocalLog.AccumulateLastTraceLog("ChangePhase" + phase.ToString());
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (_phase != null)
		{
			VfxBase vfx = _phase.Teardown();
			sequentialVfxPlayer.Register(vfx);
		}
		_phase = phase;
		VfxBase vfx2 = _phase.Setup();
		sequentialVfxPlayer.Register(vfx2);
		return sequentialVfxPlayer;
	}

	public BattlePlayerBase GetBattlePlayer(bool isPlayer)
	{
		if (isPlayer)
		{
			return BattlePlayer;
		}
		return BattleEnemy;
	}

	public BattlePlayerPair GetBattlePlayerPair(bool isPlayer)
	{
		BattlePlayerBase battlePlayer = GetBattlePlayer(isPlayer);
		BattlePlayerBase battlePlayer2 = GetBattlePlayer(!isPlayer);
		return new BattlePlayerPair(battlePlayer, battlePlayer2);
	}

	public BattlePlayerReadOnlyInfoPair GetBattlePlayerInfoPair(bool isPlayer)
	{
		BattlePlayerBase battlePlayer = GetBattlePlayer(isPlayer);
		BattlePlayerBase battlePlayer2 = GetBattlePlayer(!isPlayer);
		return new BattlePlayerReadOnlyInfoPair(battlePlayer, battlePlayer2);
	}

	public virtual int StableRandom(int val)
	{
		if (this.InstanceIsForecast)
		{
			return 0;
		}
		stableRandomCount++;
		randomResult = _stableRandom.NextDouble();
		return (int)Math.Floor((double)val * randomResult);
	}

	public virtual double StableRandomDouble()
	{
		if (this.InstanceIsForecast)
		{
			return 0.0;
		}
		stableRandomCount++;
		randomResult = _stableRandom.NextDouble();
		return randomResult;
	}

	public virtual int StableRandomOnlySelf(int val)
	{
		if (this.InstanceIsForecast)
		{
			return 0;
		}
		return _stableRandomOnlySelf.Next(val);
	}

	public BattleCardBase GetBattleCardIdx(IList<BattleCardBase> list, int idx)
	{
		return list.SingleOrDefault((BattleCardBase c) => c.Index == idx);
	}

	public virtual BattleCardBase MetamorphoseCard(int cardId, bool isPlayer, int addIndex, SkillBase skill, bool isFusion = false)
	{
		return CreateBattleCardWithGameObject(new CardCreateInfo(cardId, isPlayer, skill.ApplyingTargetFilter is SkillTargetChosenCardsFilter, NetworkBattleDefine.NetworkCardPlaceState.None, isReferenceOpponentCard: false, skill), new IndexInfo(addIndex));
	}

	public virtual BattleCardBase CreateBattleCardWithGameObject(CardCreateInfo info, IndexInfo indexInfo, int repeatCount = -1, bool isVirtual = false, bool isActualCard = false)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(info.Id);
		BattlePlayerBase battlePlayer = GetBattlePlayer(info.IsPlayer);
		int cardIndex = SetupCardIndex(battlePlayer, indexInfo.AddIndex);
		GameObject cardGameObject = null;
		if (!IsRecovery || !isVirtual)
		{
			cardGameObject = CreateBaseCardGameObject(cardParameterFromId, info.IsPlayer, cardIndex);
		}
		BattleCardBase battleCardBase = CreateBattleCard(info.Id, info.IsPlayer, cardGameObject, cardParameterFromId, battlePlayer, cardIndex, info.Cost);
		SetupCardObjectMaterials(cardGameObject, battleCardBase);
		return battleCardBase;
	}

	public BattleCardBase CreateBattleCard(int cardId, bool isPlayer, GameObject cardGameObject, CardParameter cardParameter, BattlePlayerBase battlePlayer, int cardIndex, int cost = -1)
	{
		BattleCardBase battleCardBase = CardCreatorBase.CreateToken(CreateCardBuildInfo(cardGameObject, cardParameter, isPlayer, cardIndex, cardId), cardGameObject == null);
		battleCardBase.IsTokenLoad = true;
		battlePlayer.SetupCardEvent(battleCardBase);
		if (cost != -1)
		{
			battleCardBase.CostModifierList.Add(new CostSetModifier(cost));
		}
		return battleCardBase;
	}

	public BattleCardBase CreateTransformCardRegisterVfx(BattleCardBase originalCard, int tokenId, bool isPlayer, VfxMgr predictionVfxMgr = null, bool isRecoveryFinish = false, bool isChoice = false)
	{
		BattleCardBase battleCardBase = null;
		if (!IsRecovery || isRecoveryFinish)
		{
			battleCardBase = CreateTransformCardWithGameObject(tokenId, originalCard, isPlayer, isChoice);
		}
		else
		{
			battleCardBase = TransformCardList.FirstOrDefault((BattleCardBase c) => c.CardId == tokenId && c.IsPlayer == isPlayer);
			if (battleCardBase == null)
			{
				CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(tokenId);
				battleCardBase = CreateChoiceCard(tokenId, isPlayer, null, cardParameterFromId, GetBattlePlayer(isPlayer));
			}
		}
		battleCardBase.TransformInfo = new BattleCardBase.TransformInformation(battleCardBase.TransformInfo.Type, originalCard);
		return battleCardBase;
	}

	protected virtual BattleCardBase CreateTransformCardWithGameObject(int cardId, BattleCardBase originalCard, bool isPlayer, bool isChoice)
	{
		BattleCardBase battleCardBase = TransformCardList.FirstOrDefault((BattleCardBase c) => c.CardId == cardId && c.IsPlayer == isPlayer && (isChoice || c.TransformInfo.OriginalCard == originalCard));
		if (battleCardBase == null)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
			BattlePlayerBase battlePlayer = GetBattlePlayer(isPlayer);
			GameObject cardGameObject = CreateBaseCardGameObject(cardParameterFromId, isPlayer, cardId);
			battleCardBase = CreateChoiceCard(cardId, isPlayer, cardGameObject, cardParameterFromId, battlePlayer);
			SetupCardObjectMaterials(cardGameObject, battleCardBase);
			TransformCardList.Add(battleCardBase);
		}
		return battleCardBase;
	}

	protected BattleCardBase CreateChoiceCard(int cardId, bool isPlayer, GameObject cardGameObject, CardParameter cardParameter, BattlePlayerBase battlePlayer)
	{
		BattleCardBase battleCardBase = CardCreatorBase.CreateToken(CreateCardBuildInfo(cardGameObject, cardParameter, isPlayer, cardId, cardId), cardGameObject == null);
		battleCardBase.IsTokenLoad = true;
		return battleCardBase;
	}

	public BattleCardBase ReplaceChoiceBraveCard(BattleCardBase originalCard, int cardId, BattleCardBase selectSkillCard)
	{
		BattleCardBase choiceBraveCard = originalCard.SelfBattlePlayer.CreateCard(cardId, originalCard.SelfBattlePlayer.Class.Index, isChoiceBrave: true);
		choiceBraveCard.BattleCardView.GameObject.SetActive(value: false);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(ParallelVfxPlayer.Create(choiceBraveCard.SelfBattlePlayer.BattleMgr.LoadCardResources(new List<BattleCardBase> { choiceBraveCard }), InstantVfx.Create(delegate
		{
			if (!IsRecovery)
			{
				Transform transform = choiceBraveCard.BattleCardView.Transform;
				Transform transform2 = ((selectSkillCard != null) ? selectSkillCard.BattleCardView.Transform : originalCard.SelfBattlePlayer.BattleView.ChoiceBraveButtonTransform);
				transform.position = ((selectSkillCard != null) ? transform2.position : new Vector3(transform2.position.x, transform2.position.y, 0f));
				transform.rotation = transform2.rotation;
				transform.parent = originalCard.SelfBattlePlayer.BattleView.HandDeck.transform;
				transform.localScale = transform2.localScale;
				transform.SetSiblingIndex(transform2.GetSiblingIndex());
				choiceBraveCard.BattleCardView.GameObject.SetActive(value: true);
			}
		})));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			if (selectSkillCard != null)
			{
				selectSkillCard.BattleCardView.GameObject.SetActive(value: false);
			}
		}));
		choiceBraveCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(sequentialVfxPlayer);
		return choiceBraveCard;
	}

	protected GameObject CreateBaseCardGameObject(CardParameter cardParameter, bool isPlayer, int cardIndex)
	{
		GameObject gameObject = null;
		if (cardParameter.CharType == CardBasePrm.CharaType.NORMAL)
		{
			gameObject = this.GameMgr.GetPrefabMgr().CloneObjectToParent(SBattleLoad.UnitCardTemplate.gameObject, _backGround.m_Battle3DContainer);
		}
		else if (cardParameter.CharType == CardBasePrm.CharaType.SPELL)
		{
			gameObject = this.GameMgr.GetPrefabMgr().CloneObjectToParent(SBattleLoad.SpellCardTemplate.gameObject, _backGround.m_Battle3DContainer);
		}
		else if (cardParameter.CharType == CardBasePrm.CharaType.FIELD || cardParameter.CharType == CardBasePrm.CharaType.CHANT_FIELD)
		{
			gameObject = this.GameMgr.GetPrefabMgr().CloneObjectToParent(SBattleLoad.FieldCardTemplate.gameObject, _backGround.m_Battle3DContainer);
			gameObject.transform.Find("CardObj/NormalField").gameObject.SetActive(value: false);
		}
		SetupCardObjectTags(gameObject, isPlayer, cardIndex);
		return gameObject;
	}

	private void SetupCardObjectTags(GameObject cardGameObject, bool isPlayer, int cardIndex)
	{
		string tag;
		string text;
		if (isPlayer)
		{
			tag = "PlayerToken";
			text = "P";
		}
		else
		{
			tag = "Enemy";
			text = "E";
		}
		cardGameObject.tag = tag;
		cardGameObject.transform.Find("CardObj").tag = tag;
		cardGameObject.transform.Find("Collider").tag = tag;
		cardGameObject.name = text + cardIndex;
	}

	protected void SetupCardObjectMaterials(GameObject cardGameObject, BattleCardBase battleCard)
	{
		cardGameObject.GetComponent<CardTemplate>().DynamicSetupMaterials(battleCard, BattleResourceMgr);
		cardGameObject.SetActive(value: false);
		cardGameObject.transform.localPosition = Vector3.zero;
		cardGameObject.transform.localScale = Global.CARD_BATTLE_SCALE;
		cardGameObject.transform.rotation = Quaternion.identity;
	}

	protected int SetupCardIndex(BattlePlayerBase battlePlayer, int cardIndex)
	{
		if (cardIndex == -1)
		{
			cardIndex = battlePlayer.cardTotalNum;
			battlePlayer.cardTotalNum++;
		}
		return cardIndex;
	}

	private BattleCardBase.BuildInfo CreateCardBuildInfo(GameObject cardGameObject, CardParameter cardParameter, bool isPlayer, int cardIndex, int cardId)
	{
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(cardParameter);
		IInnerOptionsBuilder innerOptionsBuilder = (isPlayer ? CreatePlayerInnerOptionsBuilder() : CreateEnemyInnerOptionsBuilder());
		bool isPlayer2 = isPlayer;
		return new BattleCardBase.BuildInfo(cardGameObject, cardId, GetBattlePlayer(isPlayer), GetBattlePlayer(!isPlayer), GetBattlePlayer(isPlayer), cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, isPlayer2, cardIndex, innerOptionsBuilder.CreateCardOptions(), this, BattleResourceMgr);
	}

	public virtual VfxBase JudgeBattleResult()
	{
		if (BattlePlayer.Class.IsDead && BattleEnemy.Class.IsDead)
		{
			return InstantVfx.Create(delegate
			{
				InitiateGameEndSequence(!BattlePlayer.IsSelfTurn);
			});
		}
		if (BattlePlayer.Class.IsDead)
		{
			return InstantVfx.Create(delegate
			{
				InitiateGameEndSequence(hasWon: false);
			});
		}
		if (BattleEnemy.Class.IsDead)
		{
			return InstantVfx.Create(delegate
			{
				InitiateGameEndSequence(hasWon: true);
			});
		}
		return NullVfx.GetInstance();
	}

	public VfxBase DeadClass(bool PlayerDead, FINISH_TYPE finishType)
	{
		ClassBattleCardBase classBattleCardBase = (ClassBattleCardBase)GetBattlePlayer(PlayerDead).Class;
		if (this.GameMgr.IsReplayBattle && !classBattleCardBase.ClassBattleCardView.InPlayModelActive)
		{
			return NullVfx.GetInstance();
		}
		switch ((int)finishType)
		{
		case 0:
			return classBattleCardBase.SelfBattlePlayer.CardManagement(classBattleCardBase, new SkillProcessor(), BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false);
		case 1:
			if (IsBothClassDead(PlayerDead))
			{
				return NullVfx.GetInstance();
			}
			classBattleCardBase.FlagCardAsDestroyedBySkill();
			return classBattleCardBase.Retire();
		case 2:
			if (IsBothClassDead(PlayerDead))
			{
				return NullVfx.GetInstance();
			}
			return classBattleCardBase.DestroyBySpecialWin();
		default:
			return classBattleCardBase.SelfBattlePlayer.CardManagement(classBattleCardBase, new SkillProcessor(), BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false);
		}
	}

	private bool IsBothClassDead(bool isPlayer)
	{
		ClassBattleCardBase obj = (ClassBattleCardBase)GetBattlePlayer(isPlayer).Class;
		ClassBattleCardBase classBattleCardBase = (ClassBattleCardBase)GetBattlePlayer(!isPlayer).Class;
		if (obj.IsDead && classBattleCardBase.IsDead)
		{
			return true;
		}
		return false;
	}

	protected virtual int GetFirstAttack(int FirstAttack)
	{
		return UnityEngine.Random.Range(0, 2);
	}

	public virtual void SetupEnemyAI()
	{
		EnemyAI = new EnemyAINull();
	}

	public virtual void SetupPlayerClassEvent()
	{
	}

	public virtual void SetupOpponentClassEvent()
	{
	}

	public virtual void SetupCardEvent(BattleCardBase card)
	{
	}

	public virtual void InitiateGameEndSequence(bool hasWon)
	{
		if (this.GameMgr.IsReplayBattle && !this.GameMgr.IsNewReplayBattle)
		{
			hasWon = Data.ReplayBattleInfo.is_win;
		}
		IResultPhase resultPhase = PhaseCreator.CreateResultPhase(hasWon);
		if (hasWon)
		{
			resultPhase.OnSetupEnd += this.OnWin;
		}
		VfxBase vfx = ChangePhase(resultPhase);
		VfxMgr.RegisterSequentialVfx(vfx);
		this.OnBattleFinish.Call(hasWon);
	}

	public virtual VfxBase PlaySpecialWin(BattlePlayerBase winPlayer)
	{
		GetIns().VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
		bool playerDead = !winPlayer.IsPlayer;
		return SequentialVfxPlayer.Create(DeadClass(playerDead, FINISH_TYPE.SPECIAL_WIN), InstantVfx.Create(delegate
		{
			InitiateGameEndSequence(winPlayer.IsPlayer);
		}));
	}

	public virtual void PlayRetire()
	{
		GetIns().VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
		if (!GetBattlePlayer(isPlayer: true).Class.IsDead)
		{
			BattlePlayer.BattleView.HideTurnEndButton();
			IsPlayerRetire = true;
			VfxMgr.RegisterSequentialVfx(DeadClass(PlayerDead: true, FINISH_TYPE.RETIRE));
			VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
			{
				InitiateGameEndSequence(hasWon: false);
			}));
		}
	}

	public void ChangeCameraFieldOfView(float aspectRatio)
	{
		if ((bool)Battle3DContainer)
		{
			int num = CalculateCameraFieldOfView(aspectRatio);
			int num2 = CalculateBackgroundCameraFieldOfView(aspectRatio);
			Transform transform = Battle3DContainer.transform.Find("Camera");
			Camera component;
			Camera camera = (component = transform.GetComponent<Camera>());
			component.fieldOfView = num;
			component = transform.Find("Camera 3DGround").GetComponent<Camera>();
			component.fieldOfView = num2;
			(_subParticleCamera = transform.Find("Camera SubParticles").GetComponent<Camera>()).fieldOfView = num;
			component = transform.Find("Camera BattleUnder").GetComponent<Camera>();
			component.fieldOfView = num;
			Transform transform2 = Battle3DContainer.transform.Find("Camera HighRankEvolve");
			if (transform2 == null)
			{
				transform2 = transform.transform.Find("Camera HighRankEvolve");
			}
			Camera component2 = transform2.GetComponent<Camera>();
			component2.fieldOfView = num;
			DataMgr dataMgr = this.GameMgr.GetDataMgr();
			if (dataMgr.Is3DSkin(isPlayer: true))
			{
				component2.depth = 40f;
			}
			if (dataMgr.GetPlayerCharaData().IsNoEvolveShift)
			{
				component2.transform.SetParent(camera.transform);
			}
			component = CutInContainer.transform.Find("Camera").GetComponent<Camera>();
			component.fieldOfView = num;
		}
	}

	private int CalculateCameraFieldOfView(float aspectRatio)
	{
		if (!(aspectRatio < 1.5f))
		{
			_ = Global.NormalFieldOfView;
		}
		else
		{
			_ = Global.WideFieldOfView;
		}
		float num = aspectRatio - 1.5f;
		return Mathf.Clamp((int)((float)(Global.WideFieldOfView + Global.NormalFieldOfView) / 2f * (1f - num)), Global.NormalFieldOfView, Global.WideFieldOfView);
	}

	private int CalculateBackgroundCameraFieldOfView(float aspectRatio)
	{
		return CalculateCameraFieldOfView(aspectRatio);
	}

	public bool CanOpenEvolutionConfirmation(BattleCardBase card)
	{
		if (IsStopOperate || !card.IsInplay || !card.SelfBattlePlayer.IsSelfTurn || !card.IsPlayer || !card.IsUnit || this.GameMgr.IsWatchBattle)
		{
			return false;
		}
		return card.CanEvolution(isSkill: false, isSelfBattlePlayer: true);
	}

	protected virtual SequentialVfxPlayer OnShortageDeck(BattlePlayerBase battlePlayer)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (battlePlayer.IsShortageDeckWin)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			sequentialVfxPlayer.Register(DeadClass(!battlePlayer.IsPlayer, FINISH_TYPE.SPECIAL_WIN));
			battlePlayer.Class.OpponentBattlePlayer.Class.FlagCardAsDestroyedBySkill();
		}
		else
		{
			if (battlePlayer.IsPlayer)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
				BattlePlayer.SetIsShortageDeckLose(flag: true);
			}
			else
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
				BattleEnemy.SetIsShortageDeckLose(flag: true);
			}
			sequentialVfxPlayer.Register(DeadClass(battlePlayer.IsPlayer, FINISH_TYPE.NORMAL));
		}
		return sequentialVfxPlayer;
	}

	public SkillCardLimitUpperCountFilter CheackCalledCreateFilterDictionary(IReadOnlyBattleCardInfo ownerCard, string partText, string infoText)
	{
		SkillCardLimitUpperCountFilter result = null;
		CalledCreateFilterPair calledFilterPair = new CalledCreateFilterPair(ownerCard, partText);
		if (_calledCreateLimitFilterDictionary.Any((KeyValuePair<CalledCreateFilterPair, SkillCardLimitUpperCountFilter> c) => c.Key.Equal(calledFilterPair)))
		{
			KeyValuePair<CalledCreateFilterPair, SkillCardLimitUpperCountFilter> keyValuePair = _calledCreateLimitFilterDictionary.FirstOrDefault((KeyValuePair<CalledCreateFilterPair, SkillCardLimitUpperCountFilter> c) => c.Key.Equal(calledFilterPair));
			if (keyValuePair.Value != null)
			{
				result = keyValuePair.Value;
			}
		}
		else if (SkillFilterCreator.COUNT_EXTENSIONS_FILTER_NAMES.Any((string n) => Regex.IsMatch(partText, "^" + n + "[<>!:=]=?")))
		{
			SkillCardLimitUpperCountFilter skillCardLimitUpperCountFilter = new SkillCardLimitUpperCountFilter(infoText);
			result = skillCardLimitUpperCountFilter;
			if (!IsVirtualBattleEnemyTurn)
			{
				_calledCreateLimitFilterDictionary.Add(calledFilterPair, skillCardLimitUpperCountFilter);
			}
		}
		else if (!IsVirtualBattleEnemyTurn)
		{
			_calledCreateLimitFilterDictionary.Add(calledFilterPair, null);
		}
		return result;
	}

	public SkillOrFilter CheackCalledCreateOrFilterDictionary(IReadOnlyBattleCardInfo ownerCard, string partText, string infoText)
	{
		SkillOrFilter result = null;
		CalledCreateFilterPair calledFilterPair = new CalledCreateFilterPair(ownerCard, partText);
		if (_calledCreateOrFilterDictionary.Any((KeyValuePair<CalledCreateFilterPair, SkillOrFilter> c) => c.Key.Equal(calledFilterPair)))
		{
			KeyValuePair<CalledCreateFilterPair, SkillOrFilter> keyValuePair = _calledCreateOrFilterDictionary.FirstOrDefault((KeyValuePair<CalledCreateFilterPair, SkillOrFilter> c) => c.Key.Equal(calledFilterPair));
			if (keyValuePair.Value != null)
			{
				result = keyValuePair.Value;
			}
		}
		else
		{
			string text = SkillFilterCreator.ContentKeyword.or.ToStringCustom();
			if (Regex.IsMatch(partText, "^" + text + "[<>!:=]=?"))
			{
				SkillOrFilter skillOrFilter = new SkillOrFilter(int.Parse(infoText));
				result = skillOrFilter;
				if (!IsVirtualBattleEnemyTurn)
				{
					_calledCreateOrFilterDictionary.Add(calledFilterPair, skillOrFilter);
				}
			}
			else if (!IsVirtualBattleEnemyTurn)
			{
				_calledCreateOrFilterDictionary.Add(calledFilterPair, null);
			}
		}
		return result;
	}

	public ISkillCardFilter CheackCalledCreateSkillCardFilterDictionary(IReadOnlyBattleCardInfo ownerCard, string partText, string infoText)
	{
		ISkillCardFilter result = null;
		CalledCreateFilterPair calledFilterPair = new CalledCreateFilterPair(ownerCard, partText);
		if (_calledCreateFilterDictionary.Any((KeyValuePair<CalledCreateFilterPair, ISkillCardFilter> c) => c.Key.Equal(calledFilterPair)))
		{
			KeyValuePair<CalledCreateFilterPair, ISkillCardFilter> keyValuePair = _calledCreateFilterDictionary.FirstOrDefault((KeyValuePair<CalledCreateFilterPair, ISkillCardFilter> c) => c.Key.Equal(calledFilterPair));
			if (keyValuePair.Value != null)
			{
				result = keyValuePair.Value;
			}
		}
		else if (SkillFilterCreator.CARD_PARAMETER_COMPARE_FILTER_NAMES.Any((string n) => Regex.IsMatch(partText, "^" + n + "[<>!:=]=?")))
		{
			ISkillCardFilter skillCardFilter = SkillFilterCreator.CreateCardParameterCompareFilter(partText, ownerCard);
			result = skillCardFilter;
			if (!IsVirtualBattleEnemyTurn)
			{
				_calledCreateFilterDictionary.Add(calledFilterPair, skillCardFilter);
			}
		}
		else if (!IsVirtualBattleEnemyTurn)
		{
			_calledCreateFilterDictionary.Add(calledFilterPair, null);
		}
		return result;
	}

	public void RemoveUnUseCalledFilterDictionary()
	{
		if (IsVirtualBattleEnemyTurn)
		{
			return;
		}
		List<CalledCreateFilterPair> list = new List<CalledCreateFilterPair>();
		foreach (KeyValuePair<CalledCreateFilterPair, ISkillCardFilter> item in new Dictionary<CalledCreateFilterPair, ISkillCardFilter>(_calledCreateFilterDictionary))
		{
			if (item.Key.HasOwnerCard())
			{
				if (item.Key.IsOwnerCardDead(EnemyFusionCard))
				{
					list.Add(item.Key);
				}
			}
			else
			{
				list.Add(item.Key);
			}
		}
		foreach (CalledCreateFilterPair item2 in list)
		{
			_calledCreateFilterDictionary.Remove(item2);
		}
		list.Clear();
		foreach (KeyValuePair<CalledCreateFilterPair, SkillCardLimitUpperCountFilter> item3 in new Dictionary<CalledCreateFilterPair, SkillCardLimitUpperCountFilter>(_calledCreateLimitFilterDictionary))
		{
			if (item3.Key.HasOwnerCard())
			{
				if (item3.Key.IsOwnerCardDead(EnemyFusionCard))
				{
					list.Add(item3.Key);
				}
			}
			else
			{
				list.Add(item3.Key);
			}
		}
		foreach (CalledCreateFilterPair item4 in list)
		{
			_calledCreateLimitFilterDictionary.Remove(item4);
		}
		list.Clear();
		foreach (KeyValuePair<CalledCreateFilterPair, SkillOrFilter> item5 in new Dictionary<CalledCreateFilterPair, SkillOrFilter>(_calledCreateOrFilterDictionary))
		{
			if (item5.Key.HasOwnerCard())
			{
				if (item5.Key.IsOwnerCardDead(EnemyFusionCard))
				{
					list.Add(item5.Key);
				}
			}
			else
			{
				list.Add(item5.Key);
			}
		}
		foreach (CalledCreateFilterPair item6 in list)
		{
			_calledCreateOrFilterDictionary.Remove(item6);
		}
	}

	protected void SetUpMyRotationBattle(int playerMaxLife, int enemyMaxLife)
	{
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		MyRotationInfo myRotationInfo;
		bool flag = dataMgr.TryGetPlayerMyRotationInfo(out myRotationInfo);
		MyRotationInfo myRotationInfo2;
		bool flag2 = dataMgr.TryGetEnemyMyRotationInfo(out myRotationInfo2);
		if (!flag && !flag2)
		{
			return;
		}
		List<AttachInfo> list = new List<AttachInfo>();
		if (flag)
		{
			for (int i = 0; i < myRotationInfo.Abilities.Count; i++)
			{
				MyRotationInfo.MyRotationBonus myRotationBonus = myRotationInfo.Abilities[i];
				for (int j = 0; j < myRotationBonus.AttachAbilities.Length; j++)
				{
					AttachInfo attachInfo = AddAttachSkillToClass(isPlayer: true, myRotationBonus.AttachAbilities[j], myRotationBonus.AbilityId);
					if (attachInfo != null)
					{
						list.Add(attachInfo);
					}
				}
				BattlePlayer.PpTotal += myRotationBonus.AddStartPp;
				playerMaxLife += myRotationBonus.AddStartLife;
				BattlePlayer.BonusConditionList.Add(new BattlePlayerBase.MyRotationBonusCondition(myRotationBonus));
			}
		}
		if (flag2)
		{
			for (int k = 0; k < myRotationInfo2.Abilities.Count; k++)
			{
				MyRotationInfo.MyRotationBonus myRotationBonus2 = myRotationInfo2.Abilities[k];
				for (int l = 0; l < myRotationBonus2.AttachAbilities.Length; l++)
				{
					AttachInfo attachInfo2 = AddAttachSkillToClass(isPlayer: false, myRotationBonus2.AttachAbilities[l], myRotationBonus2.AbilityId);
					if (attachInfo2 != null)
					{
						list.Add(attachInfo2);
					}
				}
				BattleEnemy.PpTotal += myRotationBonus2.AddStartPp;
				enemyMaxLife += myRotationBonus2.AddStartLife;
				BattleEnemy.BonusConditionList.Add(new BattlePlayerBase.MyRotationBonusCondition(myRotationBonus2));
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			AttachInfo attachInfo3 = list[m];
			SkillBase attachSkill = Skill_attach_skill.CreateAndAttachSkill(attachInfo3._classCard, attachInfo3._attachSkill, attachInfo3._targetSkillBuildInfo);
			IDetailPanelControl detailPanel = DetailMgr.DetailPanelControl;
			if (!attachSkill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessRemoveAfterAction))
			{
				continue;
			}
			attachSkill.OnSkillStart += delegate
			{
				attachSkill.OnSkillEnd += delegate
				{
					if (attachSkill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessRemoveAfterAction && (s as SkillPreprocessRemoveAfterAction).IsEnd()))
					{
						List<BattlePlayerBase.MyRotationBonusCondition> list2 = (attachInfo3._classCard.IsPlayer ? BattlePlayer.BonusConditionList : BattleEnemy.BonusConditionList);
						BattlePlayerBase.MyRotationBonusCondition myRotationBonusCondition = list2.FirstOrDefault((BattlePlayerBase.MyRotationBonusCondition b) => b.MyRotationBonus.AbilityId == attachInfo3._myRotationBonusId);
						myRotationBonusCondition.ReduceSkillCount();
						myRotationBonusCondition.UseUpSkill();
						if (detailPanel._card != null && detailPanel._card.IsClass && detailPanel._card.IsPlayer == attachInfo3._classCard.IsPlayer)
						{
							detailPanel.UpdateBuffInfo(attachInfo3._classCard, list2);
						}
					}
					return NullVfx.GetInstance();
				};
			};
		}
		BattlePlayer.SetPpTotal(BattlePlayer.PpTotal, isUpdatePp: true, null);
		BattleEnemy.SetPpTotal(BattleEnemy.PpTotal, isUpdatePp: true, null);
		InitializeClassLife(playerMaxLife, enemyMaxLife);
	}

	protected void SetupAvatarBattle(bool doesPlayerGoFirst)
	{
		if (Data.CurrentFormat == Format.Avatar)
		{
			this.GameMgr.GetDataMgr();
			if (doesPlayerGoFirst)
			{
				SetupSpecifiedPlayerAvatarBattle(isPlayer: true);
				SetupSpecifiedPlayerAvatarBattle(isPlayer: false);
			}
			else
			{
				SetupSpecifiedPlayerAvatarBattle(isPlayer: false);
				SetupSpecifiedPlayerAvatarBattle(isPlayer: true);
			}
		}
	}

	private void SetupSpecifiedPlayerAvatarBattle(bool isPlayer)
	{
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		if ((!isPlayer) ? dataMgr.TryGetEnemyAvatarBattleInfo(out var avatarBattleInfo) : dataMgr.TryGetPlayerAvatarBattleInfo(out avatarBattleInfo))
		{
			SetupPlayerAvatarBattle(isPlayer ? ((BattlePlayerBase)BattlePlayer) : ((BattlePlayerBase)BattleEnemy), avatarBattleInfo);
		}
	}

	private void SetupPlayerAvatarBattle(BattlePlayerBase battlePlayerBase, AvatarBattleInfo avatarBattleInfo)
	{
		battlePlayerBase.AvatarBattleInfo = avatarBattleInfo;
		AvatarBattleInfo.AvatarBattleBonus bonus = avatarBattleInfo.Bonus;
		string[] abilities = bonus.Abilities;
		List<AttachInfo> list = new List<AttachInfo>();
		for (int i = 0; i < abilities.Length; i++)
		{
			AttachInfo attachInfo = AddAttachSkillToClass(battlePlayerBase is BattlePlayer, abilities[i]);
			if (attachInfo != null)
			{
				list.Add(attachInfo);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			AttachInfo attachInfo2 = list[j];
			Skill_attach_skill.CreateAndAttachSkill(attachInfo2._classCard, attachInfo2._attachSkill, attachInfo2._targetSkillBuildInfo);
		}
		battlePlayerBase.AvatarBattlePassiveSkillDescInfo = new BattlePlayerBase.AvatarBattleDescInfo(avatarBattleInfo.Bonus.PassiveAbilityDesc, "");
		battlePlayerBase.ChoiceBraveSkillDescInfoList = new List<BattlePlayerBase.AvatarBattleDescInfo>();
		for (int k = 0; k < bonus.AbilityDesc.Count(); k++)
		{
			battlePlayerBase.ChoiceBraveSkillDescInfoList.Add(new BattlePlayerBase.AvatarBattleDescInfo(bonus.AbilityDesc[k], bonus.AbilityCosts[k]));
		}
		VfxMgr.RegisterImmediateVfx(battlePlayerBase.SetBp(battlePlayerBase.IsGameFirst ? bonus.BattleStartFirstPlayerTurnBp : bonus.BattleStartSecondPlayerTurnBp));
		((ClassBattleCardBase)battlePlayerBase.Class).InitBaseMaxLife(bonus.BattleStartMaxLife);
	}

	protected AttachInfo AddAttachSkillToClass(bool isPlayer, string skillText, string myRotationBonusId = "")
	{
		if (skillText == string.Empty)
		{
			return null;
		}
		SkillCreator.SkillBuildInfo targetSkillBuildInfo = Skill_attach_skill.CreateAttachSkillBuildInfo(skillText);
		BattleCardBase battleCardBase = null;
		battleCardBase = ((!isPlayer) ? BattleEnemy.ClassAndInPlayCardList.First((BattleCardBase c) => c is ClassBattleCardBase) : BattlePlayer.ClassAndInPlayCardList.First((BattleCardBase c) => c is ClassBattleCardBase));
		SkillCreator.SkillBuildInfo skillBuildInfo = new SkillCreator.SkillBuildInfo("attach_skill", "none", "character=me", "character=me&target=inplay&card_type=class", "skill=" + skillText, "none");
		SkillBase skillBase = battleCardBase.CreateSkillCreator(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer, BattleResourceMgr).Create(skillBuildInfo, null, isAttachSkill: true);
		battleCardBase.NormalSkills.Add(skillBase);
		battleCardBase.NormalSkillBuildInfos.Add(skillBuildInfo);
		return new AttachInfo(battleCardBase, skillBase, targetSkillBuildInfo, myRotationBonusId);
	}

	public VfxBase LoadCardResources(List<BattleCardBase> cards, bool isRecoveryFinish = false)
	{
		if (cards.Count == 0)
		{
			return NullVfx.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<ResourceInfo> list = new List<ResourceInfo>();
		for (int i = 0; i < cards.Count; i++)
		{
			BattleCardBase battleCardBase = cards[i];
			if (battleCardBase != null)
			{
				parallelVfxPlayer.Register(battleCardBase.BattleCardView.GetResourcePathes(list));
				parallelVfxPlayer.Register(battleCardBase.BattleCardView.GetChoiceTransformCardsResourcePathes(battleCardBase, list, isRecoveryFinish));
			}
		}
		List<string> list2 = new List<string>();
		Action action = delegate
		{
		};
		for (int num = 0; num < list.Count; num++)
		{
			ResourceInfo info = list[num];
			if (info.ObjectPath != string.Empty)
			{
				string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(info.ObjectPath, ResourcesManager.AssetLoadPathType.Effect2D);
				if (!list2.Contains(assetTypePath))
				{
					list2.Add(assetTypePath);
				}
			}
			else if (!list2.Contains(info.ObjectFullPath))
			{
				list2.Add(info.ObjectFullPath);
			}
			if (info.SePath != string.Empty)
			{
				string item = "s/" + info.SePath + ".acb";
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
			}
			if (info.IsEffectBattleInfoDictionary)
			{
				BattleResourceMgr.AddEffectBattleInfoDictionary(info.ObjectPath, info.SePath);
				action = (Action)Delegate.Combine(action, (Action)delegate
				{
					BattleResourceMgr.SetEffectBattleInfoDictionary(info.ObjectPath, info.ObjectFullPath);
				});
			}
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		return sequentialVfxPlayer;
	}
}
