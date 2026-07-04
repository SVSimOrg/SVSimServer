using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Resource;

public class CardTemplate : MonoBehaviour
{
	public GameObject CardWrapObjTemp;

	public Transform CardNormalTemp;

	public LODGroup CardNormalLodGroup;

	public MeshRenderer NormalCardBaseMeshTemp;

	public MeshRenderer EvolCardBaseMeshTemp;

	public UISprite SkillIconTemp;

	public UILabel SkillIconLabelTemp;

	public UILabel LifeLabelTemp;

	public UILabel AtkLabelTemp;

	public UILabel NormalCostLabelTemp;

	public UILabel NormalZeroCostLabelTemp;

	public UILabel NormalSignLabelTemp;

	public UILabel NormalSignedCostLabelTemp;

	public UILabel NormalLifeLabelTemp;

	public UILabel NormalAtkLabelTemp;

	public UILabel NormalNameLabelTemp;

	public UILabel NormalChoiceBraveNameLabelTemp;

	public BoxCollider Collider;

	private bool isPlayer = true;

	private bool _isChoiceBrave;

	public void DynamicSetupMaterials(BattleCardBase card, IBattleResourceMgr resourceMgr)
	{
		isPlayer = card.IsPlayer;
		if (card.IsUnit)
		{
			DynamicSetupNormalObjMaterials(card.BaseParameter, resourceMgr);
		}
		else if (card.IsSpell)
		{
			DynamicSetupSpellObjMaterials(card.BaseParameter, resourceMgr);
		}
		else if (card.IsField)
		{
			DynamicSetupFieldObjMaterials(card.BaseParameter, resourceMgr);
		}
	}

