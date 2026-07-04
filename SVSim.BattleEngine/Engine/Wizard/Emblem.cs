using System;

namespace Wizard;

public class Emblem
{
	public readonly long _id;

	public readonly bool _isSpecial;

	public readonly string _name;

	public readonly string _path;

	public readonly int _categoryId;

	private bool _isAcquired;

	private MasterLocalizeSetting _language;

	public bool IsAcquired
	{
		get
		{
			if (_isAcquired)
			{
				return IsEnableInCurrentLanguage;
			}
			return false;
		}
		private set
		{
			_isAcquired = value;
		}
	}

	public bool IsNew { get; private set; }

	public bool IsFavorite { get; private set; }

	public bool IsEnableInCurrentLanguage => _language.IsEnableInCurrentLanguage;

	public Emblem(string[] columns)
	{
		int index = 0;
		_id = long.Parse(columns[index++]);
		_isSpecial = Convert.ToBoolean(int.Parse(columns[index++]));
		_name = ConvEmblemName(columns[index++]);
		_path = columns[index++];
		_categoryId = int.Parse(columns[index++]);
		_language = new MasterLocalizeSetting(columns, ref index);
		IsAcquired = Data.Load.data.IsAcquiredEmblem(_id);
		IsFavorite = Data.Load.data.IsFavoriteEmblem(_id);
		IsNew = false;
	}

	private string ConvEmblemName(string id)
	{
		return Data.Master.GetEmblemText(id);
	}

	public void Acquired()
	{
		if (!IsAcquired)
		{
			IsAcquired = true;
			IsNew = true;
		}
	}
}
