using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class CardListTemplate : MonoBehaviour
{

	public static readonly Color RED_COLOR_NAME = new Color32(215, 142, 153, byte.MaxValue);

	public static readonly Color RED_COLOR = new Color32(215, 142, 153, byte.MaxValue);

	public static readonly Color RED_COLOR_EFFECT = new Color32(121, 0, 41, byte.MaxValue);

	[SerializeField]
	private GameObject _numRoot;

	[SerializeField]
	private GameObject _normalNumRoot;

	[SerializeField]
	private GameObject _favoriteNumRoot;

	[SerializeField]
	public UITexture _cardTexture;

	[SerializeField]
	public UIShaderSprite _frameSprite;

	[SerializeField]
	public UITexture _classIconTexture;

	[SerializeField]
	private UISprite _skillClassIconBossRush;

	[SerializeField]
	public UILabel _nameLabel;

	[SerializeField]
	public UILabel _atkLabel;

	[SerializeField]
	public UILabel _lifeLabel;

	[SerializeField]
	public UILabel _costLabel;

	[SerializeField]
	public UILabel _newLabel;

	[SerializeField]
	private UILabel _normalNumLabel;

	[SerializeField]
	private UILabel _favoriteNumLabel;

	[SerializeField]
	private UIShaderSprite _rotationOnlyIcon;

	[SerializeField]
	private GameObject _infoRoot;

	private int _id;

	private int _num;

	private int _spotCardNum;

	private bool _isShowingNormalNum;

	private bool _isHidingNum;

	private bool _isFaceUp;

	private static Shader _grayShader = null;

	private static Shader _redShader = null;

	private static Shader _frameShader = null;

	private static Shader _normalShader = null;

	private static Shader _premiumShader = null;

	private static Shader _graySpriteShader = null;

	private static Shader _redSpriteShader = null;

	private static Shader _spriteNormalShader = null;

	private Material _duplicatedCardMaterial;

	private UIWidget[] widgetList;

	private bool _rotationIconVisible;

	public bool IsIncludingSpotCard { get; set; }

	public bool RotationOnlyIconVisible
	{
		set
		{
			_rotationIconVisible = value;
			_rotationOnlyIcon.gameObject.SetActive(value);
		}
	}

	private void Awake()
	{
		_isShowingNormalNum = true;
		_isHidingNum = false;
		SetFace(isUp: true);
		_numRoot.SetActive(!_isHidingNum);
		_normalNumRoot.SetActive(_isShowingNormalNum);
		_favoriteNumRoot.SetActive(!_isShowingNormalNum);
		SetInfoVisible(visible: false);
		_skillClassIconBossRush.gameObject.SetActive(value: false);
		if (_spriteNormalShader == null)
		{
			_spriteNormalShader = Shader.Find("Unlit/JongTransparent Colored");
		}
	}

	public void SetId(int id)
	{
		_id = id;
	}

	public void SetNum(int num, int spotCardNum = 0)
	{
		_num = num;
		_spotCardNum = spotCardNum;
		string text = (IsIncludingSpotCard ? $"{num}[fcd24a]+{spotCardNum}" : num.ToString());
		UILabel normalNumLabel = _normalNumLabel;
		string text2 = (_favoriteNumLabel.text = text);
		normalNumLabel.text = text2;
		if (!_isHidingNum && _isFaceUp)
		{
			if (_isShowingNormalNum /* Pre-Phase-5b: no favorites headless */)
			{
				_normalNumRoot.SetActive(value: true);
				_favoriteNumRoot.SetActive(value: false);
			}
			else
			{
				_normalNumRoot.SetActive(value: false);
				_favoriteNumRoot.SetActive(value: true);
			}
		}
	}

	public int GetNum()
	{
		return _num;
	}

	public void ShowNum()
	{
		_isHidingNum = false;
		SetNum(_num, _spotCardNum);
	}

	public void HideNum()
	{
		_isHidingNum = true;
		_normalNumRoot.SetActive(value: false);
		_favoriteNumRoot.SetActive(value: false);
	}

	public void ChangeShowNumType(bool isNormal)
	{
		_isShowingNormalNum = isNormal;
		SetNum(_num, _spotCardNum);
	}

	public void SetFace(bool isUp)
	{
		_isFaceUp = isUp;
		_frameSprite.gameObject.SetActive(isUp);
		_classIconTexture.gameObject.SetActive(isUp);
		_atkLabel.gameObject.SetActive(isUp);
		_lifeLabel.gameObject.SetActive(isUp);
		_costLabel.gameObject.SetActive(isUp);
		_rotationOnlyIcon.gameObject.SetActive(isUp && _rotationIconVisible);
		if (isUp)
		{
			SetNum(_num, _spotCardNum);
			return;
		}
		_normalNumRoot.SetActive(value: false);
		_favoriteNumRoot.SetActive(value: false);
	}

	public void AttachNormalShaderRotationOnlyIcon()
	{
		_rotationOnlyIcon.SetShader(_spriteNormalShader);
	}

	public void AttachGrayShader()
	{
		if (_grayShader == null)
		{
			_grayShader = Shader.Find("Wizard/Card/GrayOut");
		}
		if (_graySpriteShader == null)
		{
			_graySpriteShader = Shader.Find("Wizard/Card/GrayOutSprite");
		}
		if (_cardTexture.material != null)
		{
			if (_duplicatedCardMaterial == null)
			{
				_duplicatedCardMaterial = new Material(_cardTexture.material);
			}
			_duplicatedCardMaterial.shader = _grayShader;
			_cardTexture.material = _duplicatedCardMaterial;
		}
		_rotationOnlyIcon.SetShader(_graySpriteShader, isSharedMaterial: false);
		_frameSprite.SetShader(_graySpriteShader);
		_classIconTexture.shader = _grayShader;
		_costLabel.effectColor = Color.gray;
		_atkLabel.effectColor = Color.gray;
		_lifeLabel.effectColor = Color.gray;
		_costLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_atkLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_lifeLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_nameLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_newLabel.gameObject.SetActive(value: false);
		_cardTexture.enabled = false;
		_cardTexture.enabled = true;
	}

	public void AttachRedMaterial()
	{
		if (_redShader == null)
		{
			_redShader = Shader.Find("Wizard/Card/RedOut");
		}
		if (_redSpriteShader == null)
		{
			_redSpriteShader = Shader.Find("Wizard/Card/RedOutSprite");
		}
		if (_cardTexture.material != null)
		{
			if (_duplicatedCardMaterial == null)
			{
				_duplicatedCardMaterial = new Material(_cardTexture.material);
			}
			_duplicatedCardMaterial.shader = _redShader;
			_cardTexture.material = _duplicatedCardMaterial;
		}
		_rotationOnlyIcon.SetShader(_redSpriteShader);
		_frameSprite.SetShader(_redSpriteShader);
		_classIconTexture.shader = _redShader;
		_costLabel.effectColor = RED_COLOR_EFFECT;
		_atkLabel.effectColor = RED_COLOR_EFFECT;
		_lifeLabel.effectColor = RED_COLOR_EFFECT;
		_costLabel.color = RED_COLOR;
		_atkLabel.color = RED_COLOR;
		_lifeLabel.color = RED_COLOR;
		_nameLabel.color = RED_COLOR_NAME;
		_cardTexture.enabled = false;
		_cardTexture.enabled = true;
	}

	public void AttachShaders(CardMaster.CardMasterId cardMasterId)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstance(cardMasterId).GetCardParameterFromId(_id);
		ResourcesManager.AssetLoadPathType type = ResourcesManager.AssetLoadPathType.UnitCardMaterial;
		if (cardParameterFromId.CharType != CardBasePrm.CharaType.NORMAL && !CardMaster.IsMutationCardCheck(cardParameterFromId.BaseCardId))
		{
			type = ResourcesManager.AssetLoadPathType.SpellCardMaterial;
		}
		if (_cardTexture.material == null)
		{
			_cardTexture.material = Toolbox.ResourcesManager.FindCardMaterial(cardParameterFromId.ResourceCardId, type);
		}
		if (_cardTexture.material != null)
		{
			if (cardParameterFromId.IsFoil)
			{
				if (_premiumShader == null)
				{
					_premiumShader = Shader.Find("Wizard/VariantCardShader");
				}
				_cardTexture.material.shader = _premiumShader;
			}
			else
			{
				if (_normalShader == null)
				{
					_normalShader = Shader.Find("Wizard/Card/Basic_Front0_Back0_Normal");
				}
				_cardTexture.material.shader = _normalShader;
			}
			_cardTexture.material.color = Color.white;
		}
		AttachNormalFrame(cardParameterFromId);
		_rotationOnlyIcon.SetShader(_spriteNormalShader);
		_cardTexture.enabled = false;
		_cardTexture.enabled = true;
	}

	public void AttachNormalFrame(CardParameter param)
	{
		if (_frameShader == null)
		{
			_frameShader = Shader.Find("Unlit/Transparent Colored");
		}
		if (_frameSprite.shader != _frameSprite.atlas?.spriteMaterial.shader)
		{
			_frameSprite.SetShader(null);
		}
		if (_classIconTexture.shader != _frameShader)
		{
			_classIconTexture.shader = _frameShader;
		}
		_classIconTexture.mainTexture = ClassCharaPrm.GetClassIconTexture((int)param.Clan);
		_costLabel.effectColor = Global.CARD_LABEL_FRAME_COST_COLOR;
		_atkLabel.effectColor = Global.CARD_LABEL_FRAME_ATTACK_COLOR;
		_lifeLabel.effectColor = Global.CARD_LABEL_FRAME_HEALTH_COLOR;
		_costLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_atkLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_lifeLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
		_nameLabel.color = Global.CARD_LABEL_FRAME_TEXT_COLOR;
	}

	public void SetInfoVisible(bool visible)
	{
		_infoRoot.SetActive(visible);
	}

	public void SetFrame(CardParameter cardParam)
	{
		string arg = string.Empty;
		if (cardParam.IsPhantomCard)
		{
			arg = "_phantom";
			_frameSprite.atlas = null;
		}
		string spriteName = string.Empty;
		int num = cardParam.Rarity - 1;
		switch (cardParam.CharType)
		{
		case CardBasePrm.CharaType.NORMAL:
			spriteName = $"frame_card_unit_{num:D2}{arg}";
			break;
		case CardBasePrm.CharaType.SPELL:
			spriteName = $"frame_card_spell_{num:D2}{arg}";
			break;
		case CardBasePrm.CharaType.FIELD:
		case CardBasePrm.CharaType.CHANT_FIELD:
			spriteName = $"frame_card_field_{num:D2}{arg}";
			break;
		}
		_frameSprite.spriteName = spriteName;
		UIManager.GetInstance().AttachAtlas(_frameSprite.gameObject);
	}

	public void SetParentAndResetPos(Transform parent)
	{
		base.transform.parent = parent;
		base.transform.localPosition = Vector3.zero;
	}

	public void SetScale(float scale)
	{
		base.transform.localScale = new Vector3(scale, scale, 1f);
	}

	public void AddDepth(int addValue)
	{
		if (widgetList == null)
		{
			widgetList = base.gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
		}
		UIWidget[] array = widgetList;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].depth += addValue;
		}
	}

	public UIEventListener AddColliderToFrame(float scale = 1f)
	{
		GameObject obj = _frameSprite.gameObject;
		obj.AddComponent<BoxCollider>().size = _frameSprite.localSize * scale;
		return UIEventListener.Get(obj);
	}
}
