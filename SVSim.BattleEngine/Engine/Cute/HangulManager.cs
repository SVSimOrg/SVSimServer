using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Cute;

public static class HangulManager
{
	private class JosiConversionRule
	{
		public char Type { get; private set; }

		public string Text1 { get; private set; }

		public string Text2 { get; private set; }

		public Func<char, bool> IsConvertToText1 { get; private set; }

		public JosiConversionRule(char type, string text1, string text2, Func<char, bool> isConvertToText1)
		{
			Type = type;
			Text1 = text1;
			Text2 = text2;
			IsConvertToText1 = isConvertToText1;
		}
	}

	private class DecomposedHangul
	{
		public char? Chosung { get; set; }

		public char? Jungsung { get; set; }

		public char? Jongsung { get; set; }

		public DecomposedHangul()
		{
			Chosung = null;
			Jungsung = null;
			Jongsung = null;
		}

		public DecomposedHangul(char hangulCharacter)
		{
			int num = hangulCharacter - 44032;
			int num2 = (int)Mathf.Floor((float)num / (float)JUNGSUNG_TABLE.Length / (float)JONGSUNG_TABLE.Length);
			Chosung = CHOSUNG_TABLE[num2];
			int num3 = (int)Mathf.Floor((float)num / (float)JONGSUNG_TABLE.Length - (float)(num2 * JUNGSUNG_TABLE.Length));
			Jungsung = JUNGSUNG_TABLE[num3];
			Jongsung = JONGSUNG_TABLE[num % JONGSUNG_TABLE.Length];
		}
	}

	private static readonly char[] CHOSUNG_TABLE = new char[19]
	{
		'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ',
		'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
	};

	private static readonly char[] JUNGSUNG_TABLE = new char[21]
	{
		'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ',
		'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ',
		'ㅣ'
	};

	private static readonly char?[] JONGSUNG_TABLE = new char?[28]
	{
		null, 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ',
		'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ',
		'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
	};

	private static readonly JosiConversionRule[] RULE_TABLE = new JosiConversionRule[6]
	{
		new JosiConversionRule('a', "이", "가", IsConvertToText1_common),
		new JosiConversionRule('b', "은", "는", IsConvertToText1_common),
		new JosiConversionRule('c', "을", "를", IsConvertToText1_common),
		new JosiConversionRule('d', "과", "와", IsConvertToText1_common),
		new JosiConversionRule('e', "으로", "로", IsConvertToText1_typeE),
		new JosiConversionRule('f', "이라면", "라면", IsConvertToText1_common)
	};

	private static StringBuilder _strBuilder = new StringBuilder(512);

	private const string START_TAG = "START_TAG";

	private const string END_TAG = "END_TAG";

	private const string ENCLOSED = "ENCLOSED";

	private static readonly string[] NUMERAL_TABLE = new string[100]
	{
		"0", "하나", "둘", "셋", "넷", "다섯", "여섯", "일곱", "여덟", "아홉",
		"열", "열하나", "열둘", "열셋", "열넷", "열다섯", "열여섯", "열일곱", "열여덟", "열아홉",
		"스물", "스물하나", "스물둘", "스물셋", "스물넷", "스물다섯", "스물여섯", "스물일곱", "스물여덟", "스물아홉",
		"서른", "서른하나", "서른둘", "서른셋", "서른넷", "서른다섯", "서른여섯", "서른일곱", "서른여덟", "서른아홉",
		"마흔", "마흔하나", "마흔둘", "마흔셋", "마흔넷", "마흔다섯", "마흔여섯", "마흔일곱", "마흔여덟", "마흔아홉",
		"쉰", "쉰하나", "쉰둘", "쉰셋", "쉰넷", "쉰다섯", "쉰여섯", "쉰일곱", "쉰여덟", "쉰아홉",
		"예순", "예순하나", "예순둘", "예순셋", "예순넷", "예순다섯", "예순여섯", "예순일곱", "예순여덟", "예순아홉",
		"일흔", "일흔하나", "일흔둘", "일흔셋", "일흔넷", "일흔다섯", "일흔여섯", "일흔일곱", "일흔여덟", "일흔아홉",
		"여든", "여든하나", "여든둘", "여든셋", "여든넷", "여든다섯", "여든여섯", "여든일곱", "여든여덟", "여든아홉",
		"아흔", "아흔하나", "아흔둘", "아흔셋", "아흔넷", "아흔다섯", "아흔여섯", "아흔일곱", "아흔여덟", "아흔아홉"
	};

	public static string ConvertRule(string inputStr)
	{
		return ConvertJosiType(ConvertNumeral(inputStr));
	}

	private static string ConvertNumeral(string inputStr)
	{
		foreach (Match item in Regex.Matches(inputStr, "(?<START_TAG>\\[num\\])(?<ENCLOSED>.*?)(?<END_TAG>\\[/num\\])").Cast<Match>().Reverse())
		{
			Group obj = item.Groups["END_TAG"];
			inputStr = inputStr.Remove(obj.Index, obj.Length);
			Group obj2 = item.Groups["ENCLOSED"];
			foreach (Match item2 in Regex.Matches(obj2.Value, "\\d+").Cast<Match>().Reverse())
			{
				Group obj3 = item2.Groups[0];
				int num = int.Parse(obj3.Value);
				if (0 < num && num < NUMERAL_TABLE.Length)
				{
					int startIndex = obj2.Index + obj3.Index;
					inputStr = inputStr.Remove(startIndex, obj3.Length).Insert(startIndex, NUMERAL_TABLE[num]);
				}
			}
			Group obj4 = item.Groups["START_TAG"];
			inputStr = inputStr.Remove(obj4.Index, obj4.Length);
		}
		return inputStr;
	}

	private static string ConvertJosiType(string inputStr)
	{
		if (inputStr.Length <= 0)
		{
			return inputStr;
		}
		_strBuilder.Length = 0;
		_strBuilder.Append(inputStr[0]);
		int length = inputStr.Length;
		for (int i = 1; i < length; i++)
		{
			char c = inputStr[i];
			if (c != '@')
			{
				_strBuilder.Append(c);
				continue;
			}
			if (i + 1 == length)
			{
				_strBuilder.Append(c);
				break;
			}
			bool flag = false;
			char c2 = inputStr[i + 1];
			for (int j = 0; j < RULE_TABLE.Length; j++)
			{
				JosiConversionRule josiConversionRule = RULE_TABLE[j];
				if (josiConversionRule.Type == c2)
				{
					flag = true;
					_strBuilder.Append(josiConversionRule.IsConvertToText1(inputStr[i - 1]) ? josiConversionRule.Text1 : josiConversionRule.Text2);
					i++;
					break;
				}
			}
			if (!flag)
			{
				_strBuilder.Append(c);
			}
		}
		return _strBuilder.ToString();
	}

	private static char? GetJongsung(char character)
	{
		if (character < '가' || '힣' < character)
		{
			return null;
		}
		return JONGSUNG_TABLE[(character - 44032) % JONGSUNG_TABLE.Length];
	}

	private static bool IsConvertToText1_common(char latestCharacter)
	{
		if (GetJongsung(latestCharacter).HasValue)
		{
			return true;
		}
		if ("013678".IndexOf(latestCharacter) >= 0)
		{
			return true;
		}
		return false;
	}

	private static bool IsConvertToText1_typeE(char latestCharacter)
	{
		char? jongsung = GetJongsung(latestCharacter);
		if (jongsung.HasValue && jongsung.Value != 'ㄹ')
		{
			return true;
		}
		if ("036".IndexOf(latestCharacter) >= 0)
		{
			return true;
		}
		return false;
	}
}
