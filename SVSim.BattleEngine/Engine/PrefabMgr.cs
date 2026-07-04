using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabMgr
{
	private Dictionary<string, Object> m_PrefabData;

	private IList<GameObject> m_AddedGameObj = new List<GameObject>();

	public PrefabMgr()
	{
		m_PrefabData = new Dictionary<string, Object>();
	}

	public void Load(string str)
	{
		if (!m_PrefabData.ContainsKey(str) && !string.IsNullOrEmpty(str))
		{
			m_PrefabData.Add(str, Resources.Load(str));
		}
	}

	public void AllUnLoad()
	{
		m_PrefabData.Clear();
		Resources.UnloadUnusedAssets();
	}

	public GameObject Get(string str)
	{
		if (m_PrefabData.ContainsKey(str))
		{
			return (GameObject)m_PrefabData[str];
		}
		return null;
	}

	public Dictionary<string, Object> GetPrefabData()
	{
		return m_PrefabData;
	}

	public void DisposeAllClonedObject()
	{
		for (int i = 0; i < m_AddedGameObj.Count; i++)
		{
			Object.DestroyImmediate(m_AddedGameObj[i]);
			m_AddedGameObj[i] = null;
		}
		m_AddedGameObj.Clear();
	}

	public GameObject CloneObjectToParent(string str, GameObject parentObject)
	{
		GameObject gameObject = null; // Pre-Phase-5b: no PrefabMgr headless
		m_AddedGameObj.Add(gameObject);
		return gameObject;
	}

	public GameObject CloneObjectToParent(GameObject gobj, GameObject parentObject)
	{
		GameObject gameObject = NGUITools.AddChild(parentObject, gobj);
		m_AddedGameObj.Add(gameObject);
		return gameObject;
	}
}
