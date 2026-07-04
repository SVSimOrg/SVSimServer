using Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;
using Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

public class RoomTwoPickInfo
{

	public Deck deckData;

	public CandidateChaos CandidateChaos;

	public RoomTwoPickInfo()
	{
		deckData = new Deck();
		deckData.cardIds = new int[0];
		CandidateChaos = new CandidateChaos();
	}
}
