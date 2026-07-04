using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 47 of 50 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.MulliganInfoControl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard.Battle.Mulligan;

public class MulliganInfoControl : UIBase
{
	public enum ViewType
	{
		Normal,
		Watch
	}

	private enum TitleType
	{
		MulliganHelp,
		WaitOpponent,
		Watch
	}

	[Serializable]
	private class MulliganParts
	{
		public UIWidget _keepZone;

		public UISprite _keepBG;

		public UISprite _keepText;

		public UIWidget _abandonZone;

		public UISprite _abandonBG;

		public UISprite _abandonText;

		public UISprite[] _exchangeMark;
	}

	private static readonly string MARIGAN_PANEL_CLASS = "battle_marigan_panel_class_";

	private static readonly Vector3 ALL_UI_ROTATION = new Vector3(-10f, 0f, 0f);

	private static readonly Vector3 ALL_UI_POSITION = new Vector3(0f, 0f, -80f);

	private static readonly Vector3 SUBMIT_BUTTON_SCALE = new Vector3(1.4f, 1.4f, 1f);

	private static readonly float[] ABANDON_TEXT_OFFSET_BOTTOM = new float[2] { -5f, -50f };

	private static readonly float[] ABANDON_TEXT_OFFSET_TOP = new float[2] { 55f, 10f };

	private static readonly float[] KEEP_TEXT_OFFSET_BOTTOM = new float[2] { -5f, -50f };

	private static readonly float[] KEEP_TEXT_OFFSET_TOP = new float[2] { 55f, 10f };

	private static readonly int[] CARD_POS_Y = new int[2] { -25, 25 };

	private static readonly float[][] CHANGEMARK_POS_Y = new float[2][]
	{
		new float[2] { -25f, 0f },
		new float[2] { 25f, -25f }
	};

	private static readonly Vector3[] CHANGEMARK_SCALE = new Vector3[2]
	{
		new Vector3(1f, 1f, 1f),
		new Vector3(0.92f, 0.92f, 1f)
	};

	private static readonly float[] CARD_POS_OFFSET_X = new float[2] { 400f, 260f };

	private static readonly Vector3[] CARD_SCALE = new Vector3[2]
	{
		new Vector3(1.2f, 1.2f, 1f),
		new Vector3(1.1f, 1.1f, 1f)
	};

	private static readonly int[] ZONE_WIDTH = new int[2] { 1240, 819 };

	private static readonly int[] ZONE_POS_X = new int[2] { 0, 416 };

	private static readonly Vector3 ENEMY_CLASS_ICON_POSITION = new Vector3(-274.2f, -33f, 0f);

	private static readonly Vector3[] ENEMY_CLASS_ICON_POSITION_2 = new Vector3[2]
	{
		new Vector3(-274.2f, -21.9f, 0f),
		new Vector3(-274.2f, -58f, 0f)
	};

	private static readonly Vector3 ENEMY_CLASS_ICON_POSITION_MY_ROTATION = new Vector3(-274.2f, -50f, 0f);

	private static readonly Vector3 ENEMY_CLASS_ICON_SCALE = new Vector3(1.2f, 1.2f, 1.2f);

	private static readonly Vector2[] ENEMY_CLASS_INFO_LABEL_SIZE = new Vector2[2]
	{
		new Vector2(300f, 66f),
		new Vector2(300f, 80f)
	};

	private static readonly Vector2 ENEMY_CLASS_INFO_LABEL_SIZE_MY_ROTATION = new Vector2(300f, 100f);

	private static readonly Vector3[] ENEMY_CLASS_INFO_LABEL_POSITION = new Vector3[2]
	{
		new Vector3(-47f, -36f, 0f),
		new Vector3(-47f, -43f, 0f)
	};

	private static readonly Vector2 ENEMY_CLASS_INFO_LABEL_POSITION_MY_ROTATION = new Vector3(-47f, -70f, 0f);

