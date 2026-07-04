using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Scripts.Network.Data.TaskData.Arena;

namespace Wizard;

public class RewardBase : MonoBehaviour
{
	private class RewardCardData
	{
		public int _id;

		public int _num;

		public bool _isSpotCard;

		public RewardCardData(int id, int num, bool isSpotCard = false)
		{
			_id = id;
			_num = num;
			_isSpotCard = isSpotCard;
		}
	}

	[SerializeField]
	private UIGrid RewardGrid;

	[SerializeField]
	private GameObject RewardCard;

	[SerializeField]
	private NguiObjs RewardSprite;

	[SerializeField]
	private UILabel RewardLabel;

	[SerializeField]
	private GameObject _mypageCamera;

	[SerializeField]
	private CardDetailUI CardDetailPrefab;

	private CardDetailUI CardDetail;

	[SerializeField]
	private GameObject CardDetailRoot;

	[SerializeField]
	private UIButton _buttonPrev;

	[SerializeField]
	private UIButton _buttonNext;

	[SerializeField]
	private UIEventListener _flickCollider;

	[SerializeField]
	private UILabel _labelPage;

	[SerializeField]
	private GameObject _blankCollider;

	[SerializeField]
	private UILabel _labelTitle;

	[SerializeField]
	private bool _anyTimeCentering;

	private List<RewardCardData> _rewardCardDataList = new List<RewardCardData>();

	private bool _isPaging = true;

	private List<string> _rewardThumbnailAssetList = new List<string>();

	public List<RewardObjectInfo> _rewardObjectInfoList = new List<RewardObjectInfo>();

	private Vector3 _cardScale = new Vector3(0.93f, 0.93f, 1f);

	private readonly Vector3 POS_GRID_CENTER = new Vector3(0f, 50f, 0f);

	private readonly Vector3 POS_GRID_LEFT = new Vector3(-270f, 50f, 0f);

	private readonly Vector2 CardColliderScale = new Vector2(0.75f, 1f);

	private List<GameObject> _rewardObjList = new List<GameObject>();

	private int _currentPage;

	private int _lastPage;

	private bool _isDragStart;

	private void CreateDetail()
	{
		if (!(CardDetail != null))
		{
			CardDetail = UnityEngine.Object.Instantiate(CardDetailPrefab);
			CardDetail.transform.parent = CardDetailRoot.transform;
			CardDetail.transform.localPosition = Vector3.zero;
			CardDetail.transform.localScale = Vector3.one;
			CardDetail.OnClose = OnCardDetailClose;
			CardDetail.gameObject.SetActive(value: false);
			CardDetail.Initialize(CardDetail.gameObject.layer, CardMaster.CardMasterId.Default);
			CardDetail.IsShowFlavorTextButton = true;
			CardDetail.IsShowVoiceButton = true;
			CardDetail.IsShowEvolutionButton = true;
		}
	}

	public void Awake()
	{
		SystemText systemText = Data.SystemText;
		RewardLabel.text = systemText.Get("Story_0030");
		SetTitleLabel(isEnabled: false);
		UIEventListener flickCollider = _flickCollider;
		flickCollider.onDragStart = (UIEventListener.VoidDelegate)Delegate.Combine(flickCollider.onDragStart, new UIEventListener.VoidDelegate(OnDragStart));
		UIEventListener flickCollider2 = _flickCollider;
		flickCollider2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(flickCollider2.onDrag, new UIEventListener.VectorDelegate(OnDrag));
		if (_mypageCamera != null)
		{
			_mypageCamera.gameObject.SetActive(value: false);
		}
	}

	private void OnDragStart(GameObject obj)
	{
		_isDragStart = true;
	}

	private void OnDrag(GameObject obj, Vector2 dir)
	{
		if (_isDragStart)
		{
			if (dir.x >= 70f)
			{
				_isDragStart = false;
				OnBtnPrevPage();
			}
			else if (dir.x <= -70f)
			{
				_isDragStart = false;
				OnBtnNextPage();
			}
		}
	}

	public void EndCreate()
	{
		if (_rewardCardDataList.Count > 0)
		{
			_buttonPrev.gameObject.SetActive(value: false);
			_buttonNext.gameObject.SetActive(value: false);
			int layer = LayerMask.NameToLayer("SystemUI");
			RewardGrid.gameObject.layer = layer;
			RewardGrid.gameObject.SetActive(value: false);
			List<int> cardNums = _rewardCardDataList.Select((RewardCardData data) => data._id).ToList();
			UIManager.GetInstance().CardLoadSelect(null, cardNums, layer, is2D: true, OnLoadFinished);
		}
		else
		{
			if (_isPaging)
			{
				ShowPage(1);
			}
			UIManager.GetInstance().closeInSceneCenterLoading();
		}
	}

	public void AddReward(ReceivedReward r)
	{
		AddReward((UserGoods.Type)r.reward_type, r.rewardUserGoodsId, r.reward_count);
	}

