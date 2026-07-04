using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Wizard.AutoTest;

public static class LitJsonExtention
{
	public static bool? ToBooleanOrNull(this JsonData jsonData, string key)
	{
		if (!jsonData.HasKey(key))
		{
			return null;
		}
		if (!jsonData[key].IsBoolean)
		{
			return null;
		}
		return jsonData[key].ToBoolean();
	}

	public static bool ToBooleanOrDefault(this JsonData jsonData, string key, bool defaultBoolean)
	{
		bool? flag = jsonData.ToBooleanOrNull(key);
		if (!flag.HasValue)
		{
			return defaultBoolean;
		}
		return flag.Value;
	}

	public static int? ToIntOrNull(this JsonData jsonData, string key)
	{
		if (!jsonData.HasKey(key))
		{
			return null;
		}
		if (!jsonData[key].IsInt)
		{
			return null;
		}
		return jsonData[key].ToInt();
	}

	public static int ToIntOrDefault(this JsonData jsonData, string key, int defaultInt)
	{
		int? num = jsonData.ToIntOrNull(key);
		if (!num.HasValue)
		{
			return defaultInt;
		}
		return num.Value;
	}

	public static long? ToLongOrNull(this JsonData jsonData, string key)
	{
		if (!jsonData.HasKey(key))
		{
			return null;
		}
		if (!jsonData[key].IsLong && !jsonData[key].IsInt)
		{
			return null;
		}
		return jsonData[key].ToLong();
	}

	public static long ToLongOrDefault(this JsonData jsonData, string key, int defaultLong)
	{
		long? num = jsonData.ToLongOrNull(key);
		if (!num.HasValue)
		{
			return defaultLong;
		}
		return num.Value;
	}

	public static string ToStringOrNull(this JsonData jsonData, string key)
	{
		if (!jsonData.HasKey(key))
		{
			return null;
		}
		if (!jsonData[key].IsString)
		{
			return null;
		}
		return jsonData[key].ToString();
	}

	public static string ToStringOrDefault(this JsonData jsonData, string key, string defaultString)
	{
		return jsonData.ToStringOrNull(key) ?? defaultString;
	}

	public static JsonData ToObjectOrNull(this JsonData jsonData, string key)
	{
		if (!jsonData.HasKey(key))
		{
			return null;
		}
		if (!jsonData[key].IsObject)
		{
			return null;
		}
		return jsonData[key];
	}

	public static IEnumerable<JsonData> ToJsonDataCollection(this JsonData jsonData, string key)
	{
		if (jsonData.HasKey(key) && jsonData[key].IsArray)
		{
			JsonData arrayJsonData = jsonData[key];
			for (int i = 0; i < arrayJsonData.Count; i++)
			{
				yield return arrayJsonData[i];
			}
		}
	}

	public static bool HasKey(this JsonData jsonData, string key)
	{
		return jsonData.Keys.Any((string k) => k == key);
	}
}
