using UnityEngine;

public class CharIdx : MonoBehaviour
{

	public int m_CardId { get; protected set; }

	public void SetCardId(int cardid)
	{
		m_CardId = cardid;
	}

	public int GetCardId()
	{
		return m_CardId;
	}
}
