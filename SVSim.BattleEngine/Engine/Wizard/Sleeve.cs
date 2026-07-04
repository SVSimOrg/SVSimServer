namespace Wizard;

public class Sleeve
{
	public long sleeve_id;

	public string sleeve_name;

	public string sleeve_path;

	public int _categoryId;

	private MasterLocalizeSetting _language;

	private bool _isAcquired;

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

	public bool IsPremiumSleeve { get; private set; }

	public bool IsEnableInCurrentLanguage => _language.IsEnableInCurrentLanguage;

	public bool IsFavorite { get; private set; }

	public Sleeve(string[] columns)
	{
		int index = 0;
		sleeve_id = long.Parse(columns[index++]);
		sleeve_name = ConvSleeveName(columns[index++]);
		sleeve_path = columns[index++];
		_categoryId = int.Parse(columns[index++]);
		IsPremiumSleeve = int.Parse(columns[index++]) != 0;
		_language = new MasterLocalizeSetting(columns, ref index);
		IsAcquired = Data.Load.data.IsAcquiredSleeve(sleeve_id);
		IsFavorite = Data.Load.data.IsFavoriteSleeve(sleeve_id);
		IsNew = false;
	}

	private string ConvSleeveName(string id)
	{
		return Data.Master.GetSleeveText(id);
	}

	public void Acquired()
	{
		if (!IsAcquired)
		{
			IsAcquired = true;
			IsNew = true;
		}
	}

	public void UnsetNew()
	{
		IsNew = false;
	}

	public void SetFavorite(bool favorite)
	{
		IsFavorite = favorite;
	}
}
