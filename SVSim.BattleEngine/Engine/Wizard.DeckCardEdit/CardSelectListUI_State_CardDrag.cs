using System;
using System.Collections;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CardSelectListUI_State_CardDrag : MecanimStateBase
{
	public enum Mode
	{
		OutToIn,
		InToOut
	}

	private enum Area
	{
		InSelectionArea,
		OutSelectionArea,
		NoSelectionArea,
		SameCardArea
	}

	[SerializeField]
	private string m_closeStateName_Insert;

	[SerializeField]
	private string m_closeStateName_Remove;

	[SerializeField]
	private CardSelectListUIBase m_scene;

	[SerializeField]
	private CardSelectListUI_State_Edit _stateEdit;

	[SerializeField]
	private GameObject m_parentCloneCard;

	[SerializeField]
	private UIWidget m_parentClone_Grab;

	[SerializeField]
	private UIWidget m_parentClone_Insert;

	[SerializeField]
	private UISprite m_darkMask_Grab;

	[SerializeField]
	private UISprite m_darkMask_Insert;

	[SerializeField]
	private BoxCollider _colliderSelectionArea;

	[SerializeField]
	private BoxCollider _sameCardAddCollider;

	[SerializeField]
	private BoxCollider _colliderPagingArea;

	[SerializeField]
	private GameObject m_insertInfo;

	[SerializeField]
	private GameObject m_removeInfo;

	private CardObject m_grabCardData;

	private GameObject[] m_destroyList;

	private Area _cursorArea;

	private Area _lastDragArea;

	private bool _isFlashWhenInsert = true;

	private MyRotationInfo _myRotationInfo;

	private FilterController.MyRotationFilterType _myRotationFilterType;

	public Mode EditMode { get; set; } = Mode.InToOut;

	public bool ImmediateMove { get; set; }

	public Action<int> AddCardForSameCardSwipe { get; set; }

	public void SetMyRotationInfo(MyRotationInfo info, FilterController.MyRotationFilterType filterType)
	{
		_myRotationInfo = info;
		_myRotationFilterType = filterType;
	}

	private void Awake()
	{
		m_parentCloneCard.gameObject.layer = LayerMask.NameToLayer("Detail");
	}

	public override bool onCloseRequest(MecanimStateBase next, bool isSkip)
	{
		if (!(next == _stateEdit))
		{
			return next == this;
		}
		return true;
	}

	public override void onOpen()
	{
		base.onOpen();
		_colliderSelectionArea.gameObject.SetActive(value: true);
		_colliderPagingArea.gameObject.SetActive(value: true);
		switch (EditMode)
		{
		case Mode.OutToIn:
			m_insertInfo.SetActive(value: true);
			m_removeInfo.SetActive(value: false);
			_sameCardAddCollider.gameObject.SetActive(value: false);
			break;
		case Mode.InToOut:
			m_insertInfo.SetActive(value: false);
			m_removeInfo.SetActive(value: true);
			break;
		}

	}

	public override void onFinishCloseAnim()
	{
		base.onFinishCloseAnim();
		_colliderSelectionArea.gameObject.SetActive(value: false);
		_colliderPagingArea.gameObject.SetActive(value: false);
		_sameCardAddCollider.gameObject.SetActive(value: false);
		DestroyDragCard();
	}

	public override void onMove()
	{
		base.onMove();
		if (ImmediateMove)
		{
			if (EditMode == Mode.OutToIn)
			{
				_cursorArea = Area.InSelectionArea;
			}
			else
			{
				_cursorArea = Area.OutSelectionArea;
			}
			moveCard();
			ImmediateMove = false;
			return;
		}
		if (Input.GetMouseButton(0))
		{
			Camera camera = UIManager.GetInstance().getCamera();
			Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
			m_parentClone_Grab.transform.localPosition = camera.transform.InverseTransformPoint(position);
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(UIManager.GetInstance().getCamera().ScreenPointToRay(Input.mousePosition));
		_cursorArea = Area.NoSelectionArea;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject == _sameCardAddCollider.gameObject)
			{
				_cursorArea = Area.SameCardArea;
			}
		}
		if (_cursorArea == Area.NoSelectionArea)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].collider.gameObject == _colliderSelectionArea.gameObject)
				{
					_cursorArea = Area.InSelectionArea;
					break;
				}
				if (array[j].collider.gameObject == _colliderPagingArea.gameObject)
				{
					_cursorArea = Area.OutSelectionArea;
					break;
				}
			}
		}
		moveCard();
	}

	private void AddSelectionSameCard()
	{
		if (AddCardForSameCardSwipe == null)
		{
			AddDragCardToDeck();
		}
		else if (!m_scene.IsExistCardCardPool(m_grabCardData.CardId))
		{
			m_scene.IsAddableByBaseCardId(m_grabCardData.CardId, out var addCardId);
			if (addCardId == m_grabCardData.CardId)
			{
				AddDragCardToDeck();
			}
			else
			{
				if (addCardId == 0)
				{
					return;
				}
				m_parentClone_Insert.gameObject.SetActive(value: true);
				m_darkMask_Insert.gameObject.SetActive(value: false);
				m_darkMask_Grab.gameObject.SetActive(value: false);
				CardObject cardObject = m_scene.SelectionAreaList.FindWithCardId(addCardId);
				int num = cardObject?.MainCardNum ?? 0;
				int num2 = cardObject?.SubCardNum ?? 0;
				AddCardForSameCardSwipe(addCardId);
				DestroyDragCard();
				if (cardObject != null && (cardObject.MainCardNum != num || cardObject.SubCardNum != num2))
				{
					CreateCardAddAnimation(cardObject);
				}
				CardObject cardObject2 = m_scene.SelectionAreaList.FindWithCardId(addCardId);
				if (cardObject2 != null)
				{
					int num3 = m_scene.SelectionAreaList.IndexOf(cardObject2);
					int num4 = Mathf.Clamp(num3, 5, m_scene.SelectionAreaList.CountKind - 5);
					if (cardObject2.TotalCardNum == 1 && num3 > 0)
					{
						num4++;
					}
					_stateEdit.CenterOn(num4);
				}
			}
		}
		else
		{
			AddDragCardToDeck();
		}
	}

	private void AddDragCardToDeck()
	{
		m_parentClone_Insert.gameObject.SetActive(value: true);
		m_darkMask_Insert.gameObject.SetActive(value: false);
		m_darkMask_Grab.gameObject.SetActive(value: false);
		int num = (m_scene.IsRemainingAddableCardToSelectionArea(m_grabCardData.CardId) ? m_scene.InsertToSelectionArea(m_grabCardData) : (-1));
		_stateEdit.RefreshSelectionArea(isImmediate: false);
		_stateEdit.RefreshPage(isImmediate: false);
		bool flag = m_scene.SelectionAreaList.FindWithCardId(m_grabCardData.CardId) != null;
		if (num >= 0)
		{
			m_grabCardData = m_scene.SelectionAreaList.FindWithIndex(num);
			if (!m_grabCardData.IsNonPossessionCard)
			{
				UITexture[] componentsInChildren = m_parentClone_Insert.GetComponentsInChildren<UITexture>();
				Shader shader = Resources.Load<Shader>("Shader/Effect/Additive");
				UITexture[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].shader = shader;
				}
			}
			if (m_grabCardData != null)
			{
				m_grabCardData.CardObj.SetActive(num >= 0 && flag);
			}
			base.CloseStateName = m_closeStateName_Insert;
			_isFlashWhenInsert = true;

		}
		else
		{
			m_parentClone_Insert.gameObject.SetActive(value: false);
			base.CloseStateName = m_closeStateName_Insert;
			_isFlashWhenInsert = false;

		}
	}

	private void moveCard()
	{
		_lastDragArea = _cursorArea;
		switch (EditMode)
		{
		case Mode.OutToIn:
			switch (_cursorArea)
			{
			case Area.InSelectionArea:
				AddSelectionSameCard();
				break;
			case Area.OutSelectionArea:
				m_parentClone_Insert.gameObject.SetActive(value: true);
				m_parentClone_Insert.gameObject.transform.position = m_grabCardData.CardObj.transform.position;
				m_grabCardData.CardObj.transform.position = m_parentClone_Grab.transform.position;
				m_parentClone_Grab.alpha = 0f;
				base.CloseStateName = "";
				m_darkMask_Grab.gameObject.SetActive(value: false);

				break;
			}
			break;
		case Mode.InToOut:
			switch (_cursorArea)
			{
			case Area.SameCardArea:
				AddSelectionSameCard();
				break;
			case Area.InSelectionArea:
				m_parentClone_Insert.gameObject.SetActive(value: true);
				m_parentClone_Insert.gameObject.transform.position = m_grabCardData.CardObj.transform.position;
				m_grabCardData.CardObj.transform.position = m_parentClone_Grab.transform.position;
				m_parentClone_Grab.alpha = 0f;
				base.CloseStateName = "";
				m_darkMask_Grab.gameObject.SetActive(value: false);

				break;
			case Area.OutSelectionArea:
			{
				m_darkMask_Grab.gameObject.SetActive(value: false);
				int num = m_scene.RemoveFromSelectionArea(m_grabCardData);
				_stateEdit.Fit();
				if (num >= 0)
				{
					m_grabCardData = m_scene.PagingList.FindWithIndex(num);
					m_darkMask_Insert.gameObject.SetActive(value: true);
					m_parentClone_Insert.gameObject.SetActive(value: true);
				}
				else
				{
					m_darkMask_Insert.gameObject.SetActive(value: false);
					m_parentClone_Insert.gameObject.SetActive(value: false);
				}
				_stateEdit.RefreshSelectionArea(isImmediate: false);
				_stateEdit.RefreshPage(isImmediate: true);
				base.CloseStateName = m_closeStateName_Remove;

				break;
			}
			}
			break;
		}
		m_scene.ChangeState(_stateEdit, skipCloseAnim: false, skipOpenAnim: true);
	}

	public override void onUpdateCloseAnim()
	{
		base.onUpdateCloseAnim();
		if (m_grabCardData != null && m_grabCardData.CardObj != null)
		{
			m_parentClone_Insert.transform.position = m_grabCardData.CardObj.transform.position;
			m_darkMask_Insert.transform.position = m_grabCardData.CardObj.transform.position;
		}
		_stateEdit.onMove();
	}

	public override void onNotify(int value)
	{
		base.onNotify(value);
		if (_isFlashWhenInsert)
		{
			if (m_grabCardData != null)
			{
				m_grabCardData.CardObj.SetActive(value: true);
				m_grabCardData.ChangeSelectingState(isSelect: false);
			}
			if (_lastDragArea != Area.NoSelectionArea)
			{

			}
		}
	}

	public CardObject TryGetCard(GameObject cardObj)
	{
		if (m_scene.IsLoading)
		{
			return null;
		}
		CardObject grabCard = null;
		switch (EditMode)
		{
		case Mode.InToOut:
			grabCard = m_scene.SelectionAreaList.FindWithObject(cardObj);
			if (!grabCard.IsNonPossessionCard)
			{
				CardObject cardObject = m_scene.SelectionAreaList.CardList.Find((CardObject c) => c.IsNonPossessionCard && c.CardId == grabCard.CardId);
				if (cardObject != null)
				{
					return cardObject;
				}
			}
			return grabCard;
		case Mode.OutToIn:
		{
			grabCard = m_scene.PagingList.FindWithObject(cardObj);
			if (grabCard == null)
			{
				return null;
			}
			bool flag = false; // Pre-Phase-5b: no maintenance list headless
			if (grabCard.IsVisibleSleeve || flag)
			{
				return null;
			}
			if (!m_scene.IsRemainingAddableCardToSelectionArea(grabCard.CardId))
			{
				if (m_scene.IsEnableSwipeAutoSameBasicCardAdd() && m_scene.IsAddableByBaseCardId(grabCard.CardId, out var _))
				{
					return grabCard;
				}
				return null;
			}
			return grabCard;
		}
		default:
			return null;
		}
	}

	private bool IsEnableSameCardAddCollider()
	{
		if (_myRotationInfo != null && _myRotationFilterType != FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK && !CardMaster.GetInstance(FormatBehaviorManager.GetDefaultBehaviour(Format.MyRotation).CardMasterId).GetCardParameterFromId(m_grabCardData.CardId).IsAvailableFormat(Format.MyRotation, ClassType.None, _myRotationInfo))
		{
			return false;
		}
		if (m_scene.IsRemainingAddableCardToSelectionArea(m_grabCardData.CardId))
		{
			return true;
		}
		if (m_scene.IsEnableSwipeAutoSameBasicCardAdd() && m_scene.IsAddableByBaseCardId(m_grabCardData.CardId, out var _))
		{
			return true;
		}
		return false;
	}

	private void CreateCardAddAnimation(CardObject original)
	{
		DestroyDragCard();
		m_grabCardData = original;
		m_grabCardData.ActiveCardInfo(isActive: false);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_grabCardData.CardObj);
		CardParameter cardParameterFromId = CardMaster.GetInstance(m_scene.FormatBehavior.CardMasterId).GetCardParameterFromId(m_grabCardData.CardId);
		CardListTemplate component = gameObject.GetComponent<CardListTemplate>();
		UITexture cardTexture = component._cardTexture;
		component.HideNum();
		component.AttachNormalFrame(cardParameterFromId);
		if ((bool)cardTexture && (bool)cardTexture.material && (bool)cardTexture.material.mainTexture)
		{
			Texture mainTexture = cardTexture.material.mainTexture;
			cardTexture.material = null;
			cardTexture.mainTexture = mainTexture;
		}
		gameObject.transform.localScale = Vector3.one * 0.6f;
		m_grabCardData.ActiveCardInfo(isActive: true);
		gameObject.name = "DragCard";
		Vector3 localScale = gameObject.transform.localScale;
		gameObject.transform.parent = m_parentClone_Grab.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.identity;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		CardListTemplate component2 = gameObject2.GetComponent<CardListTemplate>();
		component2.HideNum();
		gameObject2.transform.parent = m_parentClone_Insert.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localScale = localScale;
		m_parentClone_Insert.gameObject.SetActive(value: true);
		m_parentClone_Insert.gameObject.transform.position = original.CardObj.transform.position;
		component2.RotationOnlyIconVisible = cardParameterFromId.IsResurgentCard;
		m_destroyList = new GameObject[2] { gameObject, gameObject2 };
		Camera camera = UIManager.GetInstance().getCamera();
		Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
		m_parentClone_Grab.transform.localPosition = camera.transform.InverseTransformPoint(position);
		m_parentClone_Grab.alpha = 1f;
		base.CloseStateName = m_closeStateName_Insert;
		_isFlashWhenInsert = true;
	}

	public bool CreateDragCard(CardObject original)
	{
		DestroyDragCard();
		m_grabCardData = original;
		if (original != null && EditMode == Mode.InToOut && IsEnableSameCardAddCollider())
		{
			_sameCardAddCollider.gameObject.SetActive(value: true);
			Vector3 position = _sameCardAddCollider.transform.position;
			position.x = original.CardObj.transform.position.x;
			_sameCardAddCollider.transform.position = position;
		}
		m_grabCardData.ActiveCardInfo(isActive: false);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_grabCardData.CardObj);
		gameObject.GetComponent<CardListTemplate>();
		TweenScale component = gameObject.GetComponent<TweenScale>();
		if ((bool)component)
		{
			UnityEngine.Object.Destroy(component);
		}
		CardParameter cardParameterFromId = CardMaster.GetInstance(m_scene.FormatBehavior.CardMasterId).GetCardParameterFromId(m_grabCardData.CardId);
		CardListTemplate component2 = gameObject.GetComponent<CardListTemplate>();
		UITexture cardTexture = component2._cardTexture;
		component2.HideNum();
		component2.RotationOnlyIconVisible = cardParameterFromId.IsResurgentCard;
		component2.AttachNormalShaderRotationOnlyIcon();
		if (m_scene.FormatBehavior.GetPossessionCardNum(m_grabCardData.CardId, isIncludingSpotCard: true) > 0)
		{
			component2.AttachNormalFrame(cardParameterFromId);
			if ((bool)cardTexture && (bool)cardTexture.material && (bool)cardTexture.material.mainTexture)
			{
				Texture mainTexture = cardTexture.material.mainTexture;
				cardTexture.material = null;
				cardTexture.mainTexture = mainTexture;
			}
		}
		else
		{
			component2.AttachGrayShader();
		}
		gameObject.transform.localScale = Vector3.one * 0.6f;
		m_grabCardData.ActiveCardInfo(isActive: true);
		gameObject.name = "DragCard";
		Vector3 localScale = gameObject.transform.localScale;
		gameObject.transform.parent = m_parentClone_Grab.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.identity;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		CardListTemplate component3 = gameObject2.GetComponent<CardListTemplate>();
		component3.HideNum();
		gameObject2.transform.parent = m_parentClone_Insert.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localScale = localScale;
		m_parentClone_Insert.gameObject.SetActive(value: false);
		component3.RotationOnlyIconVisible = cardParameterFromId.IsResurgentCard;
		m_destroyList = new GameObject[2] { gameObject, gameObject2 };
		m_darkMask_Grab.transform.position = m_grabCardData.CardObj.transform.position + Vector3.back * 0.01f;
		m_darkMask_Grab.transform.localScale = m_grabCardData.CardObj.transform.localScale;
		m_darkMask_Grab.gameObject.SetActive(value: true);
		m_darkMask_Insert.transform.localScale = m_grabCardData.CardObj.transform.localScale;
		m_darkMask_Insert.width = cardTexture.width;
		m_darkMask_Insert.height = cardTexture.height;
		Camera camera = UIManager.GetInstance().getCamera();
		Vector3 position2 = camera.ScreenToWorldPoint(Input.mousePosition);
		m_parentClone_Grab.transform.localPosition = camera.transform.InverseTransformPoint(position2);
		m_parentClone_Grab.alpha = 1f;
		return true;
	}

	public void DestroyDragCard()
	{
		if (m_grabCardData != null && m_grabCardData.CardObj != null)
		{
			m_grabCardData.CardObj.SetActive(value: true);
		}
		m_grabCardData = null;
		if (m_destroyList != null)
		{
			for (int i = 0; i < m_destroyList.Length; i++)
			{
				UnityEngine.Object.Destroy(m_destroyList[i]);
			}
			m_destroyList = null;
		}
		m_darkMask_Insert.gameObject.SetActive(value: false);
		m_darkMask_Grab.gameObject.SetActive(value: false);
	}

	public void OnChangeSelectionAreaFilter()
	{
		DestroyDragCard();
	}
}
