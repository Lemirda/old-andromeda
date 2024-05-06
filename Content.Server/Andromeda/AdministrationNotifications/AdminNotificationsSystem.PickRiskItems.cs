using Content.Server.Chat.Managers;
using Content.Shared.Interaction;
using Content.Shared.Tag;

namespace Content.Server.Andromeda.AdministrationNotifications;

public sealed partial class AdminNotificationsSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private void InitializePickRiskItems()
    {
        SubscribeLocalEvent<InteractHandEvent>(OnHandInteract);
    }

    private void OnHandInteract(InteractHandEvent ev)
    {
        if (!_tagSystem.HasTag(ev.Target, "HighRiskItem"))
            return;

        _chat.SendAdminAlert($"{ToPrettyString(ev.User):user} взял {ToPrettyString(ev.Target):entity}");
    }
}
