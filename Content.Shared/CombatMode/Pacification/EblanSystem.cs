using System.Diagnostics.CodeAnalysis;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.CombatMode.Pacification;

public sealed class EblanSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EblanComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EblanComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EblanComponent, BeforeThrowEvent>(OnBeforeThrow);
        SubscribeLocalEvent<EblanComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<EblanComponent, ShotAttemptedEvent>(OnShootAttempt);
        SubscribeLocalEvent<EblanComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<PacifismDangerousAttackComponent, AttemptPacifiedAttackEvent2>(OnPacifiedDangerousAttack);
    }

    private void OnUnpaused(Entity<EblanComponent> ent, ref EntityUnpausedEvent args)
    {
        if (ent.Comp.NextPopupTime2 != null)
            ent.Comp.NextPopupTime2 = ent.Comp.NextPopupTime2.Value + args.PausedTime;
    }

    private bool PacifiedCanAttack(EntityUid user, EntityUid target, [NotNullWhen(false)] out string? reason)
    {
        var ev = new AttemptPacifiedAttackEvent2(user);

        RaiseLocalEvent(target, ref ev);

        if (ev.Cancelled)
        {
            reason = ev.Reason;
            return false;
        }

        reason = null;
        return true;
    }

    private void ShowPopup(Entity<EblanComponent> user, EntityUid target, string reason)
    {
        // Popup logic.
        // Cooldown is needed because the input events for melee/shooting etc. will fire continuously
        if (target == user.Comp.LastAttackedEntity2
            && !(_timing.CurTime > user.Comp.NextPopupTime2))
            return;

        _popup.PopupClient(Loc.GetString(reason, ("entity", target)), user, user);
        user.Comp.NextPopupTime2 = _timing.CurTime + user.Comp.PopupCooldown2;
        user.Comp.LastAttackedEntity2 = target;
    }

    private void OnShootAttempt(Entity<EblanComponent> ent, ref ShotAttemptedEvent args)
    {
        // Disallow firing guns in all cases.
        ShowPopup(ent, args.Used, "pacified-cannot-fire-gun");
        args.Cancel();
    }

    private void OnAttackAttempt(EntityUid uid, EblanComponent component, AttackAttemptEvent args)
    {
        if (component.DisallowAllCombat2 || args.Disarm && component.DisallowDisarm2)
        {
            args.Cancel();
            return;
        }

        // If it's a disarm, let it go through (unless we disallow them, which is handled earlier)
        if (args.Disarm)
            return;

        // Allow attacking with no target. This should be fine.
        // If it's a wide swing, that will be handled with a later AttackAttemptEvent raise.
        if (args.Target == null)
            return;

        // If we would do zero damage, it should be fine.
        if (args.Weapon != null && args.Weapon.Value.Comp.Damage.GetTotal() == FixedPoint2.Zero)
            return;

        if (PacifiedCanAttack(uid, args.Target.Value, out var reason))
            return;

        ShowPopup((uid, component), args.Target.Value, reason);
        args.Cancel();
    }

    private void OnStartup(EntityUid uid, EblanComponent component, ComponentStartup args)
    {
        if (!TryComp<CombatModeComponent>(uid, out var combatMode))
            return;

        if (component.DisallowDisarm2 && combatMode.CanDisarm != null)
            _combatSystem.SetCanDisarm(uid, false, combatMode);

        if (component.DisallowAllCombat2)
        {
            _combatSystem.SetInCombatMode(uid, false, combatMode);
            _actionsSystem.SetEnabled(combatMode.CombatToggleActionEntity, false);
        }
        //_alertsSystem.ShowAlert(uid, AlertType.Pacified);
    }

    private void OnShutdown(EntityUid uid, EblanComponent component, ComponentShutdown args)
    {
        if (!TryComp<CombatModeComponent>(uid, out var combatMode))
            return;

        if (combatMode.CanDisarm != null)
            _combatSystem.SetCanDisarm(uid, true, combatMode);

        _actionsSystem.SetEnabled(combatMode.CombatToggleActionEntity, true);
        _alertsSystem.ClearAlert(uid, component.PacifiedAlert2);
    }

    private void OnBeforeThrow(Entity<EblanComponent> ent, ref BeforeThrowEvent args)
    {
        var thrownItem = args.ItemUid;
        var itemName = Identity.Entity(thrownItem, EntityManager);

        // Raise an AttemptPacifiedThrow event and rely on other systems to check
        // whether the candidate item is OK to throw:
        var ev = new AttemptPacifiedThrowEvent2(thrownItem, ent);
        RaiseLocalEvent(thrownItem, ref ev);
        if (!ev.Cancelled)
            return;

        args.Cancelled = true;

        // Tell the player why they can’t throw stuff:
        var cannotThrowMessage = ev.CancelReasonMessageId ?? "pacified-cannot-throw";
        _popup.PopupEntity(Loc.GetString(cannotThrowMessage, ("projectile", itemName)), ent, ent);
    }

    private void OnPacifiedDangerousAttack(Entity<PacifismDangerousAttackComponent> ent, ref AttemptPacifiedAttackEvent2 args)
    {
        args.Cancelled = true;
        args.Reason = "pacified-cannot-harm-indirect";
    }
}


/// <summary>
/// Raised when a Pacified entity attempts to throw something.
/// The throw is only permitted if this event is not cancelled.
/// </summary>
[ByRefEvent]
public struct AttemptPacifiedThrowEvent2
{
    public EntityUid ItemUid;
    public EntityUid PlayerUid;

    public AttemptPacifiedThrowEvent2(EntityUid itemUid, EntityUid playerUid)
    {
        ItemUid = itemUid;
        PlayerUid = playerUid;
    }

    public bool Cancelled { get; private set; } = false;
    public string? CancelReasonMessageId { get; private set; }

    /// <param name="reasonMessageId">
    /// Localization string ID for the reason this event has been cancelled.
    /// If null, a generic message will be shown to the player.
    /// Note that any supplied localization string MUST accept a '$projectile'
    /// parameter specifying the name of the thrown entity.
    /// </param>
    public void Cancel(string? reasonMessageId = null)
    {
        Cancelled = true;
        CancelReasonMessageId = reasonMessageId;
    }
}

/// <summary>
///     Raised ref directed on an entity when a pacified user is attempting to attack it.
///     If <see cref="Cancelled"/> is true, don't allow attacking.
///     <see cref="Reason"/> should be a loc string, if there needs to be special text for why the user isn't able to attack this.
/// </summary>
[ByRefEvent]
public record struct AttemptPacifiedAttackEvent2(EntityUid User, bool Cancelled = false, string Reason = "pacified-cannot-harm-directly");
