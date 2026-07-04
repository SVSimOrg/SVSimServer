using System.Collections;
using UnityEngine;

public class BattleCoroutine
{
	private static BattleCoroutine m_instance;

	private static MonoBehaviour _coroutineObject;

	public static BattleCoroutine GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new BattleCoroutine();
		}
		if (_coroutineObject == null)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Game/_BattleCoroutine")) as GameObject;
			if (null != gameObject)
			{
				_coroutineObject = gameObject.GetComponent<MonoBehaviour>();
			}
		}
		return m_instance;
	}

	public Coroutine StartCoroutine(IEnumerator enumerator)
	{
		return _coroutineObject.StartCoroutine(enumerator);
	}

	public void StopAllCoroutines()
	{
		_coroutineObject.StopAllCoroutines();
	}

	public void StopCoroutine(IEnumerator enumerator)
	{
		if (enumerator != null)
		{
			_coroutineObject.StopCoroutine(enumerator);
		}
	}

	public void StopCoroutine(Coroutine enumerator)
	{
		if (enumerator != null)
		{
			_coroutineObject.StopCoroutine(enumerator);
		}
	}
}