	private void _LoadAndSetRewardTex(string strFileName, ResourcesManager.AssetLoadPathType pathType, UITexture tex, List<string> loadedPathList)
	{
		string strPath = Toolbox.ResourcesManager.GetAssetTypePath(strFileName, pathType);
		StartCoroutine(Toolbox.ResourcesManager.LoadAssetAsync(strPath, delegate
		{
			string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(strFileName, pathType, isfetch: true);
			tex.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath);
			loadedPathList.Add(strPath);
		}));
	}

	private void _LoadAndSetRewardSleeveTex(long UserGoodId, string strFileName, ResourcesManager.AssetLoadPathType pathType, UITexture tex, List<string> loadedPathList)
	{
		List<string> loadTryList = new List<string>();
		Sleeve sleeve = Data.Master.SleeveMgr.Get(UserGoodId);
		if (sleeve.IsPremiumSleeve)
		{
			UIManager.GetInstance().getUIBase_CardManager().AddPremireSleevePath(ref loadTryList, sleeve);
		}
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(strFileName, pathType);
		loadTryList.Add(assetTypePath);
		StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(loadTryList, delegate
		{
			UIManager.GetInstance().getUIBase_CardManager().SetSleeveTexture(tex, UserGoodId);
			loadedPathList.AddRange(loadTryList);
		}));
	}

	private NguiObjs _AddRewardObj(GameObject parent)
	{
		NguiObjs component = NGUITools.AddChild(parent, RewardSprite.gameObject).GetComponent<NguiObjs>();
		component.gameObject.SetActive(value: true);
		_rewardObjList.Add(component.gameObject);
		return component;
	}

	public NguiObjs AddReward(UserGoods.Type type, long userGoodsId, int number)
	{
		NguiObjs nguiObjs = null;
		switch (type)
		{
		case UserGoods.Type.Item:
		{
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], AreaSelInfo.GetPresentItemName((int)type, userGoodsId), number);
			Item item = Data.Master.ItemList.Find((Item data) => data.UserGoodsId == userGoodsId);
			if (item != null)
			{
				_LoadAndSetRewardTex(item.thumbnail, ResourcesManager.AssetLoadPathType.Item, nguiObjs.textures[0], _rewardThumbnailAssetList);
			}
			break;
		}
		case UserGoods.Type.Sleeve:
		{
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], AreaSelInfo.GetPresentItemName((int)type, userGoodsId), number);
			long existingSleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(userGoodsId);
			_LoadAndSetRewardSleeveTex(existingSleeveId, existingSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture, nguiObjs.textures[0], _rewardThumbnailAssetList);
			nguiObjs.textures[0].width = 170;
			nguiObjs.textures[0].height = 230;
			break;
		}
		case UserGoods.Type.RedEther:
		case UserGoods.Type.Degree:
		case UserGoods.Type.Rupy:
		case UserGoods.Type.SpotCardPoint:
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], AreaSelInfo.GetPresentItemName((int)type, userGoodsId), number);
			_LoadAndSetRewardTex(UserGoods.GetUserGoodsImageName(type, 0L), ResourcesManager.AssetLoadPathType.Item, nguiObjs.textures[0], _rewardThumbnailAssetList);
			break;
		case UserGoods.Type.Emblem:
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], AreaSelInfo.GetPresentItemName((int)type, userGoodsId), number);
			_LoadAndSetRewardTex(userGoodsId.ToString(), ResourcesManager.AssetLoadPathType.Emblem_M, nguiObjs.textures[0], _rewardThumbnailAssetList);
			break;
		case UserGoods.Type.Card:
			CreateDetail();
			_rewardCardDataList.Add(new RewardCardData((int)userGoodsId, number));
			break;
		case UserGoods.Type.SpotCard:
		case UserGoods.Type.SpotCardOnlyLatestCardPack:
			CreateDetail();
			_rewardCardDataList.Add(new RewardCardData((int)userGoodsId, number, isSpotCard: true));
			break;
		case UserGoods.Type.Skin:
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], AreaSelInfo.GetPresentItemName((int)type, userGoodsId), number);
			_LoadAndSetRewardTex(userGoodsId.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail, nguiObjs.textures[0], _rewardThumbnailAssetList);
			nguiObjs.textures[0].width = 170;
			nguiObjs.textures[0].height = 136;
			if (userGoodsId == 4403)
			{
				nguiObjs.textures[0].width = 257;
				nguiObjs.textures[0].height = 206;
			}
			break;
		case UserGoods.Type.MyPageBG:
			nguiObjs = _AddRewardObj(RewardGrid.gameObject);
			SetRewardNameAndNumText(nguiObjs.labels[0], Data.Master.MyPageCustomBGMaster[userGoodsId.ToString()].Name, number);
			_LoadAndSetRewardTex("thumbnail_mypage_custom_bg", ResourcesManager.AssetLoadPathType.Item, nguiObjs.textures[0], _rewardThumbnailAssetList);
			break;
		}
		_rewardObjectInfoList.Add(new RewardObjectInfo(type, nguiObjs));
		return nguiObjs;
	}

	public void OnBtnNextPage()
	{
		if (_currentPage < GetLastPage())
		{

			ShowPage(_currentPage + 1);
		}
	}

	public void OnBtnPrevPage()
	{
		if (_currentPage > 1)
		{

			ShowPage(_currentPage - 1);
		}
	}

	private int GetLastPage()
	{
		if (_lastPage <= 0)
		{
			_lastPage = (_rewardObjList.Count - 1) / 3 + 1;
		}
		return _lastPage;
	}

	private void SetRewardNameAndNumText(UILabel label, string strRewardName, int num)
	{
		label.SetWrapText(strRewardName + Data.SystemText.Get("Common_0040", num.ToString()));
	}

	private void OnCardDetailClose()
	{
		StartCoroutine(StopBlankCollider());
	}

	private IEnumerator StopBlankCollider()
	{
		yield return null;
		_blankCollider.SetActive(value: false);
	}

	private void OnLoadFinished()
	{
		List<UIBase_CardManager.CardObjData> cardList2DObjs = UIManager.GetInstance().getCardList2DObjs();
		for (int i = 0; i < cardList2DObjs.Count; i++)
		{
			UIBase_CardManager.CardObjData cardObjData = UIManager.GetInstance().getCardList2DObjs()[i];
			if (cardObjData != null && cardObjData.CardObj != null)
			{
				RewardCardData rewardCardData = _rewardCardDataList[i];
				string strRewardName = (rewardCardData._isSpotCard ? (cardObjData.Names + " " + Data.SystemText.Get("Mail_0062")) : cardObjData.Names);
				GameObject gameObject = NGUITools.AddChild(RewardGrid.gameObject, RewardCard);
				SetRewardNameAndNumText(gameObject.GetComponentInChildren<UILabel>(), strRewardName, rewardCardData._num);
				gameObject.SetActive(value: true);
				_rewardObjList.Add(gameObject);
				GameObject obj = cardObjData.CardObj;
				obj.SetActive(value: false);
				obj.transform.parent = gameObject.gameObject.transform;
				obj.SetActive(value: true);
				obj.transform.localScale = _cardScale;
				obj.transform.localPosition = Vector3.zero;
				CardListTemplate component = obj.GetComponent<CardListTemplate>();
				GameObject obj2 = component._frameSprite.gameObject;
				component.HideNum();
				obj2.AddComponent<BoxCollider>().size = component._frameSprite.localSize * CardColliderScale;
				UIEventListener uIEventListener = UIEventListener.Get(obj2);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
				{
					_blankCollider.SetActive(value: true);
					CardDetail.OnPushCardDetailOn(obj);
				});
				UIEventListener uIEventListener2 = UIEventListener.Get(obj2);
				uIEventListener2.onDragStart = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onDragStart, new UIEventListener.VoidDelegate(OnDragStart));
				UIEventListener uIEventListener3 = UIEventListener.Get(obj2);
				uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, new UIEventListener.VectorDelegate(OnDrag));
			}
		}
		ShowPage(1);
		UIManager.GetInstance().closeInSceneCenterLoading();
	}

	private void ShowPage(int page)
	{
		_currentPage = page;
		for (int i = 0; i < _rewardObjList.Count; i++)
		{
			_rewardObjList[i].gameObject.SetActive(value: false);
		}
		int num = (page - 1) * 3;
		for (int j = 0; j < 3; j++)
		{
			int num2 = j + num;
			if (num2 >= _rewardObjList.Count)
			{
				break;
			}
			_rewardObjList[num2].gameObject.SetActive(value: true);
		}
		RewardGrid.gameObject.SetActive(value: true);
		if (_currentPage > 1 && !_anyTimeCentering)
		{
			RewardGrid.transform.localPosition = POS_GRID_LEFT;
			RewardGrid.pivot = UIWidget.Pivot.BottomLeft;
		}
		else
		{
			RewardGrid.transform.localPosition = POS_GRID_CENTER;
			RewardGrid.pivot = UIWidget.Pivot.Bottom;
		}
		RewardGrid.Reposition();
		if (GetLastPage() <= 1)
		{
			_flickCollider.gameObject.SetActive(value: false);
			_labelPage.transform.parent.gameObject.SetActive(value: false);
			_buttonPrev.gameObject.SetActive(value: false);
			_buttonNext.gameObject.SetActive(value: false);
			return;
		}
		_flickCollider.gameObject.SetActive(value: true);
		_labelPage.transform.parent.gameObject.SetActive(value: true);
		_labelPage.text = Data.SystemText.Get("Card_0053", _currentPage.ToString(), GetLastPage().ToString());
		if (_currentPage <= 1)
		{
			_buttonPrev.gameObject.SetActive(value: false);
			_buttonNext.gameObject.SetActive(value: true);
		}
		else if (_currentPage >= GetLastPage())
		{
			_buttonPrev.gameObject.SetActive(value: true);
			_buttonNext.gameObject.SetActive(value: false);
		}
		else
		{
			_buttonPrev.gameObject.SetActive(value: true);
			_buttonNext.gameObject.SetActive(value: true);
		}
	}

	public void SetTitleLabel(bool isEnabled, string title = null)
	{
		_labelTitle.gameObject.SetActive(isEnabled);
		_labelTitle.text = title;
	}
}
