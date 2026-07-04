using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

namespace Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

public class CandidateCardInfo
{
	public List<CandidateCard> candidateCards;

	public CandidateCardInfo()
	{
	}

	public CandidateCardInfo(JsonData data)
	{
		candidateCards = new List<CandidateCard>();
		for (int i = 0; i < data.Count; i++)
		{
			CandidateCard item = new CandidateCard(data[i]);
			candidateCards.Add(item);
		}
	}
}
