using Robust.Shared.Prototypes;

namespace Content.Shared.Andromeda.Lemird.Nearsighted;

[RegisterComponent]
public sealed partial class ClothingGrantComponentComponent : Component
{
    [DataField("component", required: true)]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsActive = false;
}
