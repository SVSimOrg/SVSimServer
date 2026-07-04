using System.Linq;

namespace Wizard;

public class PuzzleBattleMasterData : Master.ReadFromCsv
{

	public int Id { get; private set; }

	public int PlayerClass { get; private set; }

	public int PlayerLife { get; private set; }

	public int PlayerPPCount { get; private set; }

	public int PlayerEPCount { get; private set; }

	public int PlayerGraveCount { get; private set; }

	public CardPrm[] PlayerInplay { get; private set; }

	public int[] PlayerHand { get; private set; }

	public int[] PlayerDeck { get; private set; }

	public int PlayerDeckCount { get; private set; }

	public int EnemyClass { get; private set; }

	public int EnemyLife { get; private set; }

	public int EnemyPPCount { get; private set; }

	public int EnemyEPCount { get; private set; }

	public int EnemyGraveCount { get; private set; }

	public CardPrm[] EnemyInplay { get; private set; }

	public int[] EnemyHand { get; private set; }

	public int[] EnemyDeck { get; private set; }

	public int EnemyDeckCount { get; private set; }

	public string WinCondition { get; private set; }

	public string WinConditionTextId { get; private set; }

	public string HintTextId { get; private set; }

	public void ReadCsvColumns(string[] columns)
	{
		int num = 0;
		Id = int.Parse(columns[num]);
		num++;
		PlayerClass = int.Parse(columns[num]);
		num++;
		PlayerLife = (string.IsNullOrEmpty(columns[num]) ? 20 : int.Parse(columns[num]));
		num++;
		PlayerPPCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		PlayerEPCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		PlayerGraveCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		PlayerInplay = (string.IsNullOrEmpty(columns[num]) ? new CardPrm[0] : (from x in columns[num].Split(',')
			select new CardPrm(x)).ToArray());
		num++;
		PlayerHand = (string.IsNullOrEmpty(columns[num]) ? new int[0] : columns[num].Split(',').Select(int.Parse).ToArray());
		num++;
		PlayerDeck = (string.IsNullOrEmpty(columns[num]) ? new int[0] : columns[num].Split(',').Select(int.Parse).ToArray());
		num++;
		PlayerDeckCount = (string.IsNullOrEmpty(columns[num]) ? 40 : int.Parse(columns[num]));
		num++;
		EnemyClass = int.Parse(columns[num]);
		num++;
		EnemyLife = (string.IsNullOrEmpty(columns[num]) ? 20 : int.Parse(columns[num]));
		num++;
		EnemyPPCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		EnemyEPCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		EnemyGraveCount = ((!string.IsNullOrEmpty(columns[num])) ? int.Parse(columns[num]) : 0);
		num++;
		EnemyInplay = (string.IsNullOrEmpty(columns[num]) ? new CardPrm[0] : (from x in columns[num].Split(',')
			select new CardPrm(x)).ToArray());
		num++;
		EnemyHand = (string.IsNullOrEmpty(columns[num]) ? new int[0] : columns[num].Split(',').Select(int.Parse).ToArray());
		num++;
		EnemyDeck = (string.IsNullOrEmpty(columns[num]) ? new int[0] : columns[num].Split(',').Select(int.Parse).ToArray());
		num++;
		EnemyDeckCount = (string.IsNullOrEmpty(columns[num]) ? 40 : int.Parse(columns[num]));
		num++;
		WinCondition = (string.IsNullOrEmpty(columns[num]) ? "{me.inplay.class.life}>0&{op.inplay.class.life}<=0" : columns[num]);
		num++;
		WinConditionTextId = columns[num];
		num++;
		HintTextId = columns[num];
		num++;
	}
}
