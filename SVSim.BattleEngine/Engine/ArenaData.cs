using LitJson;
using Wizard;

public class ArenaData : HeaderData
{
	public enum eARENA_PAY
	{
		None = 0	}

	public ArenaTwoPickData TwoPickData { get; set; }

	public SealedData SealedData { get; private set; }

	public SealedMyPageResponseData SealedMyPageResponseData { get; private set; }

	public ArenaColosseum ColosseumData { get; set; }

	public ArenaCompetition CompetitionData { get; set; }

	public ArenaData()
	{
		SealedData = new SealedData();
		ColosseumData = new ArenaColosseum();
		CompetitionData = new ArenaCompetition();
	}

	public ArenaData(JsonData data)
		: this()
	{
		if (data != null)
		{
			JsonData data2 = data[0];
			TwoPickData = new ArenaTwoPickData(data2);
		}
	}

	public void ClearSealedData()
	{
		SealedData = new SealedData();
	}

	public static Format ApiDeckFormatParse(ArenaColosseum.eRule rule)
	{
		Format format = Format.Rotation;
		switch (rule)
		{
		case ArenaColosseum.eRule.RotationBo1:
			return Format.Rotation;
		case ArenaColosseum.eRule.UnlimitedBo1:
			return Format.Unlimited;
		case ArenaColosseum.eRule.TwoPick:
		case ArenaColosseum.eRule.TwoPickChaos:
			return Format.TwoPick;
		case ArenaColosseum.eRule.HOF:
		case ArenaColosseum.eRule.WindFall:
			return Format.Max;
		case ArenaColosseum.eRule.Crossover:
			return Format.Crossover;
		case ArenaColosseum.eRule.MyRotation:
			return Format.MyRotation;
		case ArenaColosseum.eRule.Avatar:
			return Format.Avatar;
		default:
			return Format.Max;
		}
	}
}
