using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Interaction.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Actions.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Content.Server.Chat.Systems;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Andromeda.SoulСuttingKatana;

namespace Content.Server.Andromeda.SoulСuttingKatana;

public sealed class SoulCuttingKatanaSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SoulCuttingKatanaComponent, GetVerbsEvent<Verb>>(AddKatanaVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var component in EntityManager.EntityQuery<SoulCuttingKatanaComponent>(true))
        {
            if (!component.IsActive)
                return;

            component.DamageTimer = component.StandartTimer;
            component.DamageTimer -= frameTime;
            if (component.DamageTimer <= 0)
            {
                ApplyDamage(component);
            }
        }
    }

    public void ApplyDamage(SoulCuttingKatanaComponent component)
    {
        if (!HasComp<DamageableComponent>(component.OwnerUid))
            return;

        if (!_prototypeManager.TryIndex<DamageTypePrototype>("Slash", out var slashDamageType))
            return;

        var damage = new DamageSpecifier(slashDamageType, FixedPoint2.New(2.5));
        EntityManager.System<DamageableSystem>().TryChangeDamage(component.OwnerUid, damage);
    }

    private void AddKatanaVerbs(EntityUid katanaUid, SoulCuttingKatanaComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        Verb setOwnerVerb = new Verb
        {
            Text = "Стать владельцем",
            Act = () => SetOwner(katanaUid, component, args.User),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbRoboisseur/verbroboisseur.png"))
        };

        if (component.OwnerIdentified)
        {
            args.Verbs.Remove(setOwnerVerb);
        }
        else
        {
            args.Verbs.Add(setOwnerVerb);
        }

        if (component.OwnerUid == args.User)
        {
            Verb activateVerb = new Verb
            {
                Text = "Активировать катану",
                Act = () => ActivateKatana(katanaUid, component, args.User),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbRoboisseur/verbroboisseur.png"))
            };

            if (component.IsActive)
            {
                Verb deactivateVerb = new Verb
                {
                    Text = "Отключить катану",
                    Act = () => DeActivateKatana(katanaUid, component, args.User),
                    Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbRoboisseur/verbroboisseur.png"))
                };

                args.Verbs.Add(deactivateVerb);
                args.Verbs.Remove(activateVerb);
            }
            else
            {
                args.Verbs.Add(activateVerb);
            }
        }
    }

    private void SetOwner(EntityUid katanaUid, SoulCuttingKatanaComponent component, EntityUid ownerUid)
    {
        if (HasComp<SoulCuttingUserComponent>(ownerUid))
        {
            _popupSystem.PopupCursor(Loc.GetString("Вы не можете заключить сразу 2 сделки с дьяволом..."), ownerUid, PopupType.Large);
        }
        else
        {
            AddComp<SoulCuttingUserComponent>(ownerUid);

            if (TryComp<SoulCuttingUserComponent>(ownerUid, out var ownerComp))
            {
                ownerComp.OwnerUid = ownerUid;
                ownerComp.KatanaUid = katanaUid;
                component.OwnerUid = ownerUid;
                component.OwnerIdentified = true;
                _popupSystem.PopupCursor(Loc.GetString("Вы видите как ваше имя оказывается на клинке... Вы чувствуете лёгкость и силу."), ownerUid, PopupType.Large);
                AddComp<PointLightComponent>(katanaUid);
                TryComp<PointLightComponent>(katanaUid, out var light);
                _pointLight.SetColor(katanaUid, Color.Red, light);
                _pointLight.SetRadius(katanaUid, (float) 2.0, light);
                _pointLight.SetEnergy(katanaUid, (float) 1.0, light);
                var message = _random.Pick(component.OneBlockMessage);
                _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);
            }
        }
    }

    private void ActivateKatana(EntityUid katanaUid, SoulCuttingKatanaComponent component, EntityUid ownerUid)
    {
        component.IsActive = true;
        _popupSystem.PopupCursor(Loc.GetString("Вы чувствуете силу, которую невозможно познать. Но так же вы чувствуете, будто вас пронзает несколькими клинками каждую секунду."), ownerUid, PopupType.Large);

        if (TryComp<MovementSpeedModifierComponent>(ownerUid, out var moveMod))
        {
            component.OriginalWalkSpeed = moveMod.BaseWalkSpeed;
            component.OriginalSprintSpeed = moveMod.BaseSprintSpeed;

            var newWalkSpeed = component.OriginalWalkSpeed * 1.3f; //+ 30%
            var newSprintSpeed = component.OriginalSprintSpeed * 1.3f; //+ 30%

            _movementSpeedModifierSystem.ChangeBaseSpeed(ownerUid, newWalkSpeed, newSprintSpeed, moveMod.Acceleration);
        }

        AddComp<UnremoveableComponent>(katanaUid);

        TryComp<PointLightComponent>(katanaUid, out var light);
        _pointLight.SetColor(katanaUid, Color.DarkRed, light);
        _pointLight.SetRadius(katanaUid, (float) 10.0, light);
        _pointLight.SetEnergy(katanaUid, (float) 4.0, light);

        var message = _random.Pick(component.TwoBlockMessage);
        _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);
    }

    private void DeActivateKatana(EntityUid katanaUid, SoulCuttingKatanaComponent component, EntityUid ownerUid)
    {
        component.IsActive = false;
        _popupSystem.PopupCursor(Loc.GetString("Вы чувствуете что сила полученная вам, угасает... Кажется всё закончилось."), ownerUid, PopupType.Large);

        if (TryComp<MovementSpeedModifierComponent>(ownerUid, out var moveMod))
        {
            _movementSpeedModifierSystem.ChangeBaseSpeed(ownerUid, component.OriginalWalkSpeed, component.OriginalSprintSpeed, moveMod.Acceleration);
        }

        RemComp<UnremoveableComponent>(katanaUid);

        TryComp<PointLightComponent>(katanaUid, out var light);
        _pointLight.SetColor(katanaUid, Color.Red, light);
        _pointLight.SetRadius(katanaUid, (float) 2.0, light);
        _pointLight.SetEnergy(katanaUid, (float) 1.0, light);

        var message = _random.Pick(component.ThreeBlockMessage);
        _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);
    }
}