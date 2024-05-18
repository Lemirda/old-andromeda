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
using Content.Shared.Mobs;
using Content.Shared.Weapons.Reflect;

namespace Content.Server.Andromeda.SoulСuttingKatana;

public sealed class SoulCuttingKatanaSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SoulCuttingKatanaComponent, GetVerbsEvent<Verb>>(AddKatanaVerbs);
        SubscribeLocalEvent<SoulCuttingUserComponent, GetSoulCuttingMaskEvent>(OnGetMask);
        SubscribeLocalEvent<SoulCuttingUserComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var katanaComp in EntityManager.EntityQuery<SoulCuttingKatanaComponent>(true))
        {
            if (!katanaComp.IsActive)
                return;

            katanaComp.DamageTimer -= frameTime;
            if (katanaComp.DamageTimer <= 0)
            {
                ApplyDamage(katanaComp);
                katanaComp.DamageTimer = katanaComp.DamageInterval;
            }
        }
    }

    public void ApplyDamage(SoulCuttingKatanaComponent katanaComp)
    {
        if (!_prototypeManager.TryIndex<DamageTypePrototype>("Slash", out var slashDamageType))
            return;

        var damage = new DamageSpecifier(slashDamageType, FixedPoint2.New(2.5));
        EntityManager.System<DamageableSystem>().TryChangeDamage(katanaComp.OwnerUid, damage);
    }

    private void AddKatanaVerbs(EntityUid katanaUid, SoulCuttingKatanaComponent katanaComp, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (HasComp<SoulCuttingUserComponent>(args.User) && katanaComp.OwnerUid != args.User)
            return;

        Verb setOwnerVerb = new Verb
        {
            Text = "Стать владельцем",
            Act = () => SetOwner(katanaUid, katanaComp, args.User),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbKatana/takekatana.png"))
        };

        if (katanaComp.OwnerIdentified)
        {
            args.Verbs.Remove(setOwnerVerb);
        }
        else
        {
            args.Verbs.Add(setOwnerVerb);
        }

        if (katanaComp.OwnerUid == args.User)
        {
            Verb activateVerb = new Verb
            {
                Text = "Активировать катану",
                Act = () => ActivateKatana(katanaUid, katanaComp, args.User),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbKatana/activatekatana.png"))
            };

            if (katanaComp.IsActive)
            {
                Verb deactivateVerb = new Verb
                {
                    Text = "Отключить катану",
                    Act = () => DeActivateKatana(katanaUid, katanaComp, args.User),
                    Icon = new SpriteSpecifier.Texture(new("/Textures/Andromeda/Lemird/VerbKatana/deactivatekatana.png"))
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

    private void SetOwner(EntityUid katanaUid, SoulCuttingKatanaComponent katanaComp, EntityUid ownerUid)
    {
        AddComp<SoulCuttingUserComponent>(ownerUid);

        if (TryComp<SoulCuttingUserComponent>(ownerUid, out var ownerComp))
        {
            ownerComp.OwnerUid = ownerUid;
            ownerComp.KatanaUid = katanaUid;
            katanaComp.OwnerUid = ownerUid;
            katanaComp.OwnerIdentified = true;

            _popupSystem.PopupCursor(Loc.GetString("Непонятные символы оказываются на катане..."), ownerUid, PopupType.Large);

            AddComp<PointLightComponent>(katanaUid);
            TryComp<PointLightComponent>(katanaUid, out var light);
            _pointLight.SetColor(katanaUid, Color.Red, light);
            _pointLight.SetRadius(katanaUid, (float) 2.0, light);
            _pointLight.SetEnergy(katanaUid, (float) 1.0, light);

            var message = _random.Pick(katanaComp.OneBlockMessage);
            _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);

            _actionSystem.AddAction(ownerUid, ref ownerComp.GetMaskActionSoulCuttingEntity, ownerComp.GetMaskSoulCuttingAction);
        }
    }

    private void ActivateKatana(EntityUid katanaUid, SoulCuttingKatanaComponent katanaComp, EntityUid ownerUid)
    {
        katanaComp.IsActive = true;

        _popupSystem.PopupCursor(Loc.GetString("КРУШИ! РУБИ! УБИВАЙ!"), ownerUid, PopupType.Large);

        if (TryComp<MovementSpeedModifierComponent>(ownerUid, out var moveComp))
        {
            katanaComp.OriginalWalkSpeed = moveComp.BaseWalkSpeed;
            katanaComp.OriginalSprintSpeed = moveComp.BaseSprintSpeed;

            var newWalkSpeed = katanaComp.OriginalWalkSpeed * 1.3f; //+ 30%
            var newSprintSpeed = katanaComp.OriginalSprintSpeed * 1.3f; //+ 30%

            _movementSpeedModifierSystem.ChangeBaseSpeed(ownerUid, newWalkSpeed, newSprintSpeed, moveComp.Acceleration);
        }

        if (TryComp<ReflectComponent>(katanaUid, out var reflectComponent))
        {
            reflectComponent.ReflectProb = 0.7f;
        }

        if (TryComp<MeleeWeaponComponent>(katanaUid, out var meleeComp))
        {
            katanaComp.OriginalDamage = meleeComp.Damage;

            meleeComp.AttackRate = 3;
            meleeComp.Damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(4));
        }

        AddComp<UnremoveableComponent>(katanaUid);

        TryComp<PointLightComponent>(katanaUid, out var light);
        _pointLight.SetColor(katanaUid, Color.DarkRed, light);
        _pointLight.SetRadius(katanaUid, (float) 10.0, light);
        _pointLight.SetEnergy(katanaUid, (float) 4.0, light);

        var message = _random.Pick(katanaComp.TwoBlockMessage);
        _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);
    }

    private void DeActivateKatana(EntityUid katanaUid, SoulCuttingKatanaComponent katanaComp, EntityUid ownerUid)
    {
        katanaComp.IsActive = false;

        _popupSystem.PopupCursor(Loc.GetString("Вы чувствуете что сила полученная вам, угасает... Кажется всё закончилось."), ownerUid, PopupType.Large);

        if (TryComp<MovementSpeedModifierComponent>(ownerUid, out var moveMod))
        {
            _movementSpeedModifierSystem.ChangeBaseSpeed(ownerUid, katanaComp.OriginalWalkSpeed, katanaComp.OriginalSprintSpeed, moveMod.Acceleration);
        }

        if (TryComp<MeleeWeaponComponent>(katanaUid, out var meleeComp))
        {
            meleeComp.AttackRate = 1;
            meleeComp.Damage = katanaComp.OriginalDamage;
        }

        if (TryComp<ReflectComponent>(katanaUid, out var reflectComponent))
        {
            reflectComponent.ReflectProb = 0.1f;
        }

        RemComp<UnremoveableComponent>(katanaUid);

        TryComp<PointLightComponent>(katanaUid, out var light);
        _pointLight.SetColor(katanaUid, Color.Red, light);
        _pointLight.SetRadius(katanaUid, (float) 2.0, light);
        _pointLight.SetEnergy(katanaUid, (float) 1.0, light);

        var message = _random.Pick(katanaComp.ThreeBlockMessage);
        _chat.TrySendInGameICMessage(ownerUid, message, InGameICChatType.Speak, true);
    }

    private void OnGetMask(EntityUid ownerUid, SoulCuttingUserComponent ownerComp, GetSoulCuttingMaskEvent args)
    {
        var user = args.Performer;
        var mask = Spawn(ownerComp.SoulСuttingMaskPrototype, Transform(user).Coordinates);
        _hands.TryPickupAnyHand(user, mask);

        _popupSystem.PopupEntity("Дьявольская маска появляется в руках.", user, user);
        _actionSystem.RemoveAction(user, ownerComp.GetMaskActionSoulCuttingEntity);
    }

    private void OnMobStateChanged(EntityUid ownerUid, SoulCuttingUserComponent ownerComp, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Critical && ownerComp.KatanaUid.HasValue)
        {
            var katanaUid = ownerComp.KatanaUid.Value;
            if (TryComp<SoulCuttingKatanaComponent>(katanaUid, out var katanaComp) && katanaComp.IsActive)
            {
                DeActivateKatana(katanaUid, katanaComp, ownerUid);
            }
        }
    }
}