using UnityEngine;

namespace Wizard;

public class ClassInfoParts : MonoBehaviour
{
	[SerializeField]
	private UILabel _labelClassName;

	[SerializeField]
	private UILabel _labelCharaName;

	[SerializeField]
	private UISprite _spriteClassIcon;

	[SerializeField]
	private GameObject _randomIcon;

	[SerializeField]
	private UILabel _subClassSeparator;

	[SerializeField]
	private UISprite _subClassIconSprite;

	[SerializeField]
	private UILabel _subClassNameLabel;

	[SerializeField]
	private GameObject _myRotationIconRoot;

	[SerializeField]
	private UIGrid _myRotationIconGrid;

	[SerializeField]
	private GameObject _myRotationIconOriginal;

	public UILabel ClassNameLabel => _labelClassName;

	public UILabel SubClassNameLabel => _subClassNameLabel;

	public UISprite ClassIconSprite => _spriteClassIcon;

	public UISprite SubClassIconSprite => _subClassIconSprite;

	public void InitByCharaPrm(ClassCharacterMasterData charaPrm, int chaosId = -1, string className = null)
	{
		if (base.gameObject == null)
		{
			return;
		}
		if (charaPrm == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		CardBasePrm.ClanType classColorId = charaPrm.ClassColorId;
		bool flag = !charaPrm.hide_class_name;
		if (_labelClassName != null)
		{
			_labelClassName.gameObject.SetActive(value: true);
			if (chaosId != -1)
			{
				switch (DataMgr.BattleType.FreeBattle) // Pre-Phase-5b: headless has no BattleType; FreeBattle default falls through
				{
				case DataMgr.BattleType.TwoPick:
					_labelClassName.text = ChaosUtil.GetDeckName(chaosId, Data.ArenaData.TwoPickData.ChallengeData.ChaosNum);
					break;
				case DataMgr.BattleType.ColosseumTwoPick:
					_labelClassName.text = ChaosUtil.GetDeckName(chaosId, Data.ArenaData.ColosseumData.ChaosNum);
					break;
				case DataMgr.BattleType.RoomTwoPick:
				case DataMgr.BattleType.TwoPickBackdraft:
					_labelClassName.text = ChaosUtil.GetDeckName(chaosId, Data.MyPageNotifications.data.RoomRule.RoomChaosNum);
					break;
				}
			}
			else if (!string.IsNullOrEmpty(className))
			{
				_labelClassName.text = (flag ? className : string.Empty);
			}
			else
			{
				_labelClassName.text = (flag ? charaPrm._className : string.Empty);
			}
			ClassCharaPrm.SetClassLabelSetting(_labelClassName, classColorId);
		}
		if (_labelCharaName != null)
		{
			_labelCharaName.gameObject.SetActive(value: true);
			_labelCharaName.text = charaPrm.chara_name;
			ClassCharaPrm.SetClassLabelSetting(_labelCharaName, classColorId);
		}
		if (_spriteClassIcon != null)
		{
			_spriteClassIcon.gameObject.SetActive(value: true);
			_spriteClassIcon.spriteName = (flag ? ClassCharaPrm.GetIconSpriteName(charaPrm.clan) : string.Empty);
		}
		if (_randomIcon != null)
		{
			_randomIcon.SetActive(false /* Pre-Phase-5b: headless has no class prm */);
			}
			SetSubClassVisible(visible: false);
			SetMyRotationVisible(visible: false);
		}

		public void InitByCharaId(int charaId)
		{
			// Pre-Phase-5b: no chara master headless
		InitByCharaPrm(null);
		}

		public void InitBySkinId(int skinId)
		{
			// Pre-Phase-5b: no chara master headless
		InitByCharaPrm(null);
		}

		public void InitClassByDeckData(DeckData deck)
		{
			if (deck.Format == Format.MyRotation)
			{
				InitBySkinId(deck.GetSkinId());
				if (_labelClassName != null)
				{
					_labelClassName.text = deck.GetMyRotationClassName();
				}
				SetMyRotationInfo(Data.MyRotationAllInfo.Get(deck.MyRotationId), null /* Pre-Phase-5b: no chara master */);
			}
			else
			{
				SetMyRotationVisible(visible: false);
				InitBySkinId(deck.GetSkinId());
			}
		}

		public void SetCharacterNameHeight(int height)
		{
			_labelCharaName.height = height;
		}

		public void SetSubClass(CardBasePrm.ClanType subClass)
		{
			if (subClass != CardBasePrm.ClanType.ALL && subClass != CardBasePrm.ClanType.NONE)
			{
				SetSubClassVisible(visible: true);
				if (_subClassNameLabel != null)
				{
					_subClassNameLabel.text = ((int)subClass).ToString(); // Pre-Phase-5b: no clan-name lookup
					ClassCharaPrm.SetClassLabelSetting(_subClassNameLabel, subClass);
				}
				if (_subClassIconSprite != null)
				{
					_subClassIconSprite.spriteName = ClassCharaPrm.GetIconSpriteName(subClass);
				}
			}
		}

		public void SetMyRotationInfo(MyRotationInfo myRotationInfo, ClassCharacterMasterData classCharacterMasterData)
		{
			if (myRotationInfo == null)
			{
				return;
			}
			SetMyRotationVisible(visible: true);
			if (_labelClassName != null)
			{
				_labelClassName.text = DeckData.CreateMyRotationClassName(classCharacterMasterData.class_id, myRotationInfo);
				ClassCharaPrm.SetClassLabelSetting(_labelClassName, classCharacterMasterData.ClassColorId);
			}
			foreach (MyRotationInfo.MyRotationBonus ability in myRotationInfo.Abilities)
			{
				GameObject obj = NGUITools.AddChild(_myRotationIconGrid.gameObject, _myRotationIconOriginal);
				obj.GetComponent<UISprite>().spriteName = ability.IconName;
				obj.SetActive(value: true);
			}
			_myRotationIconGrid.Reposition();
		}

		private void SetSubClassVisible(bool visible)
		{
			if (_subClassSeparator != null)
			{
				_subClassSeparator.gameObject.SetActive(visible);
			}
			if (_subClassIconSprite != null)
			{
				_subClassIconSprite.gameObject.SetActive(visible);
			}
			if (_subClassNameLabel != null)
			{
				_subClassNameLabel.gameObject.SetActive(visible);
			}
		}

		private void SetMyRotationVisible(bool visible)
		{
			if (_myRotationIconRoot != null)
			{
				_myRotationIconRoot.SetActive(visible);
			}
			if (_myRotationIconOriginal != null)
			{
				_myRotationIconOriginal.SetActive(value: false);
			}
			if (visible && _myRotationIconGrid != null)
			{
				_myRotationIconGrid.transform.DestroyChildren();
			}
		}
	}
