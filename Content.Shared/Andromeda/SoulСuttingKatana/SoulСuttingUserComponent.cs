namespace Content.Shared.Andromeda.SoulСuttingKatana;

[RegisterComponent]
public sealed partial class SoulCuttingUserComponent : Component
{
    [DataField("ownerUid")]
    public EntityUid OwnerUid;

    [DataField("katanaUid")]
    public EntityUid? KatanaUid;

    [DataField("maskUid")]
    public EntityUid? MaskUid;
}