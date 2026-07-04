using System;
using System.Text.RegularExpressions;

public class SkillVariableComareFilter
{
	protected readonly Func<int, int, bool> _compareFunc;

	public string Lhs { get; private set; }

	public string Rhs { get; private set; }

	public string Compare { get; private set; }

	public string Text { get; protected set; }

	public SkillVariableComareFilter(string text)
	{
		Text = text;
		Match match = Regex.Match(text, "^(?<lhs>((?<open>{)[^{}]*)+((?<-open>})[^{}]*)*(?<-open>})([+\\-\\*/]((?<open>{)[^{}]*)+((?<-open>})[^{}]*)*(?<-open>}))*(%[0-9]+)*)(?<c>[<>!]?:?=?)(?<rhs>.*)");
		Lhs = match.Groups["lhs"].ToString();
		Rhs = match.Groups["rhs"].ToString();
		Compare = match.Groups["c"].ToString();
		_compareFunc = SkillCompareFuncCreator.Create(Compare);
	}

	public virtual bool Filtering(SkillOptionValue optionValue)
	{
		int arg = optionValue.ParseInt(Lhs);
		int arg2 = optionValue.ParseInt(Rhs);
		return _compareFunc(arg, arg2);
	}
}
