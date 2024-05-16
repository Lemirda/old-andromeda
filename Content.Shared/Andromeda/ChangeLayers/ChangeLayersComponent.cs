using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Andromeda.ChangeLayers;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeLayersComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("actionChangeLayers", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ChangeLayersAction = "ActionChangeLayers";

    [DataField]
    public EntityUid? ChangeLayersActionEntity;

    [DataField("actionActive")]
    public bool ActionActive { get; set; } = false;
}