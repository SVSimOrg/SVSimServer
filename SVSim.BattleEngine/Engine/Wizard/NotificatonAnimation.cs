using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class NotificatonAnimation : MonoBehaviour
{
	public class Param
	{
		public enum Type
		{
			GachaResult,
			TemporaryDeckResult		}

		public readonly Type _type;

		public readonly string _text;

		public Param(Type type, string text)
		{
			_type = type;
			_text = text;
		}
	}

	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private UILabel[] _labels;

	private Vector3 _rootInitialPos;

	private Vector3 _labelInitialPos;

	private void Awake()
	{
		_rootInitialPos = _root.transform.localPosition;
		_labelInitialPos = _labels[0].transform.localPosition;
		_root.SetActive(value: false);
	}
}
