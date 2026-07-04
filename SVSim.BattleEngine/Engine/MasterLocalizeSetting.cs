using System;
using System.Collections.Generic;
using Cute;

public struct MasterLocalizeSetting
{
	private bool[] _langSetting;

	private static Dictionary<string, Global.LANG_TYPE> _langTypeDict;

	public bool IsEnableInCurrentLanguage
	{
		get
		{
			int num = (int)_langTypeDict[CustomPreference.GetTextLanguage()];
			return _langSetting[num];
		}
	}

	static MasterLocalizeSetting()
	{
		_langTypeDict = new Dictionary<string, Global.LANG_TYPE>(Enum.GetNames(typeof(Global.LANG_TYPE)).Length);
		foreach (Global.LANG_TYPE value in Enum.GetValues(typeof(Global.LANG_TYPE)))
		{
			_langTypeDict.Add(value.ToString(), value);
		}
	}

	public MasterLocalizeSetting(string[] columns, ref int index)
	{
		_langSetting = new bool[9];
		for (int i = 0; i < 9; i++)
		{
			_langSetting[i] = int.Parse(columns[index++]) != 0;
		}
	}
}
