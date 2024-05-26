using Content.Shared.Andromeda.Botany;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Kitchen;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Andromeda.Botany.UI;

[UsedImplicitly]
public sealed class PlantExtractorBoundUserInterface : BoundUserInterface
{
    //[Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [ViewVariables]
    private PlantExtractorWindow? _window;

    public PlantExtractorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _window = new PlantExtractorWindow(this, EntMan, _prototypeManager);
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        _window?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not PlantExtractorBoundUserInterfaceState cState)
            return;

        _window?.UpdateState(cState);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);
        _window?.HandleMessage(message);
    }

    // Make Tehnox's shit cleaner someday
    // https://github.com/space-syndicate/space-station-14/commit/3ac4cf85db02c9db8196bf126ea9fc1c25a082b3
    public void StartExtracting(BaseButton.ButtonEventArgs? args = null) => SendMessage(new PlantExtractorStartMessage());
    public void EjectAll(BaseButton.ButtonEventArgs? args = null) => SendMessage(new PlantExtractorEjectChamberAllMessage());
    public void EjectBeaker(BaseButton.ButtonEventArgs? args = null) =>
        SendMessage(new ItemSlotButtonPressedEvent(SharedPlantExtractor.BeakerContainerId, true, false));
    public void EjectChamberContent(EntityUid uid) => SendMessage(new ReagentGrinderEjectChamberContentMessage(EntMan.GetNetEntity(uid)));
    public void SetTransferMode(BaseButton.ButtonEventArgs? args = null) => SendMessage(new PlantExtractorSetModeMessage(PlantExtractorMode.Transfer));
    public void SetDiscardMode(BaseButton.ButtonEventArgs? args = null) => SendMessage(new PlantExtractorSetModeMessage(PlantExtractorMode.Discard));
    public void TransferReagent(BaseButton.ButtonEventArgs? args, ReagentButton button) =>
        SendMessage(new PlantExtractorReagentAmountButtonMessage(button.Id, button.Amount, button.IsBuffer));
}