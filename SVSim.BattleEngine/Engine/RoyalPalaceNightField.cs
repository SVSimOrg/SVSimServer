// Post-Phase-5b (2026-07-03) UI stub. Every override in the pre-cull file was
// Unity-3D VFX/iTween/particle-system code driven by BattleCoroutine — never
// invoked headless. The type stays because BattleManagerBase.CreateManager's
// background-id switch instantiates it; inheriting BackGroundBase's no-op
// default virtuals is correct headless behavior.
public class RoyalPalaceNightField : RoyalPalaceField
{
	public override int FieldId => 14;
	public override int FieldEffectId => 4;

	public RoyalPalaceNightField(string bgmId = "NONE")
		: base(bgmId)
	{
	}
}
