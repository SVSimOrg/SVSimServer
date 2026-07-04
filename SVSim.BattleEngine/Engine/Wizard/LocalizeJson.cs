using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using LitJson;
using UnityEngine;

namespace Wizard;

public class LocalizeJson
{
	private enum ParserState
	{
		ObjectAwait,
		ObjectStart,
		ObjectEnd,
		KeyAwait,
		KeyStart,
		KeyEnd,
		ValueAwait,
		ValueStart,
		ValueEnd
	}

	private static readonly string PC_SP_TEXT_PREFIX = "[pcsp]";

	private static readonly char PC_SP_SPLIT = '|';

	public static void Parse(IDictionary<string, string> dic, string region, string text, bool isTrimKey = false)
	{
		int i;
		for (i = 0; i < text.Length && Convert.ToInt32(text[i]) == 65279; i++)
		{
		}
		int length = text.Length;
		int num = 0;
		int num2 = 0;
		string text2 = "";
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		ParserState parserState = ParserState.ObjectAwait;
		for (; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '{':
				switch (parserState)
				{
				case ParserState.ObjectAwait:
					parserState = ParserState.ObjectStart;
					num2++;
					break;
				default:
					flag4 = true;
					break;
				case ParserState.KeyStart:
				case ParserState.ValueStart:
					break;
				}
				break;
			case '"':
				switch (parserState)
				{
				case ParserState.ObjectStart:
				case ParserState.KeyAwait:
					parserState = ParserState.KeyStart;
					num = i + 1;
					break;
				case ParserState.ValueAwait:
					parserState = ParserState.ValueStart;
					num = i + 1;
					break;
				case ParserState.KeyStart:
					if (flag3)
					{
						text2 = text.Substring(num, i - num);
					}
					else if (num2 == 2 && text.Substring(num, i - num) == region)
					{
						flag3 = true;
					}
					parserState = ParserState.KeyEnd;
					break;
				case ParserState.ValueStart:
					if (flag3)
					{
						string text3 = text.Substring(num, i - num);
						if (flag)
						{
							text3 = text3.Replace("\\n", "\n");
							flag = false;
						}
						if (flag2)
						{
							text3 = text3.Replace("\\\"", "\"");
							flag2 = false;
						}
						if (text3.StartsWith(PC_SP_TEXT_PREFIX, StringComparison.Ordinal))
						{
							text3 = text3.Remove(0, PC_SP_TEXT_PREFIX.Length);
							text3 = text3.Split(PC_SP_SPLIT)[0];
						}
						dic.Add(isTrimKey ? text2.Trim() : text2, text3);
					}
					parserState = ParserState.ValueEnd;
					break;
				default:
					flag4 = true;
					break;
				}
				break;
			case ':':
				switch (parserState)
				{
				case ParserState.KeyEnd:
					parserState = ((num2 == 3) ? ParserState.ValueAwait : ParserState.ObjectAwait);
					break;
				default:
					flag4 = true;
					break;
				case ParserState.KeyStart:
				case ParserState.ValueStart:
					break;
				}
				break;
			case '}':
				switch (parserState)
				{
				case ParserState.ObjectStart:
				case ParserState.ObjectEnd:
				case ParserState.ValueEnd:
					if (flag3)
					{
						return;
					}
					parserState = ParserState.ObjectEnd;
					num2--;
					break;
				default:
					flag4 = true;
					break;
				case ParserState.KeyStart:
				case ParserState.ValueStart:
					break;
				}
				break;
			case ',':
				switch (parserState)
				{
				case ParserState.ObjectEnd:
					parserState = ParserState.KeyAwait;
					break;
				case ParserState.ValueEnd:
					parserState = ParserState.KeyAwait;
					break;
				default:
					flag4 = true;
					break;
				case ParserState.KeyStart:
				case ParserState.ValueStart:
					break;
				}
				break;
			case '\\':
				i++;
				if (flag3 && i < text.Length)
				{
					switch (text[i])
					{
					case 'n':
						flag = true;
						break;
					case '"':
						flag2 = true;
						break;
					}
				}
				break;
			default:
				if (parserState != ParserState.KeyStart && parserState != ParserState.ValueStart)
				{
					switch (c)
					{
					default:
						flag4 = true;
						break;
					case '\t':
					case '\n':
					case '\r':
					case ' ':
						break;
					}
				}
				break;
			}
			if (flag4)
			{
				throw new ApplicationException($"Could not parse localize JSON: invalid '{c}' at position {i}, state {parserState.ToString()}, around '{text.Substring(Math.Max(i - 32, 0), Math.Min(64, length - i))}'");
			}
		}
	}
}
