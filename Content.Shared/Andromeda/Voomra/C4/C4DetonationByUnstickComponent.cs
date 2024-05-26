using Robust.Shared.GameStates;

namespace Content.Shared.Andromeda.Voomra.C4;

/// <summary>
///     A marker component for detonating C4 when trying to detach it
/// </summary>
/// <seealso cref="T:Content.Shared.Andromeda.Voomra.C4.C4DetonationByUnstickSystem"/>
[RegisterComponent, NetworkedComponent]
public sealed partial class C4DetonationByUnstickComponent : Component
{
    /// <summary>
    ///     An alternative to the outdated <see cref="Robust.Shared.GameObjects.IComponent.Owner">IComponent.Owner</see>
    /// </summary>
    [ViewVariables]
    public EntityUid Owner2 { get; set; } = EntityUid.Invalid;

    /// <summary>
    ///     The detonation flag when trying to remove
    /// </summary>
    [ViewVariables]
    public bool Detonation { get; set; } = false;
}
