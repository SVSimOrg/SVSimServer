namespace Wizard;

public class CloneActualFlags
{

	private static readonly CloneActualFlags ALL = new CloneActualFlags(inPlay: true, hand: true, deck: true, cemetery: true, banish: true, necromanceZone: true, fusionMaterial: true, unite: true, geton: true);

	public static CloneActualFlags All => ALL;

	public bool InPlay { get; private set; }

	public bool Hand { get; private set; }

	public bool Deck { get; private set; }

	public bool Cemetery { get; private set; }

	public bool Banish { get; private set; }

	public bool NecromanceZone { get; private set; }

	public bool FusionMaterial { get; private set; }

	public bool Unite { get; private set; }

	public bool GetOn { get; private set; }

	public CloneActualFlags(bool inPlay, bool hand, bool deck, bool cemetery, bool banish, bool necromanceZone, bool fusionMaterial, bool unite, bool geton)
	{
		InPlay = inPlay;
		Hand = hand;
		Deck = deck;
		Cemetery = cemetery;
		Banish = banish;
		NecromanceZone = necromanceZone;
		FusionMaterial = fusionMaterial;
		Unite = unite;
		GetOn = geton;
	}
}
