// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.Touch\ChoiceUtility.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View;
namespace Wizard.Battle.Touch
{
public partial class ChoiceUtility
{
        public static int GetNumberOfCardsToSelect(SkillBase choiceSkill) => default!;
        public static void ToggleChoiceButtonSprite(UIButton choiceButton, GameObject check, bool setActive, int numberOfCardsToSelect, bool isFusion = false, bool isComplete = false) { }
        public static void StopChoiceEffects(List<BattleCardBase> choiceCards) { }
        public static bool DoesDuplicateCardNotExistInHand(BattleCardBase actingCard) => default!;
        public static bool DoesChoiceCardHaveSelectSkill(BattleCardBase choiceCard, SkillBase choiceSkill) => default!;
        public static void SetupActingChoiceCardToBePlayedFromQueue(BattleCardBase actingCard, BattleCardBase choiceCard, BattlePlayerBase battlePlayer, bool isChoiceBrave) { }
        public static void SetupChoiceCardForSkillTargetSelect(BattleCardBase choiceCard) { }
        public static List<BattleCardBase> SortSelectedChoiceCards(List<BattleCardBase> allChoiceCards, List<BattleCardBase> selectedChoiceCards) => default!;
}
}
