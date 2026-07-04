using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class Skill_invoke_emote : SkillBase
{
	public Skill_invoke_emote(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string[] array = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invoke_emote, "_OPT_NULL_").Split(':');
		if (array.Length < 4)
		{
			return NullVfxWithLoading.GetInstance();
		}
		return VfxWithLoading.Create(parameter.targetCards.First().SelfBattlePlayer.Emotion.PlayEmotion((ClassCharaPrm.MotionType)int.Parse(array[1]), (ClassCharaPrm.FaceType)int.Parse(array[0]), array[2], Data.Master.GetEmoteWordText(array[3])));
	}
}
