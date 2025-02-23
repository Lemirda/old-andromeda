using Content.Shared.Actions;
using Content.Shared.Inventory;
using JetBrains.Annotations;

namespace Content.Shared.Andromeda.NightVision;

public sealed class NightVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<NightVisionComponent, NVInstantActionEvent>(OnActionToggle);
    }

    private void OnComponentStartup(EntityUid uid, NightVisionComponent component, ComponentStartup args)
    {
        if (component.IsToggle)
            _actionsSystem.AddAction(uid, ref component.ActionContainer, "SwitchNightVision");
    }

    private void OnActionToggle(EntityUid uid, NightVisionComponent component, NVInstantActionEvent args)
    {
        component.IsNightVision = !component.IsNightVision;
        var changeEv = new NightVisionnessChangedEvent(component.IsNightVision);
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
    }

    [PublicAPI]
    public void UpdateIsNightVision(EntityUid uid, NightVisionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        var old = component.IsNightVision;


        var ev = new CanVisionAttemptEvent();
        RaiseLocalEvent(uid, ev);
        component.IsNightVision = ev.NightVision;

        if (old == component.IsNightVision)
            return;

        var changeEv = new NightVisionnessChangedEvent(component.IsNightVision);
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
    }
}

[ByRefEvent]
public record struct NightVisionnessChangedEvent(bool NightVision);

public sealed class CanVisionAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public bool NightVision => Cancelled;
    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.MASK | SlotFlags.HEAD;
}