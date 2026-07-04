using LitJson;

public static class JsonDataExtension
{
	public static bool TryGetValue(this JsonData data, string key, out JsonData value)
	{
		value = null;
		if (!data.IsObject)
		{
			return false;
		}
		if (!data.Keys.Contains(key))
		{
			return false;
		}
		value = data[key];
		return true;
	}

	public static string GetValueOrDefault(this JsonData data, string key, string defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value.ToString();
	}

	public static int GetValueOrDefault(this JsonData data, string key, int defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value.ToInt();
	}

	public static bool GetValueOrDefault(this JsonData data, string key, bool defaultValue)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value.ToBoolean();
	}
}
