using UnityEngine;

public class Effect : MonoBehaviour
{
	private EffectMgr.EffectType m_Type;

	private GameObject m_GameObjIns;

	private Vector3 m_Pos;

	private bool m_Use;

	private bool _buff;

	public Effect()
	{
		m_Type = EffectMgr.EffectType.NONE;
		m_GameObjIns = null;
		m_Use = false;
		m_Pos = new Vector3(0f, 0f, -3f);
		_buff = false;
	}

	public GameObject GetGameObjIns()
	{
		return m_GameObjIns;
	}
}
