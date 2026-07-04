using System;
using Cute;
using UnityEngine;

public class CommonBackGround : UIBase
{
	public enum eBGType
	{
		NONE,
		MORNING,
		DAYTIME,
		NIGHTTIME
	}

	private static CommonBackGround _instance;

	[SerializeField]
	private UITexture MypageBG;

	[SerializeField]
	private GameObject MagicCircle;

	[SerializeField]
	public ParticleSystem[] BgEffects;

	private eBGType _BGType;

	private bool _bgFinishLoad;

	private string _bgPath;

	private EffectSetUp _currentEffectSetup;

	public static CommonBackGround Instance => _instance;

	public bool IsFinishLod => _bgFinishLoad;

	private void Awake()
	{
		_instance = this;
	}

	protected override void onOpen()
	{
		base.onOpen();
		ChangeMyPageBG();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_instance = null;
		ReleaseMyPageBG();
		_BGType = eBGType.NONE;
		MagicCircle.GetComponent<UITexture>().mainTexture = null;
	}

	private void ReleaseMyPageBG()
	{
		MypageBG.mainTexture = null;
		Toolbox.ResourcesManager.RemoveAsset(_bgPath);
		_bgPath = "";
	}

	public void ChangeMyPageBG()
	{
		int hour = DateTime.Now.Hour;
		eBGType eBGType;
		string newBgStr;
		if (hour >= 4 && hour < 10)
		{
			eBGType = eBGType.MORNING;
			newBgStr = "bg_mypage_morning";
		}
		else if (hour >= 10 && hour < 18)
		{
			eBGType = eBGType.DAYTIME;
			newBgStr = "bg_mypage_day";
		}
		else
		{
			eBGType = eBGType.NIGHTTIME;
			newBgStr = "bg_mypage_night";
		}
		if (_BGType == eBGType)
		{
			return;
		}
		_bgFinishLoad = false;
		_BGType = eBGType;
		ResourcesManager resMgr = Toolbox.ResourcesManager;
		string newBgPath = resMgr.GetAssetTypePath(newBgStr, ResourcesManager.AssetLoadPathType.Background);
		resMgr.StartCoroutine_LoadAssetGroupAsync(newBgPath, delegate
		{
			_bgFinishLoad = true;
			MypageBG.mainTexture = resMgr.LoadObject(resMgr.GetAssetTypePath(newBgStr, ResourcesManager.AssetLoadPathType.Background, isfetch: true)) as Texture;
			if (_bgPath != null)
			{
				resMgr.RemoveAsset(_bgPath);
			}
			_bgPath = newBgPath;
			ChangeBgEffect();
		});
	}

	private void ChangeBgEffect()
	{
		BgEffects[0].gameObject.SetActive(value: false);
		BgEffects[1].gameObject.SetActive(value: false);
		BgEffects[2].gameObject.SetActive(value: false);
		ParticleSystem bgEffectNow = GetBgEffectNow(_BGType);
		bgEffectNow.gameObject.SetActive(value: true);
		_currentEffectSetup = bgEffectNow.GetComponent<EffectSetUp>();
	}

	public ParticleSystem GetBgEffectNow(eBGType myPageBgType)
	{
		return myPageBgType switch
		{
			eBGType.MORNING => BgEffects[2], 
			eBGType.DAYTIME => BgEffects[0], 
			_ => BgEffects[1], 
		};
	}

	public bool IsFinishEffectLoading()
	{
		if (_currentEffectSetup != null)
		{
			return _currentEffectSetup.isFinished;
		}
		return false;
	}
}
