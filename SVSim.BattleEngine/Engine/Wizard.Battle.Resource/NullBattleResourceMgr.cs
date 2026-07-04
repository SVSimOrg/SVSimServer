using System;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Resource;

public class NullBattleResourceMgr : IBattleResourceMgr
{
	private static NullBattleResourceMgr m_instance;

	public static NullBattleResourceMgr GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new NullBattleResourceMgr();
		}
		return m_instance;
	}

	private NullBattleResourceMgr()
	{
	}

	public VfxBase LoadSleeveMaterial(long sleeveId, bool isPlayer)
	{
		return NullVfx.GetInstance();
	}

	public Material GetSleeveMaterial(bool isPlayer)
	{
		return null;
	}

	public VfxBase LoadAsset(string path, ResourcesManager.AssetLoadPathType pathType, Action action)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase LoadAsset(string fullPath, Action action)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase LoadCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		return NullVfx.GetInstance();
	}

	public Material GetCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		return null;
	}

	public VfxBase UnloadCardImageMaterial(int cardId, bool isEvolution, bool isFollower)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase LoadEffectBattle(string objectFullPath, string objectPath, string sePath)
	{
		return NullVfx.GetInstance();
	}

	public void AddEffectBattleInfoDictionary(string objectPath, string sePath)
	{
	}

	public void SetEffectBattleInfoDictionary(string objectPath, string objectFullPath)
	{
	}

	public VfxBase DecrementEffectBattleRefCount(string objectPath)
	{
		return NullVfx.GetInstance();
	}

	public void UnloadEffectBattle()
	{
	}

	public VfxBase LoadAndCreateEffectBattleInstance(string objectPath, EffectMgr.EngineType engineType, string sePath, Action<EffectBattle> loadEndCallback)
	{
		return NullVfx.GetInstance();
	}

	public void DestroyEffectBattleInstance(EffectBattle effectBattle)
	{
	}

	public VfxBase LoadRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard)
	{
		return NullVfx.GetInstance();
	}

	public Material GetRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard)
	{
		return null;
	}

	public void LoadChoiceBraveCardMesh()
	{
	}

	public Mesh GetChoiceBraveCardMesh(bool isLow)
	{
		return null;
	}

	public VfxBase LoadShardObject(string path, bool isAddUIContainer)
	{
		return NullVfx.GetInstance();
	}

	public GameObject GetSharedObject(string path)
	{
		return null;
	}

	public GameObject InstantiateSharedObject(string path)
	{
		return null;
	}

	public void Dispose()
	{
	}
}
