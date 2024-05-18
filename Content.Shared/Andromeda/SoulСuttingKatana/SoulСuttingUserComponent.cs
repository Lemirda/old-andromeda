using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Andromeda.SoulСuttingKatana;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SoulCuttingUserComponent : Component
{
    [DataField("ownerUid")]
    public EntityUid OwnerUid;

    [DataField("katanaUid")]
    public EntityUid? KatanaUid;

    [DataField("maskUid")]
    public EntityUid? MaskUid;

    [DataField("soulСuttingMaskPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SoulСuttingMaskPrototype = "SoulСuttingMask";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("getMaskSoulCuttingAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string GetMaskSoulCuttingAction = "ActionGetSoulCuttingMask";

    [DataField, AutoNetworkedField]
    public EntityUid? GetMaskActionSoulCuttingEntity;
}