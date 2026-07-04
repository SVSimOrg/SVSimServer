using System.Collections.Generic;
using Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class RoomTwoPickBeforeBattleInfo
{
	public Deck receiveDeck { get; private set; }

	public RoomTwoPickBeforeBattleInfo()
	{
		receiveDeck = new Deck();
		receiveDeck.cardIds = new int[0];
	}
}
