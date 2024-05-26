using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Console;

namespace Content.Server.Andromeda.Commands.Helpers;

[AdminCommand(AdminFlags.Admin)]
public sealed class ClearSpentAmmoCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public string Command => "removeusedammo";
    public string Description => "Deletes all cartridges, shells and used bullets";
    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string argsOther, string[] args)
    {
        var deletedCount = 0;
        var query = _entManager.AllEntityQueryEnumerator<CartridgeAmmoComponent>();
        while (query.MoveNext(out var entity, out var comp))
        {
            if (comp.Spent)
            {
                _entManager.QueueDeleteEntity(entity);
                deletedCount++;
            }
        }

        shell.WriteLine($"Deleted {deletedCount} entities.");
    }
}