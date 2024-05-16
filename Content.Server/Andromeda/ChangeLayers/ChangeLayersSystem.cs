using Content.Shared.Popups;
using Content.Shared.Actions;
using Robust.Server.GameObjects;
using Content.Shared.Andromeda.ChangeLayers;
using Content.Shared.Actions.Events;

namespace Content.Server.Andromeda.ChangeLayers;
public sealed class ChangeLayersSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeLayersComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ChangeLayersComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ChangeLayersActionEvent>(OnChangeLayersAction);
    }

    private void OnComponentStartup(EntityUid uid, ChangeLayersComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ChangeLayersActionEntity, component.ChangeLayersAction);
    }

    private void OnShutdown(EntityUid uid, ChangeLayersComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ChangeLayersActionEntity);
    }

    private void OnChangeLayersAction(ChangeLayersActionEvent args)
    {
        if (EntityManager.TryGetComponent<VisibilityComponent>(args.Performer, out var visibilityComponent))
        {
            if (EntityManager.TryGetComponent<ChangeLayersComponent>(args.Performer, out var changeLayersComponent))
            {
                args.Handled = true;
                if (!changeLayersComponent.ActionActive)
                {
                    _visibilitySystem.SetLayer((args.Performer, visibilityComponent), 1);
                    _popupSystem.PopupCursor(Loc.GetString("Вы видимы для всех"), args.Performer, PopupType.Large);
                    changeLayersComponent.ActionActive = true;
                }
                else
                {
                    _visibilitySystem.SetLayer((args.Performer, visibilityComponent), 2);
                    _popupSystem.PopupCursor(Loc.GetString("Вы невидимы для всех"), args.Performer, PopupType.Large);
                    changeLayersComponent.ActionActive = false;
                }
            }
        }
    }
}