using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class SBattleLoad : MonoBehaviour
{
	public class CardFrameMaterialHolder
	{
		public static Material GetHandUnitFrame(int rarity)
		{
			string text = null;
			Material material = Toolbox.ResourcesManager.LoadObject<Material>(Toolbox.ResourcesManager.GetAssetTypePath(rarity switch
			{
				0 => "CardFrame_F_Bronze", 
				1 => "CardFrame_F_Bronze", 
				2 => "CardFrame_F_Silver", 
				3 => "CardFrame_F_Gold", 
				4 => "CardFrame_F_Legend", 
				_ => "CardFrame_F_Bronze", 
			}, ResourcesManager.AssetLoadPathType.CardFrameMaterial, isfetch: true));
			material.shader = Shader.Find(material.shader.name);
			return material;
		}

		public static Material GetInPlayNormalUnitFrame(int rarity)
		{
			string text = null;
			Material material = Toolbox.ResourcesManager.LoadObject<Material>(Toolbox.ResourcesManager.GetAssetTypePath(rarity switch
			{
				2 => "CardFrame_BTL_Silver", 
				3 => "CardFrame_BTL_Gold", 
				4 => "CardFrame_BTL_Legend", 
				_ => "CardFrame_BTL_Bronze", 
			}, ResourcesManager.AssetLoadPathType.CardFrameMaterial, isfetch: true));
			material.shader = Shader.Find(material.shader.name);
			return material;
		}

		public static Material GetHandFieldFrame(int rarity)
		{
			string text = null;
			Material material = Toolbox.ResourcesManager.LoadObject<Material>(Toolbox.ResourcesManager.GetAssetTypePath(rarity switch
			{
				0 => "CardFrame_SF_Bronze", 
				1 => "CardFrame_SF_Bronze", 
				2 => "CardFrame_SF_Silver", 
				3 => "CardFrame_SF_Gold", 
				4 => "CardFrame_SF_Legend", 
				_ => "CardFrame_SF_Bronze", 
			}, ResourcesManager.AssetLoadPathType.CardFrameMaterial, isfetch: true));
			material.shader = Shader.Find(material.shader.name);
			return material;
		}
	}

	private GameMgr _gameMgr;

	private BattleManagerBase _btlMgr;

	[SerializeField]
	private GameObject m_PanelMgr;

	public CardTemplate UnitCardTemplate;

	public CardTemplate SpellCardTemplate;

	public CardTemplate FieldCardTemplate;

	public Material[] m_RarelityFrameSkillList = new Material[5];

	public Material[] _choiceBraveCardFrame = new Material[5];

	private Material _sharedMaterialNormal;

	private GameObject UnitGameObj;

	private GameObject SpellGameObj;

	private GameObject FieldGameObj;

	public GameObject LifePoolIcon;

	private GameObject AtkPoolIcon;

	private GameObject CostPoolIcon;

	private GameObject NamePoolIcon;

	private GameObject SkillDescPool;

	private GameObject CardBasePool;

	private List<GameObject> unitCardEffectObjs = new List<GameObject>();

	private List<GameObject> spellCardEffectObjs = new List<GameObject>();

	private List<GameObject> fieldCardEffectObjs = new List<GameObject>();

	public bool isDbgEnableEnemyHandView = true;

	public TurnEndButtonUI m_TurnEndBtnUI { get; private set; }

	public event Action OnEndWaitCallBack;

	public void Dispoose()
	{
		if (UnitCardTemplate != null)
		{
			UnityEngine.Object.Destroy(UnitCardTemplate.gameObject);
		}
		if (SpellCardTemplate != null)
		{
			UnityEngine.Object.Destroy(SpellCardTemplate.gameObject);
		}
		if (FieldCardTemplate != null)
		{
			UnityEngine.Object.Destroy(FieldCardTemplate.gameObject);
		}
		UnitCardTemplate = null;
		SpellCardTemplate = null;
		FieldCardTemplate = null;
		m_RarelityFrameSkillList = null;
		_choiceBraveCardFrame = null;
		_sharedMaterialNormal = null;
		UnitGameObj = null;
		SpellGameObj = null;
		FieldGameObj = null;
		LifePoolIcon = null;
		AtkPoolIcon = null;
		CostPoolIcon = null;
		NamePoolIcon = null;
		SkillDescPool = null;
		CardBasePool = null;
		unitCardEffectObjs = null;
		spellCardEffectObjs = null;
		fieldCardEffectObjs = null;
		this.OnEndWaitCallBack = null;
		m_TurnEndBtnUI = null;
	}

	private IEnumerator Loading()
	{
		TweenAlpha component = _btlMgr.Battle3DContainer.transform.Find("OverBG").GetComponent<TweenAlpha>();
		m_TurnEndBtnUI = _btlMgr.BtlUIContainer.transform.Find("TurnEndBtn").GetComponent<TurnEndButtonUI>();
		m_TurnEndBtnUI.gameObject.SetActive(value: false);
		_btlMgr.BtlUIContainer.transform.Find("PlayerChoiceBraveBtn").gameObject.SetActive(value: false);
		_btlMgr.BtlUIContainer.transform.Find("EnemyChoiceBraveBtn").gameObject.SetActive(value: false);
		_btlMgr.ChangeCameraFieldOfView(_gameMgr.ScreenAspect);
		component.PlayReverse();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_PanelMgr);
		gameObject.transform.parent = _gameMgr.m_GameManagerObj.transform;
		_btlMgr.PanelMgr = gameObject.GetComponent<PanelMgr>();
		BattlePlayer battlePlayer = _btlMgr.BattlePlayer;
		BattleEnemy battleEnemy = _btlMgr.BattleEnemy;
		yield return null;
		battlePlayer.BattleView.Setup(_gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/StatusPanelP"), _btlMgr.BtlUIContainer, _btlMgr.BtlContainer, _btlMgr.Battle3DContainer);
		battleEnemy.BattleView.Setup(_gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/StatusPanelE"), _btlMgr.BtlUIContainer, _btlMgr.BtlContainer, _btlMgr.Battle3DContainer);
		_gameMgr.GetPrefabMgr().Load("Prefab/UI/Arrow");
		_btlMgr.Arrow = _gameMgr.GetPrefabMgr().CloneObjectToParent(_gameMgr.GetPrefabMgr().Get("Prefab/UI/Arrow"), _btlMgr.Battle3DContainer);
		_btlMgr.Arrow.transform.parent = _btlMgr.BtlContainer.transform;
		_btlMgr.Arrow.transform.Find("ArrowHead").localScale = Vector3.one * 512f;
		_btlMgr.ArrowControl = _btlMgr.Arrow.GetComponent<ArrowControl>();
		_btlMgr.AttackArrowHead = _gameMgr.GetPrefabMgr().CloneObjectToParent("Prefab/UI/ArrowHead", _btlMgr.Battle3DContainer);
		_btlMgr.AttackArrowHead.transform.parent = _btlMgr.BtlContainer.transform;
		_btlMgr.EvolutionArrowHead = _gameMgr.GetPrefabMgr().CloneObjectToParent("Prefab/UI/ArrowEvo", _btlMgr.Battle3DContainer);
		_btlMgr.EvolutionArrowHead.transform.parent = _btlMgr.BtlContainer.transform;
		_btlMgr.AlertDialogue = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/AlertDialogue");
		_btlMgr.AlertDialogue.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.AlertDialogue.SetActive(value: false);
		_btlMgr.AlertDialogueLabel = _btlMgr.AlertDialogue.transform.Find("Panel/DialogueLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailPanel = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/DetailPanel");
		_btlMgr.DetailMgr.DetailPanel.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.DetailMgr.SubDetailPanel = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/DetailPanel");
		_btlMgr.DetailMgr.SubDetailPanel.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.DetailMgr.SubDetailPanelControl = _btlMgr.DetailMgr.SubDetailPanel.GetComponent<DetailPanelControl>();
		_btlMgr.DetailMgr.SubDetailPanelControl.CreateNextPanel();
		_btlMgr.DetailMgr.DetailPanelControl = _btlMgr.DetailMgr.DetailPanel.GetComponent<DetailPanelControl>();
		_btlMgr.DetailMgr.DetailPanelControl.CreateNextPanel();
		_btlMgr.PSideLog = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/SideLogP");
		_btlMgr.PSideLog.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.PSideLogControl = _btlMgr.PSideLog.GetComponent<SideLogControl>();
		_btlMgr.ESideLog = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/SideLogE");
		_btlMgr.ESideLog.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.ESideLogControl = _btlMgr.ESideLog.GetComponent<SideLogControl>();
		_btlMgr.ESelectSkillSideLog = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/SideLogESelectSkill");
		_btlMgr.ESelectSkillSideLog.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.ESelectSkillSideLogControl = _btlMgr.ESelectSkillSideLog.GetComponent<SideLogControl>();
		_btlMgr.ESelectSkillSideLog.SetActive(value: false);
		yield return null;
		_btlMgr.SubUI = _gameMgr.GetGameObjMgr().AddUIManagerRootChildPrefab("Prefab/Container/BattleSubUI").transform;
		_btlMgr.SubUIOverLayBG = _btlMgr.SubUI.Find("BattleMenuOverLay").GetComponent<TweenAlpha>();
		_btlMgr.SetBattleMenuBtn();
		GameObject gameObject2 = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/TurnPanel");
		gameObject2.transform.parent = _btlMgr.BtlUIContainer.transform;
		gameObject2.layer = LayerMask.NameToLayer("Loading");
		_btlMgr.TurnPanelControl = gameObject2.GetComponent<TurnPanelControl>();
		DataMgr dataMgr = _gameMgr.GetDataMgr();
		if (dataMgr.IsQuestBattleType())
		{
			_btlMgr.BattleResult = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/BattleResult/QuestSpecialBattleResultUI");
		}
		else if (dataMgr.m_BattleType == DataMgr.BattleType.RankBattle)
		{
			_btlMgr.BattleResult = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/BattleResult/RankMatchBattleResultUI");
		}
		else if (dataMgr.m_BattleType == DataMgr.BattleType.ColosseumNormal || dataMgr.m_BattleType == DataMgr.BattleType.ColosseumTwoPick || dataMgr.m_BattleType == DataMgr.BattleType.ColosseumWindFall || dataMgr.m_BattleType == DataMgr.BattleType.ColosseumHof)
		{
			_btlMgr.BattleResult = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/BattleResult/ColosseumBattleResultUI");
		}
		else
		{
			_btlMgr.BattleResult = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/BattleResult/BattleResultUI");
		}
		_btlMgr.BattleResult.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.BattleResultControl = _btlMgr.BattleResult.GetComponent<BattleResultUIController>();
		Vector3 zero = Vector3.zero;
		zero.z = -200f;
		_btlMgr.BattleResult.transform.localPosition = zero;
		_btlMgr.BattleStart = _gameMgr.GetGameObjMgr().AddUIContainerChildPrefab("Prefab/UI/BattleStart");
		_btlMgr.BattleStart.transform.parent = _btlMgr.BtlUIContainer.transform;
		_btlMgr.BattleStartControl = _btlMgr.BattleStart.GetComponent<BattleStartControl>();
		_btlMgr.BattleStartControl.SetUp(this);
		GameObject gobj = _gameMgr.GetPrefabMgr().CloneObjectToParent(UnitCardTemplate.gameObject, _btlMgr.Battle3DContainer).GetComponent<CardTemplate>()
			.CardNormalTemp.gameObject;
		GameObject gameObject3 = _gameMgr.GetPrefabMgr().CloneObjectToParent(gobj, _btlMgr.SubParticleContainer);
		gameObject3.transform.parent = _btlMgr.CutInContainer.transform;
		gameObject3.GetComponent<LODGroup>().enabled = true;
		gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		gameObject3.transform.localScale = new Vector3(50f, 50f, 8f);
		gameObject3.transform.Find("CardBase(Clone)").GetComponent<MeshRenderer>();
		_btlMgr.DetailMgr.DetailNormal = gameObject3;
		_btlMgr.DetailMgr.DetailNormalLodGroup = gameObject3.GetComponent<LODGroup>();
		_btlMgr.DetailMgr.DetailNormalBaseMesh = gameObject3.transform.Find("CardBase(Clone)").GetComponent<MeshRenderer>();
		_btlMgr.DetailMgr.DetailNormalCostLabel = gameObject3.transform.Find("Cost(Clone)").Find("CostLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailNormalAtkLabel = gameObject3.transform.Find("Atk(Clone)").Find("AtkLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailNormalLifeLabel = gameObject3.transform.Find("Life(Clone)").Find("LifeLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailNormalNameLabel = gameObject3.transform.Find("Name(Clone)").Find("NameLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailNormalBaseMesh.gameObject.tag = "DetailCard";
		UIAnchor uIAnchor = gameObject3.AddComponent<UIAnchor>();
		uIAnchor.side = UIAnchor.Side.Left;
		uIAnchor.relativeOffset = new Vector2(0.11f, 0.1f);
		MotionUtils.SetLayerAll(gameObject3, 31);
		gameObject3.SetActive(value: false);
		GameObject gobj2 = _gameMgr.GetPrefabMgr().CloneObjectToParent(SpellCardTemplate.gameObject, _btlMgr.Battle3DContainer).GetComponent<CardTemplate>()
			.CardNormalTemp.gameObject;
		GameObject gameObject4 = _gameMgr.GetPrefabMgr().CloneObjectToParent(gobj2, _btlMgr.SubParticleContainer);
		gameObject4.transform.parent = _btlMgr.CutInContainer.transform;
		gameObject4.GetComponent<LODGroup>().enabled = true;
		gameObject4.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		gameObject4.transform.localScale = new Vector3(50f, 50f, 8f);
		_btlMgr.DetailMgr.DetailSkill = gameObject4;
		_btlMgr.DetailMgr.DetailSkillLodGroup = gameObject4.GetComponent<LODGroup>();
		_btlMgr.DetailMgr.DetailSkillBaseMesh = gameObject4.transform.Find("CardBase(Clone)").GetComponent<MeshRenderer>();
		_btlMgr.DetailMgr.DetailSkillCostLabel = gameObject4.transform.Find("Cost(Clone)").Find("CostLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailSkillNameLabel = gameObject4.transform.Find("Name(Clone)").Find("NameLabel").GetComponent<UILabel>();
		gameObject4.transform.Find("CardBase(Clone)").tag = "DetailCard";
		UIAnchor uIAnchor2 = gameObject4.AddComponent<UIAnchor>();
		uIAnchor2.side = UIAnchor.Side.Left;
		uIAnchor2.relativeOffset = new Vector2(0.11f, 0.1f);
		MotionUtils.SetLayerAll(gameObject4, 31);
		gameObject4.SetActive(value: false);
		GameObject gobj3 = _gameMgr.GetPrefabMgr().CloneObjectToParent(FieldCardTemplate.gameObject, _btlMgr.Battle3DContainer).GetComponent<CardTemplate>()
			.CardNormalTemp.gameObject;
		GameObject gameObject5 = _gameMgr.GetPrefabMgr().CloneObjectToParent(gobj3, _btlMgr.SubParticleContainer);
		gameObject5.transform.parent = _btlMgr.CutInContainer.transform;
		gameObject5.GetComponent<LODGroup>().enabled = true;
		gameObject5.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		gameObject5.transform.localScale = new Vector3(50f, 50f, 8f);
		_btlMgr.DetailMgr.DetailField = gameObject5;
		_btlMgr.DetailMgr.DetailFieldLodGroup = gameObject5.GetComponent<LODGroup>();
		_btlMgr.DetailMgr.DetailFieldBaseMesh = gameObject5.transform.Find("CardBase(Clone)").GetComponent<MeshRenderer>();
		_btlMgr.DetailMgr.DetailFieldCostLabel = gameObject5.transform.Find("Cost(Clone)").Find("CostLabel").GetComponent<UILabel>();
		_btlMgr.DetailMgr.DetailFieldNameLabel = gameObject5.transform.Find("Name(Clone)").Find("NameLabel").GetComponent<UILabel>();
		gameObject5.transform.Find("CardBase(Clone)").tag = "DetailCard";
		UIAnchor uIAnchor3 = gameObject5.AddComponent<UIAnchor>();
		uIAnchor3.side = UIAnchor.Side.Left;
		uIAnchor3.relativeOffset = new Vector2(0.11f, 0.1f);
		MotionUtils.SetLayerAll(gameObject5, 31);
		gameObject5.SetActive(value: false);
	}
}
