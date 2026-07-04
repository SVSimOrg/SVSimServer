using System.Collections.Generic;
using Cute;
using LitJson;

namespace Wizard;

public class PackConfig
{

	public int PackId { get; set; }

	public int SleeveId { get; set; }

	public int SpecialSleeveId { get; set; }

	public PackCategory Category { get; set; }

	public bool IsSpecialCardPack
	{
		get
		{
			if (Category != PackCategory.SpecialCardPack)
			{
				return Category == PackCategory.LimitedSpecialCardPack;
			}
			return true;
		}
	}
}
