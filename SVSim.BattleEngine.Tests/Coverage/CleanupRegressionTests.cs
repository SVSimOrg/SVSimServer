extern alias engine;
using NUnit.Framework;
using System.Reflection;

namespace SVSim.BattleEngine.Tests.Coverage;

/// <summary>Regression backstop for the engine shim cleanup. Asserts every type we
/// deleted in Tasks 9-15 of docs/superpowers/plans/2026-06-27-engine-shim-cleanup.md
/// stays deleted. If a rebase or merge accidentally re-introduces one, this test
/// fails loudly.</summary>
[TestFixture]
public class CleanupRegressionTests
{
    private static readonly string[] DeletedFqns = new[]
    {
        // Task 9 — DEAD-ORPHAN sweep (commit 119bf77)
        "Wizard.AIBarrierGlobal",

        // Task 10 — *Vfx.cs sweep (commit 3235f47)
        "Wizard.Battle.Player.ClassCharacter.SkinEffectVfx",
        "Wizard.Battle.Player.Emotion.Debug722006NullVfx",

        // Task 12 — *Page.cs sweep (commit b3e2df2)
        "Wizard.Bingo.BingoPage",
        "Wizard.Lottery.LotteryPage",
        "Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase.BuildDeckPurchasePage",
        "Wizard.StorySelectPage",

        // Task 13 — *Window.cs sweep (commit 92b6bf6)
        "Wizard.CardSleeveDetailWindow",
        "Wizard.ClassSkinDetailWindow",
        "Wizard.OptionSettingWindow",
        "Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase.BuildDeckDetailWindow",
    };

    [TestCaseSource(nameof(DeletedFqns))]
    public void DeletedType_StaysDeleted(string fqn)
    {
        var asm = typeof(engine::BattleManagerBase).Assembly;
        Assert.That(asm.GetType(fqn), Is.Null,
            $"Type {fqn} was deleted in the engine shim cleanup (see plan Tasks 9-15) and must not be re-introduced.");
    }
}
