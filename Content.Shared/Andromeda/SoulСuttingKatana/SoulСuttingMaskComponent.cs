using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Speech;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Andromeda.SoulСuttingKatana;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SoulCuttingMaskComponent : Component
{
    [DataField("ownerUid")]
    public EntityUid OwnerUid;

    [DataField("maskSealed")]
    public bool MaskSealed { get; set; } = false;

    [DataField("ownerIdentified")]
    public bool OwnerIdentified { get; set; } = false;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("speechSounds")]
    public ProtoId<SpeechSoundsPrototype>? OriginalSpeechSounds;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("recallKatanaSoulCuttingAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string RecallKatanaSoulCuttingAction = "ActionRecallSoulCuttingKatana";

    [DataField, AutoNetworkedField]
    public EntityUid? RecallKatanaActionSoulCuttingEntity;
}