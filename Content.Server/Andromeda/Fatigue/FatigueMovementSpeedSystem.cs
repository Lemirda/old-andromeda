using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Andromeda.Fatigue;

public sealed class FatigueMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public void UpdateMovementSpeed(EntityUid uid, FatigueComponent fatigueComp, MovementSpeedModifierComponent? moveMod = null)
    {
        if (!Resolve(uid, ref moveMod))
            return;

        var fatigueLevel = fatigueComp.CurrentFatigue;

        if (fatigueLevel > 45 && fatigueLevel <= 60)
        {
            var newWalkSpeed = fatigueComp.OriginalWalkSpeed * 0.8f; // 20% снижение скорости
            var newSprintSpeed = fatigueComp.OriginalSprintSpeed * 0.8f; // 20% снижение скорости

            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, newWalkSpeed, newSprintSpeed, moveMod.Acceleration);

            if (!fatigueComp.SpeedReduced)
            {
                fatigueComp.SpeedReduced = true;
            }
        }
        else if (fatigueLevel > 20 && fatigueLevel <= 45)
        {
            var newWalkSpeed = fatigueComp.OriginalWalkSpeed * 0.6f; // 40% снижение скорости
            var newSprintSpeed = fatigueComp.OriginalSprintSpeed * 0.6f; // 40% снижение скорости

            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, newWalkSpeed, newSprintSpeed, moveMod.Acceleration);

            if (!fatigueComp.SpeedReduced)
            {
                fatigueComp.SpeedReduced = true;
            }
        }
        else if (fatigueLevel > 0 && fatigueLevel <= 20)
        {
            var newWalkSpeed = fatigueComp.OriginalWalkSpeed * 0.45f; // 55% снижение скорости
            var newSprintSpeed = fatigueComp.OriginalSprintSpeed * 0.45f; // 55% снижение скорости

            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, newWalkSpeed, newSprintSpeed, moveMod.Acceleration);

            if (!fatigueComp.SpeedReduced)
            {
                fatigueComp.SpeedReduced = true;
            }
        }
        else if (fatigueLevel > 60 || fatigueComp.SpeedReduced == false)
        {
            _movementSpeedModifierSystem.ChangeBaseSpeed(uid, fatigueComp.OriginalWalkSpeed, fatigueComp.OriginalSprintSpeed, moveMod.Acceleration);

            if (fatigueComp.SpeedReduced)
            {
                fatigueComp.SpeedReduced = false;
            }
        }
    }
}