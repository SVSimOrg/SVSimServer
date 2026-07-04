using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;

public class BackGroundBase
{
	protected string _bgmId;

	protected BattleCamera _battleCamera;

	protected GameObject _fieldModel;

	protected GameObject _fieldParticles;

	protected IDictionary<string, Animation> m_FieldAnimationDictionary;

	protected IDictionary<string, GameObject> _fieldObjDictionary;

	protected IDictionary<string, Animator> m_FieldAnimatorDictionary;

	protected IDictionary<string, ParticleSystem> _fieldParticleSystemDictionary;

	protected IDictionary<string, int> _gimicCntDictionary;

	protected string _str3DFieldNo;

	protected string _str3DFieldPath;

	protected BattleManagerBase m_BtlMgrIns;

	protected string m_FieldAssetPath;

	protected List<string> m_SoundAssetPathList;

	protected float m_RandomActionTime;

	protected bool IsFieldRandom;

	private Coroutine battleLoadCoroutine;

	public virtual int FieldId => 1;

	public virtual int FieldEffectId => FieldId;

	public GameObject Field { get; protected set; }

	public GameObject m_Battle3DContainer { get; protected set; }

	public GameObject m_BattleCutInContainer { get; protected set; }

	public SetShaderGlobalColorBG SetShaderGlobalColorBG { get; protected set; }

	public bool IsLoadDone { get; protected set; }

	public BackGroundBase(string bgmId = "NONE")
	{
		_battleCamera = null;
		m_Battle3DContainer = null;
		m_BattleCutInContainer = null;
		// Pre-Phase-5b: seeded m_BtlMgrIns from the ambient mgr. All Field*.cs subclass ctors
		// were stubbed to no-ops in chunk 3; nothing reads m_BtlMgrIns after that cull. Keeping
		// the field as null in headless is safe (no user of it survives the Field cull).
		m_BtlMgrIns = null;
		IsLoadDone = false;
		_str3DFieldNo = "";
		_str3DFieldPath = "";
		m_FieldAssetPath = "";
		Field = null;
		_fieldModel = null;
		_fieldParticles = null;
		_bgmId = bgmId;
		m_RandomActionTime = 0f;
		IsFieldRandom = false;
		m_SoundAssetPathList = new List<string>();
		_fieldObjDictionary = new Dictionary<string, GameObject>();
		m_FieldAnimationDictionary = new Dictionary<string, Animation>();
		m_FieldAnimatorDictionary = new Dictionary<string, Animator>();
		_fieldParticleSystemDictionary = new Dictionary<string, ParticleSystem>();
		_gimicCntDictionary = new Dictionary<string, int>();
		SetShaderGlobalColorBG = null;
		Physics.gravity = new Vector3(0f, 0f, 9.8f);
		_str3DFieldNo = GetFieldIdString(FieldId);
		_gimicCntDictionary.Add("FieldGimic1", 0);
		_gimicCntDictionary.Add("FieldGimic2", 0);
		_gimicCntDictionary.Add("FieldGimic3", 0);
	}

	public void Dispose()
	{
		UnityEngine.Object.DestroyImmediate(Field);
		Field = null;
		_fieldModel = null;
		_fieldParticles = null;
		_fieldObjDictionary.Clear();
		m_FieldAnimationDictionary.Clear();
		m_FieldAnimatorDictionary.Clear();
		_fieldParticleSystemDictionary.Clear();
		m_SoundAssetPathList.Clear();
		_gimicCntDictionary.Clear();
		SetShaderGlobalColorBG = null;
		BattleCoroutine.GetInstance().StopCoroutine(battleLoadCoroutine);
	}

	private string GetFieldIdString(int fieldId)
	{
		return fieldId.ToString((fieldId < 100) ? "00" : "0000");
	}

	public virtual void StartFieldTapEffect(int areaId, Vector3 pos)
	{
		m_BtlMgrIns.BattlePlayer.PlayerBattleView.IsTouchable();
	}

	public void StartFieldOpening()
	{
		PlayBgm();
		OpeningVfx.OpenningLogStep = "StartFieldOpening";
		IsFieldRandom = true;
		BattleCoroutine.GetInstance().StartCoroutine(RunFieldOpening());
	}

	public void PlayBgm()
	{

	}

	protected virtual IEnumerator RunFieldOpening()
	{
		yield return new WaitForSeconds(0f);
	}

	public void StartFieldGimic(GameObject obj)
	{
		if (!m_BtlMgrIns.GameMgr.IsReplayBattle && m_BtlMgrIns.BattlePlayer.PlayerBattleView.IsTouchable())
		{
			BattleCoroutine.GetInstance().StartCoroutine(RunFieldGimic(obj));
		}
	}

	protected virtual IEnumerator RunFieldGimic(GameObject obj)
	{
		yield return new WaitForSeconds(0f);
	}

	public virtual void UpdateFieldRandom()
	{
	}
}
