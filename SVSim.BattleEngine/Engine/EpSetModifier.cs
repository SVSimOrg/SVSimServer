public class EpSetModifier : ICardEpModifier
{
	private readonly int m_ep;

	public bool IsClearBeforeModifier => true;

	public EpSetModifier(int ep)
	{
		m_ep = ep;
	}

	public int CalcEp(int baseEp)
	{
		return m_ep;
	}
}
