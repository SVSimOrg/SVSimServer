namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

/// <summary>
/// What <c>IStoryService.FinishAsync</c> returns to <c>StoryController.Finish</c>.
/// <see cref="Response"/> is the wire payload; <see cref="LeveledUp"/> and
/// <see cref="ClassId"/> are controller-side signals for the class_level_up mission emit
/// and are never serialized. <see cref="ClassId"/> is nullable because skip-shape finishes
/// (no class chosen) don't grant XP and therefore can't level anything up.
/// </summary>
public sealed record StoryFinishOutcome(FinishResponse Response, bool LeveledUp, int? ClassId);
