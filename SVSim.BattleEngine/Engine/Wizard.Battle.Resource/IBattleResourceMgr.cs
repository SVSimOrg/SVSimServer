using System;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Resource;

public interface IBattleResourceMgr
{
	VfxBase LoadSleeveMaterial(long sleeveId, bool isPlayer);

	Material GetSleeveMaterial(bool isPlayer);

	VfxBase LoadAsset(string path, ResourcesManager.AssetLoadPathType pathType, Action action);

	VfxBase LoadAsset(string fullPath, Action action);

	VfxBase LoadCardImageMaterial(int cardId, bool isEvolution, bool isFollower);

	Material GetCardImageMaterial(int cardId, bool isEvolution, bool isFollower);

	VfxBase UnloadCardImageMaterial(int cardId, bool isEvolution, bool isFollower);

	VfxBase LoadEffectBattle(string objectFullPath, string objectPath, string sePath);

	void AddEffectBattleInfoDictionary(string objectPath, string sePath);

	void SetEffectBattleInfoDictionary(string objectPath, string objectFullPath);

	VfxBase DecrementEffectBattleRefCount(string objectPath);

	void UnloadEffectBattle();

	VfxBase LoadAndCreateEffectBattleInstance(string objectPath, EffectMgr.EngineType engineType, string sePath, Action<EffectBattle> loadEndCallback);

	void DestroyEffectBattleInstance(EffectBattle effectBattle);

	VfxBase LoadRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard = false);

	Material GetRerityMaterial(bool isHand, bool isSpell, int rarity, bool isChoiceBraveCard = false);

	void LoadChoiceBraveCardMesh();

	Mesh GetChoiceBraveCardMesh(bool isLow);

	VfxBase LoadShardObject(string path, bool isAddUIContainer);

	GameObject GetSharedObject(string path);

	GameObject InstantiateSharedObject(string path);

	void Dispose();
}
