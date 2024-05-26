using Content.Server.Ghost.Roles;
using Content.Shared.Roles;

namespace Content.Server.Andromeda.Roles.GhostRoleTimeTracker;

public sealed class GhostRoleTimeTracker : EntitySystem
{
    private const string UnknownRoleName = "game-ticker-unknown-role";
    private const string GhostRoleTracker = "JobGhostRole";
    private const string GhostRoleProto = "GhostRole";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRoleMarkerRoleComponent, MindGetAllRolesEvent>(OnMindGetAllRoles);
    }

    private void OnMindGetAllRoles(EntityUid uid, GhostRoleMarkerRoleComponent component, ref MindGetAllRolesEvent args)
    {
        string name = component.Name == null ? UnknownRoleName : component.Name;
        args.Roles.Add(new RoleInfo(component, name, false, GhostRoleTracker, GhostRoleProto));
    }
}