using System;

namespace Wizard.Battle.Recovery;

// PASS-5 STUB: full 1072-line body dropped. RecoveryDataHandler drove the client-side stock-
// receive-message replay for reconnect flows. Nothing constructs it in the headless node
// (the only ctor callsite was RecoveryController's ctor body, itself now a stub). The type
// must exist because RecoveryOperationCollection.cs:42 accesses
// `_recoveryController.RecoveryDataHandlerInstance.OnCompleteRecovery += ...`. Since
// _recoveryController is always null in the node, the NRE happens on the FIRST deref
// (`_recoveryController.RecoveryDataHandlerInstance`) — we never reach the event
// subscription, so the event body doesn't matter. Kept as a symbol placeholder.
public class RecoveryDataHandler
{
    public event Action OnCompleteRecovery;
}
