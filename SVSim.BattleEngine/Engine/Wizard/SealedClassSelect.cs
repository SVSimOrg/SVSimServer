using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class SealedClassSelect : MonoBehaviour
{
	[SerializeField]
	private UIGrid _classObjectsGrid;

	[SerializeField]
	private SealedClassSelectObject _classObjectOriginal;

	[SerializeField]
	private GameObject _effectsRoot;

	[SerializeField]
	private GameObject _cardDetailRoot;

	[SerializeField]
	private SealedClassSelectConfirmDialog _classSelectConfirmDialogPrefab;

	private readonly List<string> _unloadAssetList = new List<string>();

	private readonly Dictionary<int, List<CardListTemplate>> _cardObjectsDic = new Dictionary<int, List<CardListTemplate>>();

	private CardDetailUI _cardDetailDialog;

	private List<CardListTemplate> _cardDetailCardObjectList;

	private int _cardDetailCardIndex;

	public bool IsReady { get; private set; }

	private SealedData SealedData => Data.ArenaData.SealedData;

	public void Init()
	{
		StartCoroutine(InitCoroutine());
	}

	private IEnumerator InitCoroutine()
	{
		yield return StartCoroutine(CreateCardObjectsCoroutine());
		List<SealedClassSelectLoadRequest> loadRequestList = CreateClassSelectObjects();
		List<string> loadAssetList = loadRequestList.SelectMany((SealedClassSelectLoadRequest x) => x.LoadAssetList).ToList();
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(loadAssetList, delegate
		{
			_unloadAssetList.AddRange(loadAssetList);
			loadRequestList.ForEach(delegate(SealedClassSelectLoadRequest x)
			{
				x.LoadEndCallback();
			});
		}));
		IsReady = true;
	}

	public void Final()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_unloadAssetList);
		_unloadAssetList.Clear();
	}

	private List<SealedClassSelectLoadRequest> CreateClassSelectObjects()
	{
		List<SealedClassSelectLoadRequest> list = new List<SealedClassSelectLoadRequest>();
		foreach (SealedClassInfo classInfo in SealedData.ClassInfoList)
		{
			SealedClassSelectObject component = NGUITools.AddChild(_classObjectsGrid.gameObject, _classObjectOriginal.gameObject).GetComponent<SealedClassSelectObject>();
			int classId = classInfo.ClassId;
			SealedClassSelectObjectInitParam initParam = new SealedClassSelectObjectInitParam
			{
				CharaParam = null, // Pre-Phase-5b: no chara master headless
				CardObjectList = _cardObjectsDic[classId],
				SelectButtonClickCallback = delegate
				{

					OnSelectClass(classId);
				},
				EffectRoot = _effectsRoot,
				UnloadAssetList = _unloadAssetList
			};
			list.Add(component.Init(initParam));
		}
		_classObjectsGrid.Reposition();
		return list;
	}

	private IEnumerator CreateCardObjectsCoroutine()
	{
		List<int> cardNums = (from x in SealedData.ClassInfoList.SelectMany((SealedClassInfo x) => x.PublishedCardInfoList)
			select x.SealedCardId).ToList();
		UIManager uiMgr = UIManager.GetInstance();
		bool isLoaded = false;
		uiMgr.CardLoadSelect(null, cardNums, base.gameObject.layer, is2D: true, delegate
		{
			isLoaded = true;
		});
		while (!isLoaded)
		{
			yield return null;
		}
		List<UIBase_CardManager.CardObjData> cardList2DObjs = uiMgr.getCardList2DObjs();
		List<UIBase_CardManager.CardObjData> list = new List<UIBase_CardManager.CardObjData>(cardList2DObjs);
		cardList2DObjs.Clear();
		List<string> cardListAssetPathList = Toolbox.ResourcesManager.CardListAssetPathList;
		_unloadAssetList.AddRange(new List<string>(cardListAssetPathList));
		cardListAssetPathList.Clear();
		_cardDetailDialog = DialogCreator.CreateCardDetailDialog(_cardDetailRoot, "Detail");
		_cardDetailDialog.ChangeCardMaster(FormatBehaviorManager.GetDefaultBehaviour(Format.Sealed).CardMasterId);
		_cardDetailDialog.gameObject.SetActive(value: false);
		_cardDetailDialog.OnDragCard = CardDetailDragCallback;
		_cardDetailDialog.OnDetailCardUpdate = UpdateCardDetailArrowButtonVisible;
		int num = 0;
		foreach (SealedClassInfo classInfo in SealedData.ClassInfoList)
		{
			List<CardListTemplate> list2 = new List<CardListTemplate>();
			_cardObjectsDic.Add(classInfo.ClassId, list2);
			for (int num2 = 0; num2 < classInfo.PublishedCardInfoList.Count; num2++)
			{
				GameObject cardObject = list[num++].CardObj;
				CardListTemplate component = cardObject.GetComponent<CardListTemplate>();
				list2.Add(component);
				component.SetId(classInfo.PublishedCardInfoList[num2].SealedCardId);
				component.SetScale(0.64f);
				component.AddDepth(5);
				component.HideNum();
				int tempIndex = num2;
				component.AddColliderToFrame(0.85f).onClick = delegate
				{
					_cardDetailCardObjectList = list2;
					_cardDetailCardIndex = tempIndex;
					_cardDetailDialog.OnPushCardDetailOn(cardObject);
				};
			}
		}
	}

	private void OnSelectClass(int classId)
	{

		OpenClassSelectConfirmDialog(classId);
	}

	private void OpenClassSelectConfirmDialog(int classId)
	{
		Action<Action> decideClassFunc = delegate(Action gotoNextScene)
		{
			SealedData.UnregisterAllSealedCard();
			StartCoroutine(Toolbox.NetworkManager.Connect(new SealedSelectClassTask(classId), delegate
			{
				gotoNextScene();
			}));
		};
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetTitleLabel(systemText.Get("Sealed_ClassSelect_0002"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.GrayBtn_CancelBtn_BlueBtn);
		dialogBase.SetButtonText(systemText.Get("Sealed_ClassSelect_0006"), null, systemText.Get("Common_0003"));
		dialogBase.onPushButton1 = delegate
		{
			decideClassFunc(SealedController.GoToSealedDeckEdit);
		};
		dialogBase.onPushButton3 = delegate
		{
			decideClassFunc(SealedController.GoToSealedCardPackOpen);
		};
		dialogBase.ClickSe_Btn1 = 0;
		dialogBase.ClickSe_Btn3 = 0;
		SealedClassSelectConfirmDialog component = UnityEngine.Object.Instantiate(_classSelectConfirmDialogPrefab.gameObject).GetComponent<SealedClassSelectConfirmDialog>();
		component.Init(classId, SealedData.ClassInfoList.Find((SealedClassInfo x) => x.ClassId == classId).PublishedCardInfoList.Select((SealedCardInfo x) => x.SealedCardId).ToList());
		dialogBase.SetObj(component.gameObject);
	}

	private void CardDetailDragCallback(Vector2 vec)
	{
		if (!_cardDetailDialog.IsEnableShowDetail)
		{
			return;
		}
		float x = vec.x;
		if (!(Mathf.Abs(x) < 70f))
		{
			int num = _cardDetailCardIndex + ((!(x > 0f)) ? 1 : (-1));
			if (num >= 0 && _cardDetailCardObjectList.Count > num)
			{

				_cardDetailDialog.CloseDefault(playSe: false);
				_cardDetailDialog.ShowCardDetail(_cardDetailCardObjectList[num].gameObject);
				_cardDetailCardIndex = num;
				UpdateCardDetailArrowButtonVisible();
			}
		}
	}

	private void UpdateCardDetailArrowButtonVisible()
	{
		_cardDetailDialog.LeftButtonVisible = _cardDetailCardIndex != 0;
		_cardDetailDialog.RightButtonVisible = _cardDetailCardIndex != _cardDetailCardObjectList.Count - 1;
	}
}
