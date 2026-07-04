using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

public class ResourceHandler : MonoBehaviour
{
	private Dictionary<string, List<Action>> _onLoad;

	private HashSet<string> _loaded;

	public ResourceHandler()
	{
		_loaded = new HashSet<string>();
		_onLoad = new Dictionary<string, List<Action>>();
	}

	public void Add(string path, Action action)
	{
		if (_loaded.Contains(path))
		{
			action();
			return;
		}
		if (_onLoad.ContainsKey(path))
		{
			_onLoad[path].Add(action);
			return;
		}
		_onLoad[path] = new List<Action>();
		_onLoad[path].Add(action);
		StartCoroutine(Toolbox.ResourcesManager.LoadAssetAsync(path, delegate
		{
			for (int i = 0; i < _onLoad[path].Count; i++)
			{
				_onLoad[path][i]();
			}
			_onLoad.Remove(path);
			_loaded.Add(path);
		}));
	}

	public void UnloadAll()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(new List<string>(_loaded));
		_loaded.Clear();
	}
}
