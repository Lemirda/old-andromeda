using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared.Andromeda.Voomra.C4;

/// <summary>
///     Detonation control system C4 when trying to detach it
/// </summary>
/// /// <seealso cref="T:Content.Shared.Andromeda.Voomra.C4.C4DetonationByUnstickComponent"/>
public sealed class C4DetonationByUnstickSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<C4DetonationByUnstickComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<C4DetonationByUnstickComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<C4DetonationByUnstickComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<C4DetonationByUnstickComponent, ExaminedEvent>(OnExamined);
    }

    private void OnComponentInit(EntityUid uid, C4DetonationByUnstickComponent component, ComponentInit args)
    {
        component.Owner2 = uid;
    }

    private void OnComponentRemove(EntityUid uid, C4DetonationByUnstickComponent component, ComponentRemove args)
    {
        component.Owner2 = EntityUid.Invalid;
    }

    private void OnGetAltVerbs(EntityUid uid, C4DetonationByUnstickComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (HasComp<ActiveTimerTriggerComponent>(uid))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("verb-c4-detonation-by-unstick", ("status", component.Detonation
                ? Loc.GetString("verb-c4-detonation-by-unstick-status-on")
                : Loc.GetString("verb-c4-detonation-by-unstick-status-off"))),
            Act = () => DoAltVerbs(component, uid, args.User)
        });
    }

    private void DoAltVerbs(C4DetonationByUnstickComponent component, EntityUid c4, EntityUid user)
    {
        component.Detonation = !component.Detonation;
        //есть странный баг: сообщение отображается задвоенно, словно popup вызывается дважды
        _popupSystem.PopupEntity(
            component.Detonation
                ? Loc.GetString("popup-c4-detonation-by-unstick-on")
                : Loc.GetString("popup-c4-detonation-by-unstick-off"),
            c4, user);
    }

    private void OnExamined(EntityUid uid, C4DetonationByUnstickComponent component, ExaminedEvent args)
    {
        if (component.Detonation)
            args.PushMarkup(Loc.GetString("examine-c4-detonation-by-unstick"));
    }
}