	private static readonly Vector3 MY_ROTATION_INFO_POSITION = new Vector3(-246f, -21.9f, 0f);

	private static readonly string[] USE_SHORT_WIDTH_LANGUAGE = new string[2] { "Jpn", "Kor" };

	[SerializeField]
	public UISprite TurnImg;

	[SerializeField]
	public UILabel TurnLabel;

	[SerializeField]
	public UISprite TitleBar;

	[SerializeField]
	private UILabel TitleLabel;

	[SerializeField]
	private UILabel WatchBattleTitleLabel;

	[SerializeField]
	private UILabel WatchBattleOrderLabel;

	[SerializeField]
	public UISprite EnemyInfo;

	[SerializeField]
	private UILabel EnemyInfoLabel;

	[SerializeField]
	private UISprite EnemyInfoClassIcon_1;

	[SerializeField]
	private UISprite EnemyInfoClassIcon_2;

	[SerializeField]
	private GameObject _enemyInfoMyRotationInfo;

	[SerializeField]
	private UILabel _packName;

	[SerializeField]
	private UIGrid _myRotationIconGrid;

	[SerializeField]
	private GameObject _myRotationIconOriginal;

	[SerializeField]
	private GameObject TimerObj;

	[SerializeField]
	public UIButton SubmitBtn;

	[SerializeField]
	private UILabel SubmitBtnLabel;

	[SerializeField]
	private MulliganParts _partsPlayer;

	[SerializeField]
	private MulliganParts _partsOpponent;

	[SerializeField]
	private UIAnchor AnchorTL;

	[SerializeField]
	private UIAnchor AnchorTR;

	[SerializeField]
	private UIAnchor AnchorBR;

	private ViewType _viewType;

	private Camera m_3DCamera;

	private BattleManagerBase m_BtlMgrIns;

	private bool isTimerOn;

	private int StateCnt;

	public bool IsEnd { get; private set; }

	private int SCREEN_HEIGHT => m_3DCamera.pixelHeight;

	public event Action OnStartMulligan;

	public event Func<VfxBase> OnEndMulligan;

	public event Func<VfxBase> OnTimeUp;

	public void InitMulliganInfo()
	{
		// Pre-Phase-5b: MulliganInfoControl is UI-only headless; camera lookup + AttachAtlas dropped
		m_BtlMgrIns = null;
	}

	public override void assetBundleEnd()
	{
		base.assetBundleEnd();
		StartCoroutine(assetSetting());
	}

	public void Show(ViewType type)
	{
		_viewType = type;
		InitiallizeView();
		TweenAlpha.Begin(TitleBar.gameObject, 0f, 0f);
		TweenAlpha.Begin(_partsPlayer._keepBG.gameObject, 0f, 0f);
		TweenAlpha.Begin(_partsPlayer._abandonBG.gameObject, 0f, 0f);
		TweenAlpha.Begin(_partsOpponent._keepBG.gameObject, 0f, 0f);
		TweenAlpha.Begin(_partsOpponent._abandonBG.gameObject, 0f, 0f);
		TimerObj.SetActive(value: false);
		EnableButton(on: false);
		if (m_BtlMgrIns.IsFirst)
		{
			TurnLabel.text = Data.SystemText.Get("Battle_0430");
		}
		else
		{
			TurnLabel.text = Data.SystemText.Get("Battle_0431");
		}
		TweenAlpha.Begin(TurnImg.gameObject, 0f, 0f);
		TweenAlpha.Begin(EnemyInfo.gameObject, 0f, 0f);
		isTimerOn = false;
		StateCnt = 0;
		base.gameObject.SetActive(value: true);
		SetSubmitLabel();
		if (false /* Pre-Phase-5b: IsWatchBattle const-false */)
		{
			_SetTitleLabel(TitleType.Watch);
		}
		else
		{
			_SetTitleLabel(TitleType.MulliganHelp);
		}
		SetEnemyClassInfo();
		this.OnStartMulligan.Call();
		this.OnStartMulligan = null;
	}