	public void DynamicSetupNormalObjMaterials(CardParameter cardParameter, IBattleResourceMgr resourceMgr)
	{
		Material CTexNormal = null;
		try
		{
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardParameter.NormalCardId);
			string materialPath = Toolbox.ResourcesManager.GetAssetTypePath(cardParameterFromId.ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial);
			Toolbox.ResourcesManager.StartCoroutine_LoadAssetGroupAsync(materialPath, delegate
			{
				Toolbox.ResourcesManager.BattleListAssetPathList.Add(materialPath);
				if (!(CardWrapObjTemp == null))
				{
					CTexNormal = Toolbox.ResourcesManager.FindCardMaterial(cardParameter.ResourceCardId, ResourcesManager.AssetLoadPathType.UnitCardMaterial);
					CardShaderDefine.ReplaceShader(CTexNormal);
					UnitCardCreator.SetupUnitCardMaterialToCardMesh(CardWrapObjTemp.transform, cardParameter, CTexNormal);
				}
			});
			NormalCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
			EvolCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
			AtkLabelTemp.text = cardParameter.Atk.ToString();
			NormalAtkLabelTemp.text = cardParameter.Atk.ToString();
			LifeLabelTemp.text = cardParameter.Life.ToString();
			NormalLifeLabelTemp.text = cardParameter.Life.ToString();
			NormalCostLabelTemp.text = cardParameter.Cost.ToString();
			NormalNameLabelTemp.text = cardParameter.CardName;
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(NormalCostLabelTemp, cardParameter.IsFoil);
			UIManager.GetInstance().getUIBase_CardManager().SetNameLabelStyle(NormalNameLabelTemp, cardParameter.IsFoil);
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(NormalAtkLabelTemp, cardParameter.IsFoil);
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(NormalLifeLabelTemp, cardParameter.IsFoil);
			Global.SetRepositionNameLabel(NormalNameLabelTemp, cardParameter.CardName, is2D: false);
		}
		catch
		{
			CTexNormal = null;
		}
	}

	public void DynamicSetupSpellObjMaterials(CardParameter cardParameter, IBattleResourceMgr resourceMgr)
	{
		CardMaster cardMaster = CardMaster.GetInstanceForBattle();
		Material[] MaterialArrayNormal = new Material[3];
		Material CTexNormal = null;
		try
		{
			string materialPath = Toolbox.ResourcesManager.GetAssetTypePath(cardMaster.GetCardParameterFromId(cardParameter.NormalCardId).ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.SpellCardMaterial);
			Toolbox.ResourcesManager.StartCoroutine_LoadAssetGroupAsync(materialPath, delegate
			{
				Toolbox.ResourcesManager.BattleListAssetPathList.Add(materialPath);
				if (!(CardWrapObjTemp == null))
				{
					bool flag2 = CardMaster.IsChoiceBraveCardCheck(cardParameter.BaseCardId);
					CTexNormal = Toolbox.ResourcesManager.FindCardMaterial(cardParameter.ResourceCardId, ResourcesManager.AssetLoadPathType.SpellCardMaterial, isEvol: false, CardMaster.IsMutationCardCheck(cardParameter.BaseCardId), cardMaster.GetCardParameterFromId((cardParameter.ResourceCardId / 1000000000 == 1) ? (cardParameter.ResourceCardId / 10) : cardParameter.ResourceCardId).CharType, flag2);
					CardShaderDefine.ReplaceShader(CTexNormal);
					UIManager.GetInstance().SetLayerRecursive(CardNormalTemp, 10);
					if (flag2)
					{
						/* Pre-Phase-5b: LoadChoiceBraveCardMesh dropped; headless has no BattleResourceMgr wired here */
						MeshFilter[] componentsInChildren = CardNormalTemp.GetComponentsInChildren<MeshFilter>();
						componentsInChildren[0].sharedMesh = resourceMgr.GetChoiceBraveCardMesh(isLow: false);
						componentsInChildren[1].sharedMesh = resourceMgr.GetChoiceBraveCardMesh(isLow: true);
					}
					Material rerityMaterial = resourceMgr.GetRerityMaterial(isHand: true, isSpell: true, cardParameter.Rarity, flag2);
					if (rerityMaterial != null)
					{
						rerityMaterial.shader = Shader.Find(rerityMaterial.shader.name);
					}
					MaterialArrayNormal[0] = rerityMaterial;
					MaterialArrayNormal[1] = CTexNormal;
					MaterialArrayNormal[2] = CardCreatorBase.GetSharedClassIconMaterial(cardParameter.Clan);
					LOD[] lODs = CardNormalLodGroup.GetLODs();
					for (int i = 0; i < lODs.Length; i++)
					{
						lODs[i].renderers[0].sharedMaterials = MaterialArrayNormal;
					}
				}
			});
			NormalCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
			if (CardMaster.IsChoiceBraveCardCheck(cardParameter.NormalCardId))
			{
				SetEffectColor(Global.CARD_HBP_LABEL_COST_COLOR);
			}
			bool flag = cardParameter.NormalCardId / 1000000 == 930;
			if (cardParameter.IsVariableCost)
			{
				NormalSignLabelTemp.text = "-";
				NormalSignedCostLabelTemp.text = "X";
				ShowSignedCostLabel();
				NormalChoiceBraveNameLabelTemp.text = cardParameter.CardName;
				ShowChoiceBraveNameLabel();
			}
			else if (flag)
			{
				if (cardParameter.Cost != 0)
				{
					NormalSignLabelTemp.text = ((cardParameter.Cost > 0) ? "-" : "+");
					NormalSignedCostLabelTemp.text = Mathf.Abs(cardParameter.Cost).ToString();
					ShowSignedCostLabel();
				}
				else
				{
					NormalZeroCostLabelTemp.text = "0";
					ShowZeroCostLabel();
				}
				NormalChoiceBraveNameLabelTemp.text = cardParameter.CardName;
				ShowChoiceBraveNameLabel();
			}
			else
			{
				NormalCostLabelTemp.text = cardParameter.Cost.ToString();
				NormalNameLabelTemp.text = cardParameter.CardName;
			}
			SetNumberLabelStyle(cardParameter.IsFoil);
			SetNameLabelStyle(cardParameter.IsFoil, flag);
			SetRepositionNameLabel(cardParameter.CardName, flag);
		}
		catch
		{
			CTexNormal = null;
		}
	}

	public void DynamicSetupFieldObjMaterials(CardParameter cardParameter, IBattleResourceMgr resourceMgr)
	{
		CardMaster cardMaster = CardMaster.GetInstanceForBattle();
		Material CTexNormal = null;
		try
		{
			string materialPath = Toolbox.ResourcesManager.GetAssetTypePath(CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardParameter.NormalCardId).ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.SpellCardMaterial);
			Toolbox.ResourcesManager.StartCoroutine_LoadAssetGroupAsync(materialPath, delegate
			{
				Toolbox.ResourcesManager.BattleListAssetPathList.Add(materialPath);
				if (!(CardWrapObjTemp == null))
				{
					CTexNormal = Toolbox.ResourcesManager.FindCardMaterial(cardParameter.ResourceCardId, ResourcesManager.AssetLoadPathType.SpellCardMaterial, isEvol: false, CardMaster.IsMutationCardCheck(cardParameter.BaseCardId), cardMaster.GetCardParameterFromId(cardParameter.ResourceCardId).CharType);
					CardShaderDefine.ReplaceShader(CTexNormal);
					FieldCardCreator.SetupFieldCardMaterialToCardMesh(CardWrapObjTemp.transform, cardParameter, CTexNormal);
				}
			});
			NormalCardBaseMeshTemp.sharedMaterial = resourceMgr.GetSleeveMaterial(isPlayer);
			NormalCostLabelTemp.text = cardParameter.Cost.ToString();
			NormalNameLabelTemp.text = cardParameter.CardName;
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(NormalCostLabelTemp, cardParameter.IsFoil);
			UIManager.GetInstance().getUIBase_CardManager().SetNameLabelStyle(NormalNameLabelTemp, cardParameter.IsFoil);
			Global.SetRepositionNameLabel(NormalNameLabelTemp, cardParameter.CardName, is2D: false);
		}
		catch
		{
			CTexNormal = null;
		}
	}

	public void SetEffectStyle(UILabel.Effect effectStyle)
	{
		NormalCostLabelTemp.effectStyle = effectStyle;
		NormalZeroCostLabelTemp.effectStyle = effectStyle;
		NormalSignLabelTemp.effectStyle = effectStyle;
		NormalSignedCostLabelTemp.effectStyle = effectStyle;
	}

	public void SetEffectColor(Color color)
	{
		NormalCostLabelTemp.effectColor = color;
		NormalZeroCostLabelTemp.effectColor = color;
		NormalSignLabelTemp.effectColor = color;
		NormalSignedCostLabelTemp.effectColor = color;
	}

	public void ShowZeroCostLabel()
	{
		NormalZeroCostLabelTemp.gameObject.SetActive(value: true);
		NormalCostLabelTemp.gameObject.SetActive(value: false);
		NormalSignLabelTemp.gameObject.SetActive(value: false);
		NormalSignedCostLabelTemp.gameObject.SetActive(value: false);
	}

	public void ShowSignedCostLabel()
	{
		NormalSignLabelTemp.gameObject.SetActive(value: true);
		NormalSignedCostLabelTemp.gameObject.SetActive(value: true);
		NormalCostLabelTemp.gameObject.SetActive(value: false);
		NormalZeroCostLabelTemp.gameObject.SetActive(value: false);
	}

	public void SetNumberLabelStyle(bool isFoil)
	{
		UIBase_CardManager uIBase_CardManager = UIManager.GetInstance().getUIBase_CardManager();
		uIBase_CardManager.SetNumberLabelStyle(NormalCostLabelTemp, isFoil);
		uIBase_CardManager.SetNumberLabelStyle(NormalZeroCostLabelTemp, isFoil);
		uIBase_CardManager.SetNumberLabelStyle(NormalSignLabelTemp, isFoil);
		uIBase_CardManager.SetNumberLabelStyle(NormalSignedCostLabelTemp, isFoil);
	}

	public void ShowChoiceBraveNameLabel()
	{
		NormalNameLabelTemp.gameObject.SetActive(value: false);
		NormalChoiceBraveNameLabelTemp.gameObject.SetActive(value: true);
		_isChoiceBrave = true;
	}

	public void SetNameLabelStyle(bool isFoil, bool isChoiceBrave)
	{
		UIManager.GetInstance().getUIBase_CardManager().SetNameLabelStyle(isChoiceBrave ? NormalChoiceBraveNameLabelTemp : NormalNameLabelTemp, isFoil);
	}

	public void SetRepositionNameLabel(string cardName, bool isChoiceBrave)
	{
		Global.SetRepositionNameLabel(isChoiceBrave ? NormalChoiceBraveNameLabelTemp : NormalNameLabelTemp, cardName, is2D: false);
	}
}
