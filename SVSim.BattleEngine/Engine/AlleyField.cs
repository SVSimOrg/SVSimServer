// Post-Phase-5b (2026-07-03) UI stub. Every override in the pre-cull file was
// Unity-3D VFX/iTween/particle-system code driven by BattleCoroutine — never
// invoked headless. The type stays because BattleManagerBase.CreateManager's
// background-id switch instantiates it; inheriting BackGroundBase's no-op
// default virtuals is correct headless behavior.
public class AlleyField : BackGroundBase
{
	public override int FieldId => 22;
	public override int FieldEffectId => 22;

	public AlleyField(string bgmId = "NONE")
		: base(bgmId)
	{
	}
}
