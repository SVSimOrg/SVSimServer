using System;

namespace Wizard;

public class Degree
{
	public readonly int _id;

	public readonly bool _isSpecial;

	public readonly string _name;

	public readonly string _path;

	public readonly string _achievementText;

	public readonly int _categoryId;

	private bool _isAcquired;

	private MasterLocalizeSetting _language;

	public bool IsPremium { get; }

	public bool IsEnableInCurrentLanguage => _language.IsEnableInCurrentLanguage;

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

	public Degree(string[] columns)
	{
		int index = 0;
		_id = int.Parse(columns[index++]);
		_isSpecial = Convert.ToBoolean(int.Parse(columns[index++]));
		_name = ConvDegreeName(columns[index++]);
		_path = columns[index++];
		_achievementText = GetAchievementText(columns[index++]);
		_categoryId = int.Parse(columns[index++]);
		IsPremium = Convert.ToBoolean(int.Parse(columns[index++]));
		_language = new MasterLocalizeSetting(columns, ref index);
		IsAcquired = Data.Load.data.IsAcquiredDegree(_id);
		IsFavorite = Data.Load.data.IsFavoriteDegree(_id);
		IsNew = false;
	}

	private string ConvDegreeName(string id)
	{
		return Data.Master.GetDegreeText(id);
	}

	private string GetAchievementText(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return string.Empty;
		}
		string degreeAchievementText = Data.Master.GetDegreeAchievementText(id);
		if (string.IsNullOrEmpty(degreeAchievementText))
		{
			return string.Empty;
		}
		return degreeAchievementText;
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