	public void HideButtons()
	{
		isTimerOn = false;
		TimerObj.SetActive(value: false);
		EnableButton(on: false);
		if (StateCnt < 2 /* Pre-Phase-5b: IsNetworkBattle assumed true; IsWatchBattle const-false */)
		{
			_SetTitleLabel(TitleType.WaitOpponent);
		}
	}

	public void HideTopPanels()
	{
		iTween.MoveTo(TurnImg.gameObject, iTween.Hash("x", -120f, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeInOutQuad));
		iTween.MoveTo(EnemyInfo.gameObject, iTween.Hash("x", 340f, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeInOutQuad));
	}

	public void HideMulliganTitle()
	{
		TweenAlpha.Begin(TitleBar.gameObject, 0.5f, 0f);
	}

	public void HideMulliganChangeUI()
	{
		TweenAlpha.Begin(_partsPlayer._abandonBG.gameObject, 0.5f, 0f);
	}

	public void HideMulliganOpponentChangeUI()
	{
		TweenAlpha.Begin(_partsOpponent._abandonBG.gameObject, 0.5f, 0f);
	}

	public void InitiallizeView()
	{
		AnchorTL.uiCamera = m_3DCamera;
		AnchorTR.uiCamera = m_3DCamera;
		AnchorBR.uiCamera = m_3DCamera;
		SubmitBtn.gameObject.GetComponent<UIAnchor>().uiCamera = m_3DCamera;
		SubmitBtn.transform.localScale = SUBMIT_BUTTON_SCALE;
		Vector3 localPosition = SubmitBtn.transform.localPosition;
		SubmitBtn.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -1f);
		base.transform.localRotation = Quaternion.Euler(ALL_UI_ROTATION);
		base.transform.localPosition = ALL_UI_POSITION;
		int width = ZONE_WIDTH[(int)_viewType];
		float num = ZONE_POS_X[(int)_viewType];
		float num2 = CARD_POS_OFFSET_X[(int)_viewType];
		float[] array = CHANGEMARK_POS_Y[(int)_viewType];
		for (int i = 0; i < 3; i++)
		{
			_partsPlayer._exchangeMark[i].transform.localPosition = new Vector3(num2 * (float)(i - 1), array[0], -12f);
			_partsOpponent._exchangeMark[i].transform.localPosition = new Vector3(num2 * (float)(i - 1), array[1], -12f);
		}
		for (int j = 0; j < 3; j++)
		{
			_partsPlayer._exchangeMark[j].transform.localScale = CHANGEMARK_SCALE[(int)_viewType];
			_partsOpponent._exchangeMark[j].transform.localScale = CHANGEMARK_SCALE[(int)_viewType];
		}
		_partsPlayer._keepZone.width = width;
		_partsPlayer._abandonZone.width = width;
		_partsOpponent._keepZone.width = width;
		_partsOpponent._abandonZone.width = width;
		_partsPlayer._keepZone.transform.localPosition = new Vector3(0f - num, 0f, 0f);
		_partsPlayer._abandonZone.transform.localPosition = new Vector3(num, 0f, 0f);
		_partsOpponent._keepZone.transform.localPosition = new Vector3(0f - num, 0f, 0f);
		_partsOpponent._abandonZone.transform.localPosition = new Vector3(num, 0f, 0f);
		Transform target = m_BtlMgrIns.Battle3DContainer.transform;
		_partsPlayer._keepZone.topAnchor.target = target;
		_partsPlayer._keepZone.bottomAnchor.target = target;
		_partsPlayer._abandonZone.topAnchor.target = target;
		_partsPlayer._abandonZone.bottomAnchor.target = target;
		_partsOpponent._keepZone.topAnchor.target = target;
		_partsOpponent._keepZone.bottomAnchor.target = target;
		_partsOpponent._abandonZone.topAnchor.target = target;
		_partsOpponent._abandonZone.bottomAnchor.target = target;
		_partsPlayer._keepZone.topAnchor.Set(0.5f, -50f);
		_partsPlayer._keepZone.bottomAnchor.Set(0f, 80f);
		if (_viewType == ViewType.Normal)
		{
			_partsPlayer._abandonZone.topAnchor.Set(1f, -80f);
			_partsPlayer._abandonZone.bottomAnchor.Set(0.5f, 50f);
		}
		else
		{
			_partsPlayer._abandonZone.topAnchor.Set(0.5f, -50f);
			_partsPlayer._abandonZone.bottomAnchor.Set(0f, 80f);
			_partsOpponent._keepZone.topAnchor.Set(1f, -80f);
			_partsOpponent._keepZone.bottomAnchor.Set(0.5f, 50f);
			_partsOpponent._abandonZone.topAnchor.Set(1f, -80f);
			_partsOpponent._abandonZone.bottomAnchor.Set(0.5f, 50f);
			_partsPlayer._abandonText.bottomAnchor.Set(0f, ABANDON_TEXT_OFFSET_BOTTOM[0]);
			_partsPlayer._abandonText.topAnchor.Set(0f, ABANDON_TEXT_OFFSET_TOP[0]);
			_partsPlayer._abandonText.spriteName = "battle_mulligan_change_2";
			_partsOpponent._abandonText.bottomAnchor.Set(1f, ABANDON_TEXT_OFFSET_BOTTOM[1]);
			_partsOpponent._abandonText.topAnchor.Set(1f, ABANDON_TEXT_OFFSET_TOP[1]);
			_partsOpponent._abandonText.spriteName = "battle_mulligan_change_1";
			_partsOpponent._keepText.bottomAnchor.Set(1f, KEEP_TEXT_OFFSET_BOTTOM[1]);
			_partsOpponent._keepText.topAnchor.Set(1f, KEEP_TEXT_OFFSET_TOP[1]);
		}
		TitleBar.topAnchor.target = target;
		TitleBar.topAnchor.Set(0.5f, 40f);
		TitleBar.bottomAnchor.target = target;
		TitleBar.bottomAnchor.Set(0.5f, -40f);
		TitleBar.leftAnchor.target = target;
		TitleBar.rightAnchor.target = target;
		float num3 = 180f; // Pre-Phase-5b: IsWatchBattle / IsReplayBattle const-false
		TitleBar.leftAnchor.Set(0f, num3 * m_3DCamera.aspect);
		TitleBar.rightAnchor.Set(1f, (0f - num3) * m_3DCamera.aspect);
	}

	private void EnableButton(bool on)
	{
		SubmitBtn.gameObject.SetActive(on);
	}

	public VfxBase SetPlayerReady()
	{
		StateCnt++;
		IsEnd = true;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxBase[] allFuncCallResults = this.OnEndMulligan.GetAllFuncCallResults();
		foreach (VfxBase vfx in allFuncCallResults)
		{
			sequentialVfxPlayer.Register(vfx);
		}
		return sequentialVfxPlayer;
	}

	public void SetEnemyReady(int num)
	{
		SystemText systemText = Data.SystemText;
		if (num > 0)
		{
			EnemyInfoLabel.text = systemText.Get("Battle_0107", num.ToString());
			StateCnt++;
		}
		else if (num == 0)
		{
			EnemyInfoLabel.text = systemText.Get("Battle_0106");
			StateCnt++;
		}
		else
		{
			EnemyInfoLabel.text = systemText.Get("Battle_0105");
		}
	}

	private void SetEnemyClassInfo()
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		MyRotationInfo myRotationInfo;
		if (dataMgr.GetEnemySubClassId() != 10)
		{
			EnemyInfoClassIcon_1.spriteName = ClassCharaPrm.GetIconSpriteName((CardBasePrm.ClanType)dataMgr.GetEnemyClassId());
			EnemyInfoClassIcon_2.spriteName = ClassCharaPrm.GetIconSpriteName((CardBasePrm.ClanType)dataMgr.GetEnemySubClassId());
			EnemyInfoClassIcon_1.transform.localPosition = ENEMY_CLASS_ICON_POSITION_2[0];
			EnemyInfoClassIcon_2.transform.localPosition = ENEMY_CLASS_ICON_POSITION_2[1];
			EnemyInfoClassIcon_1.transform.localScale = ENEMY_CLASS_ICON_SCALE;
			EnemyInfoClassIcon_2.transform.localScale = ENEMY_CLASS_ICON_SCALE;
			EnemyInfoClassIcon_2.transform.gameObject.SetActive(value: true);
			_enemyInfoMyRotationInfo.SetActive(value: false);
			EnemyInfo.width = (int)ENEMY_CLASS_INFO_LABEL_SIZE[1].x;
			EnemyInfo.height = (int)ENEMY_CLASS_INFO_LABEL_SIZE[1].y;
			EnemyInfoLabel.transform.localPosition = ENEMY_CLASS_INFO_LABEL_POSITION[1];
		}
		else if (dataMgr.TryGetEnemyMyRotationInfo(out myRotationInfo))
		{
			EnemyInfoClassIcon_1.spriteName = ClassCharaPrm.GetIconSpriteName((CardBasePrm.ClanType)dataMgr.GetEnemyClassId());
			EnemyInfoClassIcon_1.transform.localPosition = ENEMY_CLASS_ICON_POSITION_MY_ROTATION;
			EnemyInfoClassIcon_1.transform.localScale = ENEMY_CLASS_ICON_SCALE;
			EnemyInfoClassIcon_2.transform.gameObject.SetActive(value: false);
			_enemyInfoMyRotationInfo.SetActive(value: true);
			_enemyInfoMyRotationInfo.transform.localPosition = MY_ROTATION_INFO_POSITION;
			_packName.text = myRotationInfo.LastPackText;
			_packName.width = (USE_SHORT_WIDTH_LANGUAGE.Contains(CustomPreference.GetTextLanguage()) ? 50 : 80);
			_myRotationIconOriginal.SetActive(value: false);
			_myRotationIconGrid.transform.DestroyChildren();
			for (int i = 0; i < myRotationInfo.Abilities.Count; i++)
			{
				GameObject obj = NGUITools.AddChild(_myRotationIconGrid.gameObject, _myRotationIconOriginal);
				obj.GetComponent<UISprite>().spriteName = myRotationInfo.Abilities[i].IconName;
				obj.SetActive(value: true);
			}
			_myRotationIconGrid.Reposition();
			EnemyInfo.width = (int)ENEMY_CLASS_INFO_LABEL_SIZE_MY_ROTATION.x;
			EnemyInfo.height = (int)ENEMY_CLASS_INFO_LABEL_SIZE_MY_ROTATION.y;
			EnemyInfoLabel.transform.localPosition = ENEMY_CLASS_INFO_LABEL_POSITION_MY_ROTATION;
		}
		else
		{
			EnemyInfoClassIcon_1.spriteName = ClassCharaPrm.GetIconSpriteName((CardBasePrm.ClanType)dataMgr.GetEnemyClassId());
			EnemyInfoClassIcon_1.transform.localPosition = ENEMY_CLASS_ICON_POSITION;
			EnemyInfoClassIcon_1.transform.localScale = ENEMY_CLASS_ICON_SCALE;
			EnemyInfoClassIcon_2.transform.gameObject.SetActive(value: false);
			_enemyInfoMyRotationInfo.SetActive(value: false);
			EnemyInfo.width = (int)ENEMY_CLASS_INFO_LABEL_SIZE[0].x;
			EnemyInfo.height = (int)ENEMY_CLASS_INFO_LABEL_SIZE[0].y;
			EnemyInfoLabel.transform.localPosition = ENEMY_CLASS_INFO_LABEL_POSITION[0];
		}
		EnemyInfo.spriteName = MARIGAN_PANEL_CLASS + dataMgr.GetEnemyClassId().ToString("00");
	}

	private void SetSubmitLabel()
	{
		SystemText systemText = Data.SystemText;
		SubmitBtnLabel.text = systemText.Get("Battle_0102");
	}

	private void _SetTitleLabel(TitleType type)
	{
		TitleLabel.text = string.Empty;
		WatchBattleTitleLabel.text = string.Empty;
		WatchBattleOrderLabel.text = string.Empty;
		switch (type)
		{
		case TitleType.MulliganHelp:
			TitleLabel.text = Data.SystemText.Get("Battle_0101");
			break;
		case TitleType.WaitOpponent:
			TitleLabel.text = Data.SystemText.Get("Battle_0104");
			break;
		case TitleType.Watch:
			if (false /* Pre-Phase-5b: IsReplayBattle const-false */)
			{
				TitleLabel.text = Data.SystemText.Get("Battle_0467");
				break;
			}
			WatchBattleTitleLabel.text = Data.SystemText.Get("Battle_0467");
			WatchBattleOrderLabel.text = (m_BtlMgrIns.IsFirst ? Data.SystemText.Get("Battle_0514") : Data.SystemText.Get("Battle_0515"));
			break;
		}
	}

	public VfxBase DestroyMulliganUIVfx()
	{
		return InstantVfx.Create(delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	public RaycastHit[] GetRaycastHitFromPosition(Vector3 position)
	{
		Ray ray = m_3DCamera.ScreenPointToRay(position);
		return Physics.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity);
	}

	public RaycastHit[] Get2DRaycastHitFromPosition(Vector3 position)
	{
		Ray ray = UIManager.GetInstance().getCamera().ScreenPointToRay(position);
		return Physics.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity);
	}

	public Vector3 ScreenToWorldPoint3D(Vector3 position)
	{
		return m_3DCamera.ScreenToWorldPoint(position);
	}

	public bool IsLeavingKeepZone(Vector3 mousePosition)
	{
		float num = 30f;
		float num2 = (float)SCREEN_HEIGHT / 2f - num;
		return mousePosition.y - num2 > 0f;
	}

	public bool IsLeavingAbandonZone(Vector3 mousePosition)
	{
		float num = 30f;
		float num2 = (float)SCREEN_HEIGHT / 2f + num;
		return mousePosition.y - num2 < 0f;
	}

	public UIWidget GetKeepZonePlayer()
	{
		return _partsPlayer._keepZone;
	}

	public UIWidget GetAbandonZonePlayer()
	{
		return _partsPlayer._abandonZone;
	}

	public UIWidget GetKeepZoneOpponent()
	{
		return _partsOpponent._keepZone;
	}

	public UIWidget GetAbandonZoneOpponent()
	{
		return _partsOpponent._abandonZone;
	}

	public Vector3 GetMulliganZoneCardScale()
	{
		return CARD_SCALE[(int)_viewType];
	}

	public Vector3 GetMulliganZoneCardPos(int index, bool isAbandon, bool isPlayer)
	{
		float num = ((isAbandon && _viewType == ViewType.Normal) ? CARD_POS_Y[0] : CARD_POS_Y[1]);
		if (_viewType == ViewType.Watch && !isPlayer)
		{
			num *= -1f;
		}
		return new Vector3(CARD_POS_OFFSET_X[(int)_viewType] * (float)(index - 1), num, -10f);
	}

	public void SetExchangeMarkPlayer(int index, bool on)
	{
		_partsPlayer._exchangeMark[index].gameObject.SetActive(on);
	}

	public void SetExchangeMarkOpponent(int index, bool on)
	{
		_partsOpponent._exchangeMark[index].gameObject.SetActive(on);
	}
}
