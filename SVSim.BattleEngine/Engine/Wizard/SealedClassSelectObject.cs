using System.Collections;
using Cute;
using UnityEngine;

namespace Wizard;

public class SealedClassSelectObject : MonoBehaviour
{
	[SerializeField]
	private ClassInfoParts _classInfoParts;

	[SerializeField]
	private UITexture _charaTexture;

	[SerializeField]
	private UIGrid _cardObjectsGrid;

	[SerializeField]
	private UIButton _selectButton;

	[SerializeField]
	private UITexture _effectMask;

	private readonly eColorCodeId[] EFFECT_COLORCODEID_TABLE = new eColorCodeId[9]
	{
		eColorCodeId.MAX,
		eColorCodeId.SEALED_CLASS_ELF_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_ROYAL_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_WITCH_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_DRAGON_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_NECRO_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_VAMPIRE_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_BISHOP_EFFECT_COLOR,
		eColorCodeId.SEALED_CLASS_NEMESIS_EFFECT_COLOR
	};

	private GameObject _effect;

	public SealedClassSelectLoadRequest Init(SealedClassSelectObjectInitParam initParam)
	{
		_classInfoParts.InitByCharaPrm(initParam.CharaParam);
		foreach (CardListTemplate cardObject in initParam.CardObjectList)
		{
			cardObject.SetParentAndResetPos(_cardObjectsGrid.transform);
		}
		_cardObjectsGrid.Reposition();
		UIEventListener.Get(_selectButton.gameObject).onClick = delegate
		{
			initParam.SelectButtonClickCallback();
		};
		int class_id = initParam.CharaParam.class_id;
		CreateEffect(initParam, class_id);
		InitEffectMask(class_id);
		return CreateLoadRequest(initParam);
	}

	private SealedClassSelectLoadRequest CreateLoadRequest(SealedClassSelectObjectInitParam initParam)
	{
		SealedClassSelectLoadRequest sealedClassSelectLoadRequest = new SealedClassSelectLoadRequest();
		ResourcesManager resMgr = Toolbox.ResourcesManager;
		string strSkinId = initParam.CharaParam.skin_id.ToString();
		sealedClassSelectLoadRequest.LoadAssetList.Add(resMgr.GetAssetTypePath(strSkinId, ResourcesManager.AssetLoadPathType.ClassCharaBase));
		sealedClassSelectLoadRequest.LoadEndCallback = delegate
		{
			_charaTexture.mainTexture = resMgr.LoadObject<Texture>(resMgr.GetAssetTypePath(strSkinId, ResourcesManager.AssetLoadPathType.ClassCharaBase, isfetch: true));
			_effect.transform.position = base.transform.position;
		};
		return sealedClassSelectLoadRequest;
	}

	private void CreateEffect(SealedClassSelectObjectInitParam initParam, int stencil)
	{
		StartCoroutine(CreateEffectCoroutine(initParam, stencil));
	}

	private IEnumerator CreateEffectCoroutine(SealedClassSelectObjectInitParam initParam, int stencil)
	{
		bool isEndMaterialSet = false;
		_effect = EffectUtility.CreateEffect2D(new Effect2dCreateParam
		{
			Parent = initParam.EffectRoot,
			EffectName = "cmn_sealed_class_1",
			ColorCode = EFFECT_COLORCODEID_TABLE[initParam.CharaParam.class_id],
			InitActive = true,
			MaterialSetEndCallback = delegate
			{
				isEndMaterialSet = true;
			},
			UnloadAssetList = initParam.UnloadAssetList
		});
		while (!isEndMaterialSet)
		{
			yield return null;
		}
		ParticleSystemRenderer[] componentsInChildren = _effect.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer obj in componentsInChildren)
		{
			obj.sortingOrder = 2;
			obj.material = new Material(obj.material);
		}
		/* Pre-Phase-5b: ChangeMaskShader dropped */
	}

	private void InitEffectMask(int stencil)
	{
		_effectMask.material = MaterialDefine.CreateNguiStencilMaskMaterial(stencil);
	}
}
