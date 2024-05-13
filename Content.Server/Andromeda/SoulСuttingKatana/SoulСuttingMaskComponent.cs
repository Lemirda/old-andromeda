using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Andromeda.SoulСuttingKatana;

[RegisterComponent]
public sealed partial class SoulCuttingMaskComponent : Component
{
    [DataField("ownerUid")]
    public EntityUid OwnerUid;

    [DataField("maskSealed")]
    public bool MaskSealed { get; set; } = false;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("recallKatanaSoulCuttingAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string RecallKatanaSoulCuttingAction = "ActionRecallSoulCuttingKatana";

    [DataField] public EntityUid? RecallKatanaActionSoulCuttingEntity;
}