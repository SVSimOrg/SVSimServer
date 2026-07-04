using System;

namespace Wizard.Battle.Phase;

public interface IResultPhase : IPhase
{
	event Action OnSetupEnd;
}
