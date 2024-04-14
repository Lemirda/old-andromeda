using Robust.Shared.Player;

namespace Content.Server.Andromeda.AdministrationNotifications.GameTicking;

public sealed class AdminLoggedInEvent : EntityEventArgs
{
    public ICommonSession Session { get; }

    public AdminLoggedInEvent(ICommonSession session)
    {
        Session = session;
    }
}