using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;

public class SimpleCardDetail : CardDetailBase
{

	private int _cardID;

	[SerializeField]
	private UILabel _attackLabel;

	[SerializeField]
	private UILabel _lifeLabel;

	[SerializeField]
	private BoxCollider _skillCollider;

	[SerializeField]
	private UILabel _evoAttackLabel;

	[SerializeField]
	private UILabel _evoLifeLabel;

	[SerializeField]
	private BoxCollider _skillColliderEvo;

	[SerializeField]
	private CardCraftPanel _craftPanel;

	[SerializeField]
	private UIWidget _myRotationAnchor;

	[SerializeField]
	private Transform _myRotationAnchorTargetWithEvol;

	[SerializeField]
	private Transform _myRotationAnchorTargetNoEvol;

	[SerializeField]
	private GameObject _myRotationRoot;

	[SerializeField]
	private UILabel _myRotationLabel;

	[SerializeField]
	private BoxCollider _closeCollider;

	[SerializeField]
	private UITweener[] _openTween;

	private BoxCollider _craftPanelCollider;

	private DetailPanelInfo _currentPanel;

	private CardMaster.CardMasterId _cardMasterId = CardMaster.CardMasterId.Default;

	public bool IsVisible { get; set; }

	public int CardID
	{
		get
		{
			return _cardID;
		}
		private set
		{
			CardParam = CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(_cardID = value);
			if (CardParam != null)
			{
				if (CardParam.CharType == CardBasePrm.CharaType.NORMAL)
				{
					_currentPanel = _followerPanel;
					_followerPanel._root.SetActive(value: true);
					_nonFollowerPanel._root.SetActive(value: false);
				}
				else
				{
					_currentPanel = _nonFollowerPanel;
					_followerPanel._root.SetActive(value: false);
					_nonFollowerPanel._root.SetActive(value: true);
				}
			}
		}
	}

	private CardParameter CardParam { get; set; }

	public bool IsDetailPanelDragging { get; private set; }

	public event Action OnClickCloseButton;

	public event Action OnClickCreateButton;

	public event Action<int> OnClickLiquefyButton;

	private void Awake()
	{
		_currentPanel = _followerPanel;
	}

	public void ChangeDetail(int id, CardMaster.CardMasterId cardMasterId, MyRotationInfo myRotationInfo = null)
	{
		if (!IsVisible || CardID != id)
		{
			_cardMasterId = cardMasterId;
			CardID = id;
			UpdateFirstPanel();
			UpdateEvoPanel();
			UpdateCraftPanel();
			UpdateMyRotationPanel(myRotationInfo);
			Load();
		}
	}

	public void ShowDetail()
	{
		IsVisible = true;
		if ((bool)_closeCollider)
		{
			_closeCollider.gameObject.SetActive(value: true);
		}
		for (int i = 0; i < _openTween.Length; i++)
		{
			if (_openTween[i] != null)
			{
				_openTween[i].PlayForward();
			}
		}
		ActivePanelColliders(isActive: true);
		_currentPanel._root.SetActive(value: true);
	}

	public void HideDetail()
	{
		IsVisible = false;
		if ((bool)_closeCollider)
		{
			_closeCollider.gameObject.SetActive(value: false);
		}
		if (base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < _openTween.Length; i++)
			{
				if (_openTween[i] != null)
				{
					_openTween[i].PlayReverse();
				}
			}
		}
		ActivePanelColliders(isActive: false);
		_followerEvoPanel._root.SetActive(value: false);
		_currentPanel._root.SetActive(value: false);
		IsDetailPanelDragging = false;
	}

	public void ActiveCraftPanel(bool isActive)
	{
		_craftPanel.gameObject.SetActive(isActive);
		UpdateCraftPanel();
	}

	private void ActivePanelColliders(bool isActive)
	{
		_skillColliderEvo.enabled = isActive;
		_skillCollider.enabled = isActive;
		_craftPanelCollider.enabled = isActive;
	}

	private void Load()
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		_currentPanel._logImage.mainTexture = resourcesManager.LoadObject(GetIllustPath(CardID, isFetch: true)) as Texture;
		ShowDetail();
	}

	private void UpdateFirstPanel()
	{
		if (CardParam != null)
		{
			bool flag = CardParam.CharType == CardBasePrm.CharaType.NORMAL;
			_attackLabel.gameObject.SetActive(flag);
			_lifeLabel.gameObject.SetActive(flag);
			_currentPanel._nameLabel.text = CardParam.CardName;
			_currentPanel._costLabel.text = CardParam.Cost.ToString();
			_attackLabel.text = CardParam.Atk.ToString();
			_lifeLabel.text = CardParam.Life.ToString();
			if (!flag)
			{
				SetDescLabelText(_currentPanel, CardParam.ConvertedSkillDescription);
			}
			_currentPanel._classLabel.text = ((int)CardParam.Clan).ToString(); // Pre-Phase-5b: no clan-name lookup
			if (CardParam.TribeName != "ALL")
			{
				UILabel classLabel = _currentPanel._classLabel;
				classLabel.text = classLabel.text + "/" + CardParam.TribeName;
			}
			_currentPanel._classBG.spriteName = string.Format("battle_card_info_{0}", ((int)CardParam.Clan).ToString("00"));
		}
	}

	private void UpdateEvoPanel()
	{
		if (CardParam != null)
		{
			bool flag = CardParam.CharType == CardBasePrm.CharaType.NORMAL;
			_followerEvoPanel._root.gameObject.SetActive(flag);
			_evoAttackLabel.text = CardParam.EvoAtk.ToString();
			_evoLifeLabel.text = CardParam.EvoLife.ToString();
			if (flag)
			{
				SetFollowerDetailLabel(CardParam.ConvertedSkillDescription, CardParam.ConvertedEvoSkillDescription, needEvolutionOrFusionButton: false);
			}
		}
	}

	private void UpdateCraftPanel()
	{
		if (CardParam != null && !(_craftPanel == null) && _craftPanel.gameObject.activeInHierarchy)
		{
			_craftPanel.UpdateCraftPanel(CardParam);
		}
	}

	private void UpdateMyRotationPanel(MyRotationInfo myRotationInfo)
	{
		if (CardParam == null)
		{
			return;
		}
		_myRotationRoot.SetActive(myRotationInfo != null);
		if (myRotationInfo != null)
		{
			if (CardParam.CharType == CardBasePrm.CharaType.NORMAL)
			{
				_myRotationAnchor.topAnchor.target = _myRotationAnchorTargetWithEvol;
				_myRotationAnchor.bottomAnchor.target = _myRotationAnchorTargetWithEvol;
			}
			else
			{
				_myRotationAnchor.topAnchor.target = _myRotationAnchorTargetNoEvol;
				_myRotationAnchor.bottomAnchor.target = _myRotationAnchorTargetNoEvol;
			}
			_myRotationAnchor.ResetAndUpdateAnchors();
			CardParameter cardParameter = CardParam;
			if (cardParameter.IsPrizeCard || cardParameter.IsCollaboCard)
			{
				cardParameter = CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(cardParameter.BaseCardId);
			}
			string cardSetId = cardParameter.CardSetId;
			CardSetName cardSetName = Data.Master.CardSetNameMgr.Get(cardSetId);
			string text = (int.Parse(cardSetId) - 10000).ToString();
			string id = "MyRotation_ID_09";
			if (int.Parse(cardSetId) == 10000)
			{
				id = "MyRotation_ID_10";
			}
			_myRotationLabel.text = Data.SystemText.Get(id, cardSetName.LongName, cardSetName.ShortName, text);
		}
	}

	private string GetIllustPath(int id, bool isFetch)
	{
		CardMaster instance = CardMaster.GetInstance(_cardMasterId);
		CardParameter cardParameterFromId = instance.GetCardParameterFromId(id);
		string path = instance.GetCardParameterFromId(cardParameterFromId.NormalCardId).ResourceCardId + "0";
		ResourcesManager.AssetLoadPathType type = ((cardParameterFromId.CharType == CardBasePrm.CharaType.NORMAL || CardMaster.IsMutationCardCheck(cardParameterFromId.BaseCardId)) ? ResourcesManager.AssetLoadPathType.UnitHeader : ResourcesManager.AssetLoadPathType.OtherHeader);
		return Toolbox.ResourcesManager.GetAssetTypePath(path, type, isFetch);
	}
}
