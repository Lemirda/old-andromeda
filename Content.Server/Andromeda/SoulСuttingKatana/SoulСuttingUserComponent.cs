namespace Content.Server.Andromeda.SoulСuttingKatana;

[RegisterComponent]
public sealed partial class SoulCuttingUserComponent : Component
{
    [DataField("ownerUid")]
    public EntityUid OwnerUid;

    [DataField("katanaUid"), AutoNetworkedField]
    public EntityUid? KatanaUid;

    [DataField("maskUid"), AutoNetworkedField]
    public EntityUid? MaskUid;
}