using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 23 of 25 methods unrun in baseline
//   Type: Wizard.Battle.Resource.BattleResourceMgr
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Resource;

public class BattleResourceMgr : IBattleResourceMgr
{
	private class CardImageMaterialInfo
	{
		public Material material;

		public int refCount;
	}

	private class EffectBattleInfo
	{
		public GameObject assetObject;

		public string sePath;

		public int refCount;
	}

	private class WaitLoadLoalResourceVfx : VfxBase
	{
		private readonly string _path;

		private readonly Dictionary<string, GameObject> _containerDictionary;

		private readonly ResourceRequest _loadRequest;

		private readonly bool _isAddUIContainer;

		private readonly float _startTime;

		private bool _recordedLog;

		public WaitLoadLoalResourceVfx(string path, Dictionary<string, GameObject> containerDictionary, bool isAddUIContainer)
		{
			_path = path;
			_containerDictionary = containerDictionary;
			_isAddUIContainer = isAddUIContainer;
			if (_containerDictionary.ContainsKey(_path))
			{
				IsEnd = true;
				return;
			}
			_loadRequest = Resources.LoadAsync(path);
			_startTime = Time.time;
		}

		public override void Update(float dt, List<IEffectVfx> effectVfxList)
		{
			base.Update(dt, effectVfxList);
			if (_containerDictionary.ContainsKey(_path))
			{
				IsEnd = true;
				return;
			}
			if (!_recordedLog && (!_loadRequest.isDone || !IsEnd) && Time.time - _startTime > 10f)
			{
				_recordedLog = true;
				LocalLog.AccumulateTraceLog("#722663_" + _path);
			}
			if (_loadRequest.isDone && !IsEnd)
			{
				// Pre-Phase-5b: Prefab clone + AttachAtlas UI-only; mark done to advance loader
				GameObject gameObject = null;
				IsEnd = true;
			}
		}
	}

	private static readonly int RARITY_COUNT = 5;

	private readonly Dictionary<string, CardImageMaterialInfo> m_cardImageMaterialInfoDictionary = new Dictionary<string, CardImageMaterialInfo>();

	private readonly Dictionary<string, EffectBattleInfo> _effectBattleInfoDictionary = new Dictionary<string, EffectBattleInfo>();

	private readonly Dictionary<string, GameObject> m_shardObjectDictionary = new Dictionary<string, GameObject>();

	private Material _playerSleeveMaterial;

	private Material _enemySleeveMaterial;

	private readonly Material[] _handStandardCardFrameMaterials = new Material[RARITY_COUNT];

	private readonly Material[] _handSpellCardFrameMaterials = new Material[RARITY_COUNT];

	private readonly Material[] _inPlayStandardCardFrameMaterials = new Material[RARITY_COUNT];

	private readonly Material[] _choiceBraveCardFrameMaterials = new Material[RARITY_COUNT];

	private Mesh _choiceBraveCardMesh = new Mesh();

	private Mesh _choiceBraveCardLowMesh = new Mesh();

	public VfxBase LoadSleeveMaterial(long sleeveId, bool isPlayer)
	{
		return InstantVfx.Create(delegate
		{
			Material material = Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath(sleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveMaterial, isfetch: true)) as Material;
			material.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(sleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture, isfetch: true));
			material.shader = Shader.Find(material.shader.name);
			material.SetFloat("_CullMode", 2f);
			material.SetFloat("_ZWriteMode", 1f);
			if (isPlayer)
			{
				_playerSleeveMaterial = material;
			}
			else
			{
				_enemySleeveMaterial = material;
			}
		});
	}

	public Material GetSleeveMaterial(bool isPlayer)
	{
		if (!isPlayer)
		{
			return _enemySleeveMaterial;
		}
		return _playerSleeveMaterial;
	}

	public virtual VfxBase LoadAsset(string path, ResourcesManager.AssetLoadPathType pathType, Action action)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase LoadAsset(string fullPath, Action action)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase LoadCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		return InstantVfx.Create(delegate
		{
			string text = CalcCardImageMaterialPath(cardId, isEvolution, isFollower);
			if (m_cardImageMaterialInfoDictionary.ContainsKey(text))
			{
				m_cardImageMaterialInfoDictionary[text].refCount++;
			}
			else
			{
				Material material = Toolbox.ResourcesManager.LoadObject<Material>(text);
				material.shader = Shader.Find(material.shader.name);
				CardImageMaterialInfo value = new CardImageMaterialInfo
				{
					material = material,
					refCount = 1
				};
				m_cardImageMaterialInfoDictionary.Add(text, value);
			}
		});
	}

	public Material GetCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		string key = CalcCardImageMaterialPath(cardId, isEvolution, isFollower);
		if (!m_cardImageMaterialInfoDictionary.ContainsKey(key))
		{
			return null;
		}
		CardImageMaterialInfo cardImageMaterialInfo = m_cardImageMaterialInfoDictionary[key];
		cardImageMaterialInfo.refCount++;
		return cardImageMaterialInfo.material;
	}

	public VfxBase UnloadCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		string path = CalcCardImageMaterialPath(cardId, isEvolution, isFollower);
		if (!m_cardImageMaterialInfoDictionary.ContainsKey(path))
		{
			return NullVfx.GetInstance();
		}
		return InstantVfx.Create(delegate
		{
			CardImageMaterialInfo cardImageMaterialInfo = m_cardImageMaterialInfoDictionary[path];
			cardImageMaterialInfo.refCount--;
			if (cardImageMaterialInfo.refCount <= 0)
			{
				Toolbox.ResourcesManager.RemoveAsset(path);
			}
		});
	}

	private static string CalcCardImageMaterialPath(int cardId, bool isEvolution, bool isFollower)
	{
		CardMaster instanceForBattle = CardMaster.GetInstanceForBattle();
		CardParameter cardParameterFromId = instanceForBattle.GetCardParameterFromId(cardId);
		bool flag = CardMaster.IsMutationCardCheck(cardParameterFromId.BaseCardId);
		ResourcesManager.AssetLoadPathType type = (isFollower ? ResourcesManager.AssetLoadPathType.UnitCardMaterial : ResourcesManager.AssetLoadPathType.SpellCardMaterial);
		if (flag)
		{
			type = ((instanceForBattle.GetCardParameterFromId(cardParameterFromId.ResourceCardId).CharType == CardBasePrm.CharaType.NORMAL) ? ResourcesManager.AssetLoadPathType.UnitCardMaterial : ResourcesManager.AssetLoadPathType.SpellCardMaterial);
		}
		string path = ((cardParameterFromId.ResourceCardId / 1000000000 == 1) ? (cardParameterFromId.ResourceCardId + "_M") : (cardParameterFromId.ResourceCardId + ((isEvolution && !flag) ? "1_M" : "0_M")));
		return Toolbox.ResourcesManager.GetAssetTypePath(path, type, isfetch: true);
	}

	public virtual VfxBase LoadEffectBattle(string objectFullPath, string objectPath, string sePath)
	{
		if (_effectBattleInfoDictionary.ContainsKey(objectPath))
		{
			_effectBattleInfoDictionary[objectPath].refCount++;
			return NullVfx.GetInstance();
		}
		EffectBattleInfo effectBattleInfo = new EffectBattleInfo();
		effectBattleInfo.sePath = sePath;
		effectBattleInfo.refCount = 1;
		_effectBattleInfoDictionary.Add(objectPath, effectBattleInfo);
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(objectPath))
		{
			list.Add(Toolbox.ResourcesManager.GetAssetTypePath(objectPath, ResourcesManager.AssetLoadPathType.Effect2D));
		}
		if (!string.IsNullOrEmpty(sePath))
		{
			list.Add("s/" + sePath + ".acb");
		}
		return NullVfx.GetInstance();
	}

	public void AddEffectBattleInfoDictionary(string objectPath, string sePath)
	{
		if (!_effectBattleInfoDictionary.ContainsKey(objectPath))
		{
			EffectBattleInfo effectBattleInfo = new EffectBattleInfo();
			effectBattleInfo.sePath = sePath;
			effectBattleInfo.refCount = 1;
			_effectBattleInfoDictionary.Add(objectPath, effectBattleInfo);
		}
	}

	public void SetEffectBattleInfoDictionary(string objectPath, string objectFullPath)
	{
		if (_effectBattleInfoDictionary != null && _effectBattleInfoDictionary.ContainsKey(objectPath))
		{
			if (_effectBattleInfoDictionary[objectPath].assetObject == null)
			{
				_effectBattleInfoDictionary[objectPath].assetObject = Toolbox.ResourcesManager.LoadObject<GameObject>(objectFullPath);
			}
			else
			{
				_effectBattleInfoDictionary[objectPath].refCount++;
			}
		}
	}

	public VfxBase DecrementEffectBattleRefCount(string objectPath)
	{
		if (!_effectBattleInfoDictionary.ContainsKey(objectPath))
		{
			return NullVfx.GetInstance();
		}
		return InstantVfx.Create(delegate
		{
			EffectBattleInfo effectBattleInfo = _effectBattleInfoDictionary[objectPath];
			if (--effectBattleInfo.refCount < 0)
			{
				effectBattleInfo.refCount = 0;
			}
		});
	}

	public void UnloadEffectBattle()
	{
		string[] array = _effectBattleInfoDictionary.Keys.Where((string key) => _effectBattleInfoDictionary[key].refCount <= 0).ToArray();
		foreach (string text in array)
		{
			Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(text, ResourcesManager.AssetLoadPathType.Effect2D));
			_effectBattleInfoDictionary.Remove(text);
		}
	}

	public virtual VfxBase LoadAndCreateEffectBattleInstance(string objectPath, EffectMgr.EngineType engineType, string sePath, Action<EffectBattle> loadEndCallback)
	{
		if (string.IsNullOrEmpty(objectPath) || objectPath == "NONE")
		{
			loadEndCallback.Call(null);
			return NullVfx.GetInstance();
		}
		VfxBase instance = NullVfx.GetInstance();
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(objectPath, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true);
		instance = LoadEffectBattle(assetTypePath, objectPath, sePath);
		return SequentialVfxPlayer.Create(instance, InstantVfx.Create(delegate
		{
			if (!_effectBattleInfoDictionary.ContainsKey(objectPath))
			{
				loadEndCallback.Call(null);
			}
			else
			{
				EffectBattleInfo effectBattleInfo = _effectBattleInfoDictionary[objectPath];
				if (effectBattleInfo.assetObject == null)
				{
					loadEndCallback.Call(null);
				}
				else
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(effectBattleInfo.assetObject);
					EffectBattle effectBattle = gameObject.AddComponent<EffectBattle>();
					effectBattle.engineType = engineType;
					// Pre-Phase-5b: SetUIParticleShader VFX no-op — fire callback directly
					loadEndCallback.Call(effectBattle);
				}
			}
		}));
	}

	public void DestroyEffectBattleInstance(EffectBattle effectBattle)
	{
		UnityEngine.Object.Destroy(effectBattle.gameObject);
	}

	public VfxBase LoadRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard)
	{
		return InstantVfx.Create(delegate
		{
			Material material = Toolbox.ResourcesManager.LoadObject<Material>(CalcRerityMaterialPath(isHand, isSpell, rarity, isChoiceBraveCard));
			if (isSpell)
			{
				if (isChoiceBraveCard)
				{
					_choiceBraveCardFrameMaterials[rarity] = material;
				}
				else
				{
					_handSpellCardFrameMaterials[rarity] = material;
				}
			}
			else if (isHand)
			{
				_handStandardCardFrameMaterials[rarity] = material;
			}
			else
			{
				_inPlayStandardCardFrameMaterials[rarity] = material;
			}
		});
	}

	public Material GetRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard)
	{
		if (isSpell)
		{
			if (isChoiceBraveCard)
			{
				return _choiceBraveCardFrameMaterials[rarity];
			}
			return _handSpellCardFrameMaterials[rarity];
		}
		if (isHand)
		{
			return _handStandardCardFrameMaterials[rarity];
		}
		return _inPlayStandardCardFrameMaterials[rarity];
	}

	public void LoadChoiceBraveCardMesh()
	{
		_choiceBraveCardMesh = Toolbox.ResourcesManager.LoadObject<Mesh>(Toolbox.ResourcesManager.GetAssetTypePath("md_card_heroskill", ResourcesManager.AssetLoadPathType.CardFrameMesh, isfetch: true));
		_choiceBraveCardLowMesh = Toolbox.ResourcesManager.LoadObject<Mesh>(Toolbox.ResourcesManager.GetAssetTypePath("md_card_heroskill_low", ResourcesManager.AssetLoadPathType.CardFrameMesh, isfetch: true));
	}

	public Mesh GetChoiceBraveCardMesh(bool isLow)
	{
		if (isLow)
		{
			return _choiceBraveCardLowMesh;
		}
		return _choiceBraveCardMesh;
	}

	private string CalcRerityMaterialPath(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard)
	{
		string path;
		if (isSpell)
		{
			if (isChoiceBraveCard)
			{
				switch (rarity)
				{
				case 0:
					path = "CardFrame_HS_Bronze";
					break;
				case 1:
					path = "CardFrame_HS_Bronze";
					break;
				case 2:
					path = "CardFrame_HS_Silver";
					break;
				case 3:
					path = "CardFrame_HS_Gold";
					break;
				case 4:
					path = "CardFrame_HS_Legend";
					break;
				default:
					return "";
				}
			}
			else
			{
				switch (rarity)
				{
				case 0:
					path = "CardFrame_S_Bronze";
					break;
				case 1:
					path = "CardFrame_S_Bronze";
					break;
				case 2:
					path = "CardFrame_S_Silver";
					break;
				case 3:
					path = "CardFrame_S_Gold";
					break;
				case 4:
					path = "CardFrame_S_Legend";
					break;
				default:
					return "";
				}
			}
		}
		else if (isHand)
		{
			switch (rarity)
			{
			case 0:
				path = "CardFrame_SF_Bronze";
				break;
			case 1:
				path = "CardFrame_SF_Bronze";
				break;
			case 2:
				path = "CardFrame_SF_Silver";
				break;
			case 3:
				path = "CardFrame_SF_Gold";
				break;
			case 4:
				path = "CardFrame_SF_Legend";
				break;
			default:
				return "";
			}
		}
		else
		{
			switch (rarity)
			{
			case 0:
				path = "CardFrame_BTL_Bronze";
				break;
			case 1:
				path = "CardFrame_BTL_Bronze";
				break;
			case 2:
				path = "CardFrame_BTL_Silver";
				break;
			case 3:
				path = "CardFrame_BTL_Gold";
				break;
			case 4:
				path = "CardFrame_BTL_Legend";
				break;
			default:
				return "";
			}
		}
		return Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.CardFrameMaterial, isfetch: true);
	}

	public VfxBase LoadShardObject(string path, bool isAddUIContainer)
	{
		if (m_shardObjectDictionary.ContainsKey(path))
		{
			return NullVfx.GetInstance();
		}
		return new WaitLoadLoalResourceVfx(path, m_shardObjectDictionary, isAddUIContainer);
	}

	public GameObject GetSharedObject(string path)
	{
		if (!m_shardObjectDictionary.ContainsKey(path))
		{
			return null;
		}
		return m_shardObjectDictionary[path];
	}

	public GameObject InstantiateSharedObject(string path)
	{
		if (!m_shardObjectDictionary.ContainsKey(path))
		{
			return null;
		}
		// Pre-Phase-5b: InstantiateSharedObject UI-only; return null headless
		return null;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<string, EffectBattleInfo> item in _effectBattleInfoDictionary)
		{
			Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(item.Key, ResourcesManager.AssetLoadPathType.Effect2D));
		}
		_effectBattleInfoDictionary.Clear();
		foreach (KeyValuePair<string, GameObject> item2 in m_shardObjectDictionary)
		{
			Toolbox.ResourcesManager.RemoveAsset(item2.Key);
		}
		m_shardObjectDictionary.Clear();
		for (int i = 0; i < RARITY_COUNT; i++)
		{
			_handSpellCardFrameMaterials[i] = null;
			_handStandardCardFrameMaterials[i] = null;
			_inPlayStandardCardFrameMaterials[i] = null;
		}
		Resources.UnloadUnusedAssets();
	}
}
