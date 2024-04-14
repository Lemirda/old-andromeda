using Robust.Shared.Player;

namespace Content.Server.Andromeda.AdministrationNotifications.GameTicking;

public sealed class AdminLoggedOutEvent : EntityEventArgs
{
    public ICommonSession Session { get; }

    public AdminLoggedOutEvent(ICommonSession session)
    {
        Session = session;
    }
}