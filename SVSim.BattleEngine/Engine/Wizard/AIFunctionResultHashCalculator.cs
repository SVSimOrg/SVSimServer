using System.Collections.Generic;

namespace Wizard;

public static class AIFunctionResultHashCalculator
{
	private static readonly ulong[] PLAYPTN_HASH_FACTORS = new ulong[11]
	{
		251uL, 311uL, 379uL, 283uL, 113uL, 523uL, 269uL, 463uL, 911uL, 661uL,
		541uL
	};

	public static ulong GetHash(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, ulong argumentHash)
	{
		return 0 + owner.GetHash() + field.GetHash() + CalcPlayPtnHash(playPtn) + argumentHash;
	}

	private static ulong CalcPlayPtnHash(List<int> playPtn)
	{
		ulong num = 0uL;
		if (playPtn == null || playPtn.Count <= 0)
		{
			return num;
		}
		int num2 = PLAYPTN_HASH_FACTORS.Length;
		for (int i = 0; i < playPtn.Count; i++)
		{
			num += PLAYPTN_HASH_FACTORS[playPtn[i] % num2];
		}
		return num;
	}
}
