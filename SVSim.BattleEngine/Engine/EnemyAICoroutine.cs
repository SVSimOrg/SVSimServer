using System.Collections;
using UnityEngine;

public class EnemyAICoroutine
{
	private static EnemyAICoroutine _instance;

	private static MonoBehaviour _coroutineObject;

	public static EnemyAICoroutine GetInstance()
	{
		if (_instance == null)
		{
			_instance = new EnemyAICoroutine();
		}
		if (_coroutineObject == null)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Game/_BattleCoroutine")) as GameObject;
			if (gameObject != null)
			{
				_coroutineObject = gameObject.GetComponent<MonoBehaviour>();
			}
		}
		return _instance;
	}

	public Coroutine StartCoroutine(IEnumerator enumerator)
	{
		return _coroutineObject.StartCoroutine(enumerator);
	}

	public void StopAllCoroutines()
	{
		_coroutineObject.StopAllCoroutines();
	}
}
