using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server.Andromeda.SoulСuttingKatana;

public sealed class SoulCuttingMaskSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SoulCuttingMaskComponent, GetVerbsEvent<Verb>>(AddMaskVerbs);
        SubscribeLocalEvent<SoulCuttingMaskComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SoulCuttingMaskComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<SoulCuttingUserComponent, RecallSoulCuttingKatanaEvent>(OnRecallKatana);
    }

    private void OnMapInit(EntityUid uid, SoulCuttingMaskComponent component, MapInitEvent args)
    {
        Log.Info($"Добавляем контейнер.");
        _actionContainer.AddAction(uid, component.RecallKatanaSoulCuttingAction);
    }

    private void AddMaskVerbs(EntityUid maskUid, SoulCuttingMaskComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<SoulCuttingUserComponent>(args.User, out var userComp))
            return;

        if (userComp.OwnerUid != args.User)
            return;


        Verb saveVerb = new Verb
        {
            Text = "Зафиксировать маску",
            Act = () => SaveMask(maskUid, component, args.User),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbRoboisseur/verbroboisseur.png"))
        };

        if (component.MaskSealed)
        {
            Verb removeVerb = new Verb
            {
                Text = "Снять маску",
                Act = () => RemoveMask(maskUid, component),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbRoboisseur/verbroboisseur.png"))
            };

            args.Verbs.Remove(saveVerb);
            args.Verbs.Add(removeVerb);
        }
        else
        {
            args.Verbs.Add(saveVerb);
        }
    }

    private void SaveMask(EntityUid maskUid, SoulCuttingMaskComponent maskComp, EntityUid ownerUid)
    {
        if (TryComp<SoulCuttingUserComponent>(ownerUid, out var ownerComp))
        {
            ownerComp.MaskUid = maskUid;
            maskComp.MaskSealed = true;

            AddComp<UnremoveableComponent>(maskUid);
            _popupSystem.PopupCursor("Вы чувствует как маска наполняется энергией и запечатывается...", ownerComp.OwnerUid, PopupType.Large);
        }
        else
        {
            _popupSystem.PopupCursor("Вы не владете древней техникой владения проклятой катаной.", ownerUid, PopupType.Large);
        }
    }

    private void RemoveMask(EntityUid maskUid, SoulCuttingMaskComponent maskComp)
    {
        if (maskComp.MaskSealed)
        {
            maskComp.MaskSealed = false;

            RemComp<UnremoveableComponent>(maskUid);
        }
    }

    private void OnGetItemActions(EntityUid maskUid, SoulCuttingMaskComponent component, GetItemActionsEvent args)
    {
        Log.Info($"Вход в OnGetItem");
        if (HasComp<SoulCuttingUserComponent>(args.User))
        {
            Log.Info($"Добавляем actions");
            args.AddAction(ref component.RecallKatanaActionSoulCuttingEntity, component.RecallKatanaSoulCuttingAction);
        }
        else
        {
            Log.Info($"actions невозможно добавить.");
        }
    }

    private void OnRecallKatana(EntityUid maskUid, SoulCuttingUserComponent component, RecallSoulCuttingKatanaEvent args)
    {
        Log.Info($"АААА ПОЕХАЛИ");
        if (TryComp<SoulCuttingMaskComponent>(maskUid, out var maskComp) && HasComp<SoulCuttingKatanaComponent>(component.KatanaUid))
        {
            if (maskComp.MaskSealed)
            {
                _hands.TryPickupAnyHand(args.Performer, component.KatanaUid.Value);
                _popupSystem.PopupEntity("Катана появляется в руках.", args.Performer, args.Performer);
                args.Handled = true;
            }
            else
            {
                _popupSystem.PopupEntity("Маска не зафиксированна.", args.Performer, args.Performer);
            }
        }
        else
        {
            Log.Info($"СУКААААААААА");
        }
    }
}