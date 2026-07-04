using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;

public class SkillOptionValue
{
	public class ReplaceDataOptionValue
	{
		public SkillFilterCreator.ContentKeyword Keyword { get; private set; }

		public int Value { get; private set; }

		public SkillBase Skill { get; private set; }

		public ReplaceDataOptionValue(SkillFilterCreator.ContentKeyword keyword, int value, SkillBase skill)
		{
			Keyword = keyword;
			Value = value;
			Skill = skill;
		}
	}

	private SkillFilterCreator.ContentInfo[] _contentInfos;

	private readonly Dictionary<string, string> _variableDictionary = new Dictionary<string, string>();

	private SkillFilterVariable _filterVariable;

	private List<ReplaceDataOptionValue> _replaceIntDataList = new List<ReplaceDataOptionValue>();

	public SkillOptionValue(SkillFilterCreator.ContentInfo[] parsedInfos)
	{
		_contentInfos = parsedInfos;
	}

	public SkillOptionValue(string text)
	{
		_contentInfos = SkillCreator.ParseContentInfos(text);
	}

	public void SetText(string text)
	{
		_contentInfos = SkillCreator.ParseContentInfos(text);
	}

	public void SetupFilterVariable(BattlePlayerReadOnlyInfoPair playerInfoPair, IReadOnlyBattleCardInfo ownerCardInfo, bool isPrePlay, SkillBase skill, SkillConditionCheckerOption checkerOption = null)
	{
		_filterVariable = new SkillFilterVariable(playerInfoPair, ownerCardInfo, isPrePlay, skill, checkerOption);
	}

	public void SetVariable(string variableName, string value)
	{
		try
		{
			_variableDictionary.Remove(variableName);
			_variableDictionary.Add(variableName, value);
		}
		catch
		{
		}
	}

	public string GetOption(SkillFilterCreator.ContentKeyword nameType, string defaultValue = null)
	{
		SkillFilterCreator.ContentInfo retInfo = default(SkillFilterCreator.ContentInfo);
		if (!GetInfoByName(nameType, out retInfo))
		{
			if (string.IsNullOrEmpty(defaultValue))
			{
				return string.Empty;
			}
			return defaultValue;
		}
		return retInfo.ValueStr;
	}

	public ValueWithOperator GetValueWithOperator(SkillFilterCreator.ContentKeyword nameType)
	{
		SkillFilterCreator.ContentInfo retInfo = default(SkillFilterCreator.ContentInfo);
		if (!GetInfoByName(nameType, out retInfo))
		{
			return null;
		}
		return new ValueWithOperator(retInfo.ValueStr, retInfo.Operator);
	}

	public int GetInt(SkillFilterCreator.ContentKeyword nameType, int? defaultValue = null, bool isRemoveReplaceData = true)
	{
		if (_replaceIntDataList.Any((ReplaceDataOptionValue x) => x.Keyword == nameType))
		{
			ReplaceDataOptionValue replaceDataOptionValue = _replaceIntDataList.FirstOrDefault((ReplaceDataOptionValue s) => s.Keyword == nameType);
			if (replaceDataOptionValue != null)
			{
				if (isRemoveReplaceData)
				{
					_replaceIntDataList.Remove(replaceDataOptionValue);
				}
				return replaceDataOptionValue.Value;
			}
			if (!defaultValue.HasValue)
			{
				return 0;
			}
			return defaultValue.Value;
		}
		string defaultValue2 = (defaultValue.HasValue ? defaultValue.Value.ToString() : null);
		string text = GetString(nameType, defaultValue2);
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		try
		{
			return ParseInt(text);
		}
		catch (Exception)
		{
			return defaultValue.HasValue ? defaultValue.Value : 0;
		}
	}

	public long GetLong(SkillFilterCreator.ContentKeyword nameType, int? defaultValue = null, bool isRemoveReplaceData = true)
	{
		if (_replaceIntDataList.Any((ReplaceDataOptionValue x) => x.Keyword == nameType))
		{
			ReplaceDataOptionValue replaceDataOptionValue = _replaceIntDataList.FirstOrDefault((ReplaceDataOptionValue s) => s.Keyword == nameType);
			if (replaceDataOptionValue != null)
			{
				if (isRemoveReplaceData)
				{
					_replaceIntDataList.Remove(replaceDataOptionValue);
				}
				return replaceDataOptionValue.Value;
			}
			return defaultValue.HasValue ? defaultValue.Value : 0;
		}
		string defaultValue2 = (defaultValue.HasValue ? defaultValue.Value.ToString() : null);
		string text = GetString(nameType, defaultValue2);
		if (string.IsNullOrEmpty(text))
		{
			return 0L;
		}
		try
		{
			return ParseLong(text);
		}
		catch (Exception)
		{
			return defaultValue.HasValue ? defaultValue.Value : 0;
		}
	}

	private bool GetInfoByName(SkillFilterCreator.ContentKeyword nameType, out SkillFilterCreator.ContentInfo retInfo)
	{
		retInfo = default(SkillFilterCreator.ContentInfo);
		int i = 0;
		for (int num = _contentInfos.Length; i < num; i++)
		{
			if (_contentInfos[i].Name == nameType)
			{
				retInfo = _contentInfos[i];
				return true;
			}
		}
		return false;
	}

	public bool HasInfoByName(SkillFilterCreator.ContentKeyword nameType)
	{
		for (int i = 0; i < _contentInfos.Length; i++)
		{
			if (_contentInfos[i].Name == nameType)
			{
				return true;
			}
		}
		return false;
	}

	public string GetString(SkillFilterCreator.ContentKeyword nameType, string defaultValue = "")
	{
		SkillFilterCreator.ContentInfo retInfo = default(SkillFilterCreator.ContentInfo);
		if (!GetInfoByName(nameType, out retInfo))
		{
			if (string.IsNullOrEmpty(defaultValue))
			{
				return string.Empty;
			}
			return defaultValue;
		}
		return GetVariableValue(retInfo.ValueStr);
	}

	public string GetStringAllParse(SkillFilterCreator.ContentKeyword nameType, char separator, string defaultValue = "")
	{
		SkillFilterCreator.ContentInfo retInfo = default(SkillFilterCreator.ContentInfo);
		if (!GetInfoByName(nameType, out retInfo))
		{
			if (string.IsNullOrEmpty(defaultValue))
			{
				return string.Empty;
			}
			return defaultValue;
		}
		string text = "";
		string[] array = retInfo.ValueStr.Split(separator);
		for (int i = 0; i < array.Length; i++)
		{
			if (i != 0)
			{
				text += separator;
			}
			text += GetVariableValue(array[i]);
		}
		return text;
	}

	private static bool IsOperator(char str)
	{
		if (str != '+' && str != '-' && str != '*' && str != '/')
		{
			return str == '%';
		}
		return true;
	}

	private static List<int> IndexOfAll(string src, string searchStr)
	{
		List<int> list = new List<int>();
		int startIndex = 0;
		for (int num = src.IndexOf(searchStr, startIndex); num != -1; num = src.IndexOf(searchStr, startIndex))
		{
			list.Add(num);
			startIndex = num + searchStr.Length;
		}
		return list;
	}

	private static int GetOperatorIndex(string expression)
	{
		List<int> list = new List<int>();
		list.AddRange(IndexOfAll(expression, "+"));
		list.AddRange(IndexOfAll(expression, "-"));
		if (list.Any((int i) => i != 0 && !IsOperator(expression[i - 1])))
		{
			return list.Where((int i) => i != 0 && !IsOperator(expression[i - 1])).ToList()[0];
		}
		int num = expression.IndexOfAny("*/%".ToCharArray());
		if (num >= 0)
		{
			return num;
		}
		return -1;
	}

	public int ParseInt(string expression)
	{
		if (int.TryParse(expression, out var result))
		{
			return result;
		}
		int num = expression.IndexOf("&");
		if (num >= 0)
		{
			return ParseIntWithOption(ParseInt(expression.Substring(0, num)), expression.Substring(num + 1));
		}
		int operatorIndex = GetOperatorIndex(expression);
		if (operatorIndex == -1 && int.TryParse(GetVariableValue(expression), out result))
		{
			return result;
		}
		string name = expression.Substring(0, operatorIndex);
		string name2 = expression.Substring(operatorIndex + 1);
		string variableValue = GetVariableValue(name);
		string variableValue2 = GetVariableValue(name2);
		char c = expression[operatorIndex];
		int num2 = ParseInt(variableValue);
		int num3 = ParseInt(variableValue2);
		return c switch
		{
			'+' => num2 + num3, 
			'-' => num2 - num3, 
			'*' => num2 * num3, 
			'/' => num2 / num3, 
			'%' => num2 % num3, 
			_ => throw new Exception(), 
		};
	}

