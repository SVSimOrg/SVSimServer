using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Card;
using Wizard.Battle.Resource;

public class CardCreatorBase
{
	private class CardTypeBuildInfo
	{
		public bool isActive;

		public Vector3 localPosition;

		public Vector3 localScale;

		public Quaternion localRotation;

		public Transform parent;
	}

	private readonly GameObject _cardRootObject;

	protected static Dictionary<CardBasePrm.ClanType, Material> _classIconCache = new Dictionary<CardBasePrm.ClanType, Material>();

	private static BattleCardBase _dummyCardInstance;

	public static Material GetSharedClassIconMaterial(CardBasePrm.ClanType clanType)
	{
		Material material;
		if (_classIconCache.ContainsKey(clanType))
		{
			material = _classIconCache[clanType];
			if (material == null || material.mainTexture == null)
			{
				if (material != null)
				{
					Object.Destroy(material);
				}
				_classIconCache.Remove(clanType);
			}
			material = null;
		}
		if (_classIconCache.ContainsKey(clanType))
		{
			material = _classIconCache[clanType];
		}
		else
		{
			material = Object.Instantiate(Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath("CardFrameClassIcon", ResourcesManager.AssetLoadPathType.CardFrameMaterialPlus, isfetch: true)) as Material);
			material.mainTexture = ClassCharaPrm.GetClassIconTexture((int)clanType);
			_classIconCache[clanType] = material;
		}
		return material;
	}

	public CardCreatorBase(GameObject cardRootObject)
	{
		_cardRootObject = cardRootObject;
	}

	public GameObject LoadRootObject()
	{
		return _cardRootObject;
	}

	public static BattleCardBase CreateCard(int cardId, bool isPlayer, int index, SBattleLoad sBattleLoad, BattleManagerBase battleMgr, IBattleResourceMgr resourceMgr, IInnerOptionsBuilder innerOptionsBuilder, bool isChoiceBrave = false)
	{
		bool flag = !isPlayer;
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
		CardBasePrm.CharaType charType = cardParameterFromId.CharType;
		CardCreatorBase cardCreatorBase = null;
		switch (charType)
		{
		case CardBasePrm.CharaType.NORMAL:
			cardCreatorBase = new UnitCardCreator(sBattleLoad.UnitCardTemplate.gameObject);
			break;
		case CardBasePrm.CharaType.FIELD:
		case CardBasePrm.CharaType.CHANT_FIELD:
			cardCreatorBase = new FieldCardCreator(sBattleLoad.FieldCardTemplate.gameObject);
			break;
		case CardBasePrm.CharaType.SPELL:
			cardCreatorBase = new SpellCardCreator(sBattleLoad.SpellCardTemplate.gameObject);
			break;
		default:
			return null;
		}
		GameObject gobj = cardCreatorBase.LoadRootObject();
		GameObject gameObject = battleMgr.GameMgr.GetPrefabMgr().CloneObjectToParent(gobj, battleMgr.Battle3DContainer);
		CardTemplate component = gameObject.GetComponent<CardTemplate>();
		if (flag && !battleMgr.GameMgr.IsWatchBattle && !sBattleLoad.isDbgEnableEnemyHandView && !battleMgr.GameMgr.IsAdmin)
		{
			gameObject.GetComponent<CardTemplate>().CardNormalLodGroup.enabled = false;
		}
		GameObject gameObject2 = component.CardNormalTemp.gameObject;
		if (charType == CardBasePrm.CharaType.NORMAL)
		{
			gameObject2.SetActive(value: false);
		}
		component.NormalCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
		if (charType == CardBasePrm.CharaType.NORMAL)
		{
			component.EvolCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
		}
		Transform transform = gameObject.transform.Find("CardObj");
		Transform transform2 = gameObject.transform.Find("Collider");
		string tag = (transform.tag = (gameObject2.tag = (flag ? "Enemy" : "Player")));
		transform2.tag = tag;
		GameObject gameObject3 = null;
		if (component.LifeLabelTemp != null)
		{
			gameObject3 = component.LifeLabelTemp.transform.parent.gameObject;
		}
		UILabel lifeLabelTemp = component.LifeLabelTemp;
		if (lifeLabelTemp != null)
		{
			lifeLabelTemp.text = cardParameterFromId.Life.ToString();
		}
		if (gameObject3 != null)
		{
			gameObject3.SetActive(value: false);
		}
		if (isChoiceBrave)
		{
			component.SetEffectColor(Global.CARD_HBP_LABEL_COST_COLOR);
		}
		if (cardParameterFromId.IsVariableCost)
		{
			component.NormalSignLabelTemp.text = "-";
			component.NormalSignedCostLabelTemp.text = "X";
			component.ShowSignedCostLabel();
			component.NormalChoiceBraveNameLabelTemp.text = cardParameterFromId.CardName;
			component.ShowChoiceBraveNameLabel();
		}
		else if (isChoiceBrave)
		{
			if (cardParameterFromId.Cost != 0)
			{
				component.NormalSignLabelTemp.text = ((cardParameterFromId.Cost > 0) ? "-" : "+");
				component.NormalSignedCostLabelTemp.text = Mathf.Abs(cardParameterFromId.Cost).ToString();
				component.ShowSignedCostLabel();
			}
			else
			{
				component.NormalZeroCostLabelTemp.text = "0";
				component.ShowZeroCostLabel();
			}
			component.NormalChoiceBraveNameLabelTemp.text = cardParameterFromId.CardName;
			component.ShowChoiceBraveNameLabel();
		}
		else
		{
			component.NormalCostLabelTemp.text = cardParameterFromId.Cost.ToString();
			component.NormalNameLabelTemp.text = cardParameterFromId.CardName;
		}
		component.SetNumberLabelStyle(cardParameterFromId.IsFoil);
		component.SetNameLabelStyle(cardParameterFromId.IsFoil, isChoiceBrave);
		component.SetRepositionNameLabel(cardParameterFromId.CardName, isChoiceBrave);
		if (charType == CardBasePrm.CharaType.NORMAL)
		{
			component.NormalLifeLabelTemp.text = lifeLabelTemp.text;
			GameObject gameObject4 = component.AtkLabelTemp.transform.parent.gameObject;
			UILabel atkLabelTemp = component.AtkLabelTemp;
			atkLabelTemp.text = cardParameterFromId.Atk.ToString();
			gameObject4.SetActive(value: false);
			component.NormalAtkLabelTemp.text = atkLabelTemp.text;
		}
		UILabel component2 = gameObject2.transform.Find("Cost(Clone)").Find("CostLabel").GetComponent<UILabel>();
		if (charType == CardBasePrm.CharaType.NORMAL)
		{
			UILabel component3 = gameObject2.transform.Find("Life(Clone)").Find("LifeLabel").GetComponent<UILabel>();
			UILabel component4 = gameObject2.transform.Find("Atk(Clone)").Find("AtkLabel").GetComponent<UILabel>();
			GameObject gameObject5 = component2.gameObject;
			GameObject gameObject6 = component3.gameObject;
			int num = (component4.gameObject.layer = 12);
			int layer = (gameObject6.layer = num);
			gameObject5.layer = layer;
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(component.NormalAtkLabelTemp, cardParameterFromId.IsFoil);
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(component.NormalLifeLabelTemp, cardParameterFromId.IsFoil);
		}
		return CreateCard(cardId, gameObject, index, isPlayer, battleMgr, component, transform, innerOptionsBuilder, isChoiceBrave);
	}

	public static BattleCardBase CreateSpecialSkillCard(int cardId, bool isPlayer, int index, SBattleLoad sBattleLoad, BattleManagerBase battleMgr, IBattleResourceMgr resourceMgr, IInnerOptionsBuilder innerOptionsBuilder, BossRushSpecialSkill skill)
	{
		bool flag = !isPlayer;
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
		_ = cardParameterFromId.CharType;
		GameObject gobj = new SpecialSkillCardCreator(sBattleLoad.SpellCardTemplate.gameObject).LoadRootObject();
		GameObject gameObject = battleMgr.GameMgr.GetPrefabMgr().CloneObjectToParent(gobj, battleMgr.Battle3DContainer);
		CardTemplate component = gameObject.GetComponent<CardTemplate>();
		if (flag && !battleMgr.GameMgr.IsWatchBattle && !sBattleLoad.isDbgEnableEnemyHandView && !battleMgr.GameMgr.IsAdmin)
		{
			gameObject.GetComponent<CardTemplate>().CardNormalLodGroup.enabled = false;
		}
		GameObject gameObject2 = component.CardNormalTemp.gameObject;
		component.NormalCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
		Transform transform = gameObject.transform.Find("CardObj");
		Transform transform2 = gameObject.transform.Find("Collider");
		string tag = (transform.tag = (gameObject2.tag = (flag ? "Enemy" : "Player")));
		transform2.tag = tag;
		GameObject gameObject3 = null;
		if (component.LifeLabelTemp != null)
		{
			gameObject3 = component.LifeLabelTemp.transform.parent.gameObject;
		}
		UILabel lifeLabelTemp = component.LifeLabelTemp;
		if (lifeLabelTemp != null)
		{
			lifeLabelTemp.text = cardParameterFromId.Life.ToString();
		}
		if (gameObject3 != null)
		{
			gameObject3.SetActive(value: false);
		}
		component.NormalCostLabelTemp.text = cardParameterFromId.Cost.ToString();
		component.NormalNameLabelTemp.text = skill.Name;
		UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(component.NormalCostLabelTemp, cardParameterFromId.IsFoil);
		UIManager.GetInstance().getUIBase_CardManager().SetNameLabelStyle(component.NormalNameLabelTemp, cardParameterFromId.IsFoil);
		Global.SetRepositionNameLabel(component.NormalNameLabelTemp, skill.Name, is2D: false);
		string tag2;
		if (isPlayer)
		{
			tag2 = "Player";
			gameObject.name = "P0";
		}
		else
		{
			tag2 = "Enemy";
			gameObject.name = "E0";
		}
		CardTypeBuildInfo cardTypeBuildInfo = CreateCardTypeBuildInfo(battleMgr, cardParameterFromId.CharType, isPlayer);
		gameObject.SetActive(cardTypeBuildInfo.isActive);
		gameObject.transform.localPosition = cardTypeBuildInfo.localPosition;
		gameObject.transform.localScale = cardTypeBuildInfo.localScale;
		gameObject.transform.localRotation = cardTypeBuildInfo.localRotation;
		gameObject.transform.parent = cardTypeBuildInfo.parent;
		gameObject.tag = tag2;
		transform.tag = tag2;
		transform.transform.Find("NormalField").tag = tag2;
		transform.transform.Find("EvolField").tag = tag2;
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(cardParameterFromId);
		BattleCardBase.BuildInfo buildInfo = new BattleCardBase.BuildInfo(gameObject, cardId, battleMgr.GetBattlePlayer(isPlayer), battleMgr.GetBattlePlayer(!isPlayer), battleMgr.GetBattlePlayer(isPlayer), cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, isPlayer, 0, innerOptionsBuilder.CreateCardOptions(), battleMgr, battleMgr.BattleResourceMgr);
		SpecialSkillBattleCard specialSkillBattleCard = new SpecialSkillBattleCard(skill, buildInfo);
		specialSkillBattleCard.Setup(createNullView: true);
		return specialSkillBattleCard;
	}

	public static BattleCardBase CreateCard(int cardId, GameObject gameObject, int battleCardIndex, bool isPlayer, BattleManagerBase battleMgr, CardTemplate CardTemplateIns, Transform parentObject, IInnerOptionsBuilder innerOptionsBuilder, bool isChoiceBrave)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
		string tag;
		if (isPlayer)
		{
			tag = "Player";
			gameObject.name = "P" + battleCardIndex;
		}
		else
		{
			tag = "Enemy";
			gameObject.name = "E" + battleCardIndex;
		}
		CardTypeBuildInfo cardTypeBuildInfo = CreateCardTypeBuildInfo(battleMgr, cardParameterFromId.CharType, isPlayer);
		gameObject.SetActive(cardTypeBuildInfo.isActive);
		gameObject.transform.localPosition = cardTypeBuildInfo.localPosition;
		gameObject.transform.localScale = cardTypeBuildInfo.localScale;
		gameObject.transform.localRotation = cardTypeBuildInfo.localRotation;
		gameObject.transform.parent = cardTypeBuildInfo.parent;
		gameObject.tag = tag;
		parentObject.tag = tag;
		if (cardParameterFromId.CharType == CardBasePrm.CharaType.NORMAL)
		{
			CardTemplateIns.CardNormalTemp.tag = tag;
			CardTemplateIns.CardNormalTemp.gameObject.SetActive(value: true);
		}
		parentObject.transform.Find("NormalField").tag = tag;
		parentObject.transform.Find("EvolField").tag = tag;
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(cardParameterFromId);
		return CreateBase(new BattleCardBase.BuildInfo(gameObject, cardId, battleMgr.GetBattlePlayer(isPlayer), battleMgr.GetBattlePlayer(!isPlayer), battleMgr.GetBattlePlayer(isPlayer), cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, isPlayer, battleCardIndex, innerOptionsBuilder.CreateCardOptions(), battleMgr, battleMgr.BattleResourceMgr), createNullView: false, isChoiceBrave);
	}

	private static BattleCardBase CreateCardWithoutResources(int cardId, int battleCardIndex, bool isPlayer, BattleManagerBase battleMgr, IInnerOptionsBuilder innerOptionsBuilder)
	{
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId));
		return CreateBase(new BattleCardBase.BuildInfo(null, cardId, battleMgr.GetBattlePlayer(isPlayer), battleMgr.GetBattlePlayer(!isPlayer), battleMgr.GetBattlePlayer(isPlayer), cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, isPlayer, battleCardIndex, innerOptionsBuilder.CreateCardOptions(), battleMgr, battleMgr.BattleResourceMgr), createNullView: true);
	}

	public static BattleCardBase CreateVirtualCard(int cardId, int index, bool isPlayer, BattleManagerBase battleMgr, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer, IInnerOptionsBuilder innerOptionsBuilder)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId);
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(cardParameterFromId);
		BattleCardBase.BuildInfo buildInfo = new BattleCardBase.BuildInfo(null, cardId, selfBattlePlayer, opponentBattlePlayer, selfBattlePlayer, cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, isPlayer, index, innerOptionsBuilder.CreateCardOptions(), battleMgr, NullBattleResourceMgr.GetInstance());
		BattleCardBase battleCardBase;
		switch (cardParameterFromId.CharType)
		{
		case CardBasePrm.CharaType.NORMAL:
		case CardBasePrm.CharaType.EVOLUTION:
			battleCardBase = new VirtualUnitBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.FIELD:
			battleCardBase = new VirtualFieldBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.CHANT_FIELD:
			battleCardBase = new VirtualChantFieldBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.SPELL:
			battleCardBase = new VirtualSpellBattleCard(buildInfo, isChoiceBrave: false);
			break;
		default:
			battleCardBase = NullBattleCard.Create();
			break;
		}
		battleCardBase.Setup();
		return battleCardBase;
	}

	public static BattleCardBase CreateToken(BattleCardBase.BuildInfo buildInfo, bool createNullView = false)
	{
		return CreateBase(buildInfo, createNullView);
	}

	private static BattleCardBase CreateBase(BattleCardBase.BuildInfo buildInfo, bool createNullView = false, bool isChoiceBrave = false)
	{
		BattleCardBase battleCardBase;
		switch (CardMaster.GetInstanceForBattle().GetCardParameterFromId(buildInfo.CardId).CharType)
		{
		case CardBasePrm.CharaType.NORMAL:
		case CardBasePrm.CharaType.EVOLUTION:
			battleCardBase = new UnitBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.FIELD:
			battleCardBase = new FieldBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.CHANT_FIELD:
			battleCardBase = new ChantFieldBattleCard(buildInfo);
			break;
		case CardBasePrm.CharaType.SPELL:
			battleCardBase = new SpellBattleCard(buildInfo, isChoiceBrave);
			battleCardBase.IsChoiceBraveSkillCard = isChoiceBrave;
			break;
		default:
			battleCardBase = NullBattleCard.Create();
			break;
		}
		battleCardBase.Setup(createNullView);
		return battleCardBase;
	}

	public static BattleCardBase CreateDummyInstance()
	{
		return NullBattleCard.Create();
	}

	public static BattleCardBase GetDummyInstance()
	{
		if (_dummyCardInstance == null)
		{
			_dummyCardInstance = NullBattleCard.Create();
		}
		return _dummyCardInstance;
	}

	private static CardTypeBuildInfo CreateCardTypeBuildInfo(BattleManagerBase ins, CardBasePrm.CharaType type, bool isPlayer)
	{
		CardTypeBuildInfo cardTypeBuildInfo = new CardTypeBuildInfo();
		if (type == CardBasePrm.CharaType.CLASS)
		{
			cardTypeBuildInfo.isActive = true;
			cardTypeBuildInfo.localPosition = (isPlayer ? new Vector3(0f, -400f, 30f) : new Vector3(0f, 420f, 30f));
			cardTypeBuildInfo.localScale = Global.CLASS_BATTLE_SCALE;
			cardTypeBuildInfo.localRotation = Quaternion.identity;
			cardTypeBuildInfo.parent = ins.Battle3DContainer.transform;
		}
		else
		{
			cardTypeBuildInfo.isActive = false;
			cardTypeBuildInfo.localPosition = (isPlayer ? ins.CardHolder.transform.localPosition : ins.ECardHolder.transform.localPosition);
			cardTypeBuildInfo.localScale = Global.CARD_BATTLE_SCALE;
			cardTypeBuildInfo.localRotation = Quaternion.Euler(0f, -90f, 90f);
			cardTypeBuildInfo.parent = ins.PCardPlace.transform;
		}
		return cardTypeBuildInfo;
	}
}
