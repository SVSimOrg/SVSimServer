using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (T)Object.FindObjectOfType(typeof(T));
				if (_instance == null)
				{
					Debug.LogError(typeof(T)?.ToString() + "is nothing");
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = (T)Object.FindObjectOfType(typeof(T));
			if (_instance == null)
			{
				Debug.LogError(typeof(T)?.ToString() + "is nothing");
			}
		}
	}
}
