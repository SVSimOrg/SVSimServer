using System.Collections.Generic;

namespace Wizard;

public class PlayPtnWithToken
{
	public List<int> PlayPtn { get; private set; }

	public AISinglePlayptnRecord Record { get; private set; }

	public List<TokenPlayPattern> TokenPtn { get; private set; }

	public PlayPtnWithToken(List<int> playPtn, AISinglePlayptnRecord record, List<TokenPlayPattern> tokenPtn)
	{
		PlayPtn = playPtn;
		Record = record;
		TokenPtn = tokenPtn;
	}
}
