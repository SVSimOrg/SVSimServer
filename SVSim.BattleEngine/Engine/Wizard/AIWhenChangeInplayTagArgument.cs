using System.Collections.Generic;

namespace Wizard;

public class AIWhenChangeInplayTagArgument : AIFiltersAndSelectTypeArgument
{
	public bool IsImmediate { get; protected set; }

	public int TextHash { get; private set; }

	public AIWhenChangeInplayTagArgument(string text, bool isImmediate)
		: base(text)
	{
		IsImmediate = isImmediate;
		TextHash = text.GetHashCode();
	}

	public virtual void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISkillProcessInformation processInfo, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targets = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		if (IsImmediate)
		{
			ChangeInplayTagStartProcess(targets, field, tagOwner, playPtn, situation);
			return;
		}
		if (situation == null)
		{
			AIConsoleUtility.LogWarning("AIWhenChangeInplayTagArgument.Execute() error!! situation == null!!!!!");
			return;
		}
		if (processInfo == null)
		{
			AIConsoleUtility.LogError("AIWhenChangeInplayTagArgument.Execute() error!! processInfo == null!!!!!");
			return;
		}
		processInfo.AddExecutingAction(delegate
		{
			ChangeInplayTagStartProcess(targets, field, tagOwner, playPtn, situation);
		});
	}

	public virtual void Stop(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISkillProcessInformation processInfo, AISituationInfo situation)
	{
		List<AIVirtualCard> targets = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		if (IsImmediate)
		{
			ChangeInplayTagStopProcess(targets, field, tagOwner, playPtn, situation);
			return;
		}
		if (processInfo == null)
		{
			AIConsoleUtility.LogError("AIWhenChangeInplayTagArgument.Stop() error!! processInfo == null!!!!!");
			return;
		}
		processInfo.AddExecutingAction(delegate
		{
			ChangeInplayTagStopProcess(targets, field, tagOwner, playPtn, situation);
		});
	}

	protected virtual void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
	}

	protected virtual void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
	}
}
