public class ClassBasicCardList
{
	private static int[] AllBasicCardList = new int[8] { 100011010, 100011020, 100011030, 100011040, 100011050, 100012010, 100031010, 100031020 };

	private static int[] ElfBasicCardList = new int[11]
	{
		100111010, 100111020, 100111030, 100111040, 100111050, 100111060, 100111070, 100114010, 100121010, 100121020,
		100121030
	};

	private static int[] RoyalBasicCardList = new int[11]
	{
		100211010, 100211020, 100211030, 100211040, 100211050, 100211060, 100214010, 100214020, 100221010, 100221020,
		100222010
	};

	private static int[] WitchBasicCardList = new int[11]
	{
		100311010, 100314010, 100314020, 100314030, 100314040, 100314050, 100314060, 100314070, 100321010, 100321020,
		100321030
	};

	private static int[] DragonBasicCardList = new int[11]
	{
		100411010, 100411020, 100411030, 100411040, 100411050, 100414010, 100414020, 100414030, 100421010, 100421020,
		100424010
	};

	private static int[] NecroBasicCardList = new int[11]
	{
		100511010, 100511020, 100511030, 100511040, 100511050, 100511060, 100514010, 100514020, 100521010, 100521020,
		100521030
	};

	private static int[] VampireBasicCardList = new int[11]
	{
		100611010, 100611020, 100611030, 100611040, 100611050, 100614010, 100614020, 100614030, 100621010, 100621020,
		100624010
	};

	private static int[] BishopBasicCardList = new int[11]
	{
		100711010, 100711020, 100713010, 100713020, 100713030, 100714010, 100714020, 100714030, 100721010, 100721020,
		100723010
	};

	private static int[] NemesisBasicCardList = new int[11]
	{
		100811010, 100811020, 100811030, 100811040, 100811050, 100811060, 100811070, 100814010, 100821010, 100821020,
		100824010
	};

	public static int[] GetRandomBasicCardId(CardBasePrm.ClanType classType)
	{
		return classType switch
		{
			CardBasePrm.ClanType.MIN => ElfBasicCardList, 
			CardBasePrm.ClanType.ROYAL => RoyalBasicCardList, 
			CardBasePrm.ClanType.WITCH => WitchBasicCardList, 
			CardBasePrm.ClanType.DRAGON => DragonBasicCardList, 
			CardBasePrm.ClanType.NECRO => NecroBasicCardList, 
			CardBasePrm.ClanType.VAMPIRE => VampireBasicCardList, 
			CardBasePrm.ClanType.BISHOP => BishopBasicCardList, 
			CardBasePrm.ClanType.NEMESIS => NemesisBasicCardList, 
			_ => AllBasicCardList, 
		};
	}
}
