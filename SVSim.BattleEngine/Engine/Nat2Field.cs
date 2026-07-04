// Post-Phase-5b (2026-07-03) UI stub. Every override in the pre-cull file was
// Unity-3D VFX/iTween/particle-system code driven by BattleCoroutine — never
// invoked headless. The type stays because BattleManagerBase.CreateManager's
// background-id switch instantiates it; inheriting BackGroundBase's no-op
// default virtuals is correct headless behavior.
public class Nat2Field : NateField
{
	public override int FieldId => 32;

	public Nat2Field(string bgmId = "NONE")
		: base(bgmId)
	{
	}
}
