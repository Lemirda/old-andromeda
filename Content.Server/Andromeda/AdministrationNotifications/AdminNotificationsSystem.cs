using System.Net.Http;
using System.Text;
using System.Text.Json;
using Content.Server.Andromeda.AdministrationNotifications.GameTicking;
using Content.Server.Discord;
using Content.Shared.Andromeda.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server.Andromeda.AdministrationNotifications;

public sealed partial class AdminNotificationsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();
    private string _webhookUrl = string.Empty;
    private int _adminCount = 0;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("admin_notifications");
        SubscribeLocalEvent<AdminLoggedInEvent>(OnAdminLoggedIn);
        SubscribeLocalEvent<AdminLoggedOutEvent>(OnAdminLoggedOut);
        InitializePickRiskItems();
        _config.OnValueChanged(AndromedaCCVars.DiscordAdminWebhook, value => _webhookUrl = value, true);
    }

    private async void SendDiscordMessage(WebhookPayload payload)
    {
        var request = await _httpClient.PostAsync(_webhookUrl,
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        _sawmill.Debug($"Вебхук Discord в формате json: {JsonSerializer.Serialize(payload)}");

        var content = await request.Content.ReadAsStringAsync();
        if (!request.IsSuccessStatusCode)
        {
            _sawmill.Error($"Discord вернул неверный код статуса при публикации сообщения: {request.StatusCode}\nResponse: {content}");
            return;
        }
    }

    private void OnAdminLoggedIn(AdminLoggedInEvent e)
    {
        _adminCount++;
        SendAdminStatusUpdate(e.Session, "вошёл", 0x00FF00);
    }

    private void OnAdminLoggedOut(AdminLoggedOutEvent e)
    {
        _adminCount--;
        SendAdminStatusUpdate(e.Session, "вышел", 0xFF0000);
    }

    private void SendAdminStatusUpdate(ICommonSession session, string action, int color)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        var message = $"{session.Name} {action}. Всего администраторов онлайн: {_adminCount}";

        var payload = new WebhookPayload
        {
            Username = "Отчёт входов админов",
            Embeds = new List<WebhookEmbed>
            {
                new()
                {
                    Description = message,
                    Color = color,
                },
            },
        };

        SendDiscordMessage(payload);
    }
}