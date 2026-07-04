using System;
using System.Collections.Generic;
using System.Linq;

public class DamageClippingInfo
{
	private ISkillParameterSelectFilter _maxFilter;

	private ISkillParameterSelectFilter _minFilter;

	private int _clippingMaxRange = -1;

	private int _clippingMinRange = -1;

	public int ClippingMax { get; private set; }

	public int LifeLowerLimit { get; private set; } = -1;

	public int ClippingRangeMax(List<BattleCardBase> cards)
	{
		if (_maxFilter != null)
		{
			return _maxFilter.Filtering(cards).FirstOrDefault();
		}
		return _clippingMaxRange;
	}

	public int ClippingRangeMin(List<BattleCardBase> cards)
	{
		if (_minFilter != null)
		{
			return _minFilter.Filtering(cards).FirstOrDefault();
		}
		return _clippingMaxRange;
	}

	public bool IsClipping(BattleCardBase card, int value)
	{
		bool flag = _maxFilter != null || _clippingMaxRange != -1;
		bool flag2 = _minFilter != null || _clippingMinRange != -1;
		if (!flag && !flag2)
		{
			return true;
		}
		List<BattleCardBase> cards = new List<BattleCardBase> { card };
		if (!flag || value <= ClippingRangeMax(cards))
		{
			if (flag2)
			{
				return ClippingRangeMin(cards) <= value;
			}
			return true;
		}
		return false;
	}

	public DamageClippingInfo(int clippingMax, string maxRange, string minRange, int lifeLowerLimit)
	{
		ClippingMax = clippingMax;
		LifeLowerLimit = lifeLowerLimit;
		if (maxRange == null)
		{
			goto IL_0054;
		}
		if (!(maxRange == "self_life"))
		{
			if (maxRange == null || maxRange.Length != 0)
			{
				goto IL_0054;
			}
		}
		else
		{
			_maxFilter = new SkillParameterSelectLifeFilter();
		}
		goto IL_0060;
		IL_0060:
		if (minRange != null)
		{
			if (minRange == "self_life")
			{
				_minFilter = new SkillParameterSelectLifeFilter();
				return;
			}
			if (minRange != null && minRange.Length == 0)
			{
				return;
			}
		}
		_clippingMinRange = int.Parse(minRange);
		return;
		IL_0054:
		_clippingMaxRange = int.Parse(maxRange);
		goto IL_0060;
	}
}