	public int ParseIntWithOption(int value, string optionText)
	{
		string[] array = optionText.Split('=');
		string text = array[0];
		int result = -1;
		if (array.Length > 1)
		{
			int.TryParse(array[1], out result);
		}
		if (text != null && text == "limit_upper_count")
		{
			if (result == -1)
			{
				return value;
			}
			return Math.Min(value, result);
		}
		return value;
	}

	public long ParseLong(string expression)
	{
		if (long.TryParse(expression, out var result))
		{
			return result;
		}
		int operatorIndex = GetOperatorIndex(expression);
		if (operatorIndex == -1 && long.TryParse(GetVariableValue(expression), out result))
		{
			return result;
		}
		string name = expression.Substring(0, operatorIndex);
		string name2 = expression.Substring(operatorIndex + 1);
		string variableValue = GetVariableValue(name);
		string variableValue2 = GetVariableValue(name2);
		char c = expression[operatorIndex];
		int num = ParseInt(variableValue);
		int num2 = ParseInt(variableValue2);
		return c switch
		{
			'+' => num + num2, 
			'-' => num - num2, 
			'*' => num * num2, 
			'/' => num / num2, 
			'%' => num % num2, 
			_ => throw new Exception(), 
		};
	}

	public void SettingReplaceIntData(ReplaceDataOptionValue data)
	{
		_replaceIntDataList.Add(data);
	}

	public static bool IsVariableValue(string name)
	{
		if (name.Length > 0 && name.First() == '{')
		{
			return name.Last() == '}';
		}
		return false;
	}

	private string GetVariableValue(string name)
	{
		name = ConvertNewVariableName(name);
		if (IsVariableValue(name))
		{
			if (name.Count((char c) => c == '{') >= 3)
			{
				return name;
			}
			if (name.Count((char c) => c == '}') >= 3)
			{
				return name;
			}
			string text = name.Trim('{', '}');
			if (text.IndexOf('{') > text.IndexOf('}'))
			{
				return name;
			}
			if (text.IndexOf('{') > -1 && text.IndexOf('}') > -1)
			{
				Match match = Regex.Match(text, "^(?<l>[^{]*)(?<c>{[^}]*})(?<r>.*)");
				text = match.Groups["l"].ToString() + GetVariableValue(match.Groups["c"].ToString()) + match.Groups["r"].ToString();
			}
			return _filterVariable.Parse(text).ToString();
		}
		if (_variableDictionary.ContainsKey(name))
		{
			return _variableDictionary[name];
		}
		return name;
	}

	private string ConvertNewVariableName(string name)
	{
		if (_filterVariable == null)
		{
			return name;
		}
		if (name != null && name == "CHARGE_COUNT")
		{
			return "{self.charge_count}";
		}
		return name;
	}

	public static IEnumerable<int> ParseOptionTokenID(string option)
	{
		return from str in (from s in option.Replace(")", "").Replace("(", "").Split(':')
				where !s.Contains("?")
				select s).ToArray()
			select int.Parse(str);
	}

	public static int ParseTokenOption(string option)
	{
		string[] source = option.Replace(")", "").Replace("(", "").Split('?');
		if (source.Count() < 2)
		{
			return -1;
		}
		return int.Parse(source.Last());
	}

	public SkillGainType GetShieldSkillGainType(SkillFilterCreator.ContentKeyword nameType, string defaultValue = null)
	{
		if (GetInfoByName(nameType, out var retInfo))
		{
			if (retInfo.ValueStr == SkillFilterCreator.ContentKeyword.skill.ToString())
			{
				return SkillGainType.ShieldSkill;
			}
			if (retInfo.ValueStr == SkillFilterCreator.ContentKeyword.spell.ToString())
			{
				return SkillGainType.ShieldSpell;
			}
			if (retInfo.ValueStr == SkillFilterCreator.ContentKeyword.attack.ToString())
			{
				return SkillGainType.ShieldAttack;
			}
			return SkillGainType.ShieldAll;
		}
		return SkillGainType.ShieldAll;
	}

	public DamageCutInfo.DamageType GetDamageCutGainType()
	{
		if (GetInfoByName(SkillFilterCreator.ContentKeyword.type, out var retInfo))
		{
			if (retInfo.ValueStr == SkillFilterCreator.ContentKeyword.skill.ToString())
			{
				return DamageCutInfo.DamageType.SKILL;
			}
			return DamageCutInfo.DamageType.ALL;
		}
		return DamageCutInfo.DamageType.ALL;
	}
}
