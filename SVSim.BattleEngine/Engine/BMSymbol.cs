using System;
using UnityEngine;

[Serializable]
public class BMSymbol
{

	private bool mIsValid;

	public void MarkAsChanged()
	{
		mIsValid = false;
	}
}
