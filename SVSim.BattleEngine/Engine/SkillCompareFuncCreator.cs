using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wizard.Battle;

public static class SkillCompareFuncCreator
{
	public static readonly string Clan = "Clan";

	public static readonly string Tribe = "Tribe";

	public static readonly string Cost = "Cost";

	public static readonly string BaseCost = "BaseCost";

	public static readonly string BaseAtk = "BaseAtk";

	public static readonly string BaseLife = "BaseLife";

	public static readonly string Atk = "Atk";

	public static readonly string Life = "Life";

	public static readonly string MaxLife = "MaxLife";

	public static readonly string ChantCount = "ChantCount";

	public static readonly string BuffCount = "BuffCount";

	public static readonly string BuffLifeCount = "BuffLifeCount";

	public static readonly string AttackableCount = "AttackableCount";

	public static readonly string MaxAttackableCount = "MaxAttackableCount";

	public static readonly string CardId = "CardId";

	public static readonly string LastLife = "LastLife";

	public static readonly string GenericValue = "GenericValue";

	public static readonly string ChangeMaxLifeCount = "ChangeMaxLifeCount";

	public static TSource FindMin<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
	{
		return self.First((TSource c) => selector(c).Equals(self.Min(selector)));
	}

	public static TSource FindMax<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
	{
		return self.First((TSource c) => selector(c).Equals(self.Max(selector)));
	}

	public static Func<int, int, bool> Create(string op)
	{
		return op switch
		{
			"=" => (int a, int b) => a == b, 
			"!=" => (int a, int b) => a != b, 
			"<=" => (int a, int b) => a <= b, 
			">=" => (int a, int b) => a >= b, 
			"<" => (int a, int b) => a < b, 
			">" => (int a, int b) => a > b, 
			_ => (int a, int b) => false, 
		};
	}

	public static Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> Create(string op, string propertyName)
	{
		Func<int, int, bool> compareFunc = null;
		bool searchMax = false;
		switch (op)
		{
		case "=":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a == b;
		case "!=":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a != b;
		case "<=":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a <= b;
		case ">=":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a >= b;
		case "<":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a < b;
		case ">":
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => a > b;
		case "<:=":
			compareFunc = (int a, int b) => a <= b;
			searchMax = true;
			break;
		case ">:=":
			compareFunc = (int a, int b) => a >= b;
			break;
		case "<:":
			compareFunc = (int a, int b) => a < b;
			searchMax = true;
			break;
		case ">:":
			compareFunc = (int a, int b) => a > b;
			break;
		default:
			return (int a, int b, IReadOnlyBattleCardInfo c, IEnumerable<IReadOnlyBattleCardInfo> cards) => false;
		}
		return delegate(int a, int number, IReadOnlyBattleCardInfo card, IEnumerable<IReadOnlyBattleCardInfo> cards)
		{
			Type typeFromHandle = typeof(BattleCardBase);
			PropertyInfo prop = typeFromHandle.GetProperty(propertyName);
			IEnumerable<IReadOnlyBattleCardInfo> enumerable = cards.Where((IReadOnlyBattleCardInfo c) => compareFunc(int.Parse(prop.GetValue(c, null).ToString()), number));
			if (enumerable.Count() > 0)
			{
				IReadOnlyBattleCardInfo obj = (searchMax ? enumerable.FindMax((IReadOnlyBattleCardInfo c) => int.Parse(prop.GetValue(c, null).ToString())) : enumerable.FindMin((IReadOnlyBattleCardInfo c) => int.Parse(prop.GetValue(c, null).ToString())));
				int num = int.Parse(prop.GetValue(obj, null).ToString());
				if (num == -1)
				{
					return false;
				}
				return int.Parse(prop.GetValue(card, null).ToString()) == num;
			}
			return false;
		};
	}
}
