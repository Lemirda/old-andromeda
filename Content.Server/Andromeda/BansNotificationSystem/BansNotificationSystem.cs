using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Content.Shared.Andromeda.CCVar;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server.Andromeda.BansNotifications;

/// <summary>
/// Listen game events and send notifications to Discord
/// </summary>

/*
public interface IBansNotificationsSystem
{
    void RaiseLocalBanEvent(string username, DateTimeOffset? expires, string reason, NoteSeverity severity, string adminusername);
    void RaiseLocalJobBanEvent(string username, DateTimeOffset? expires, JobPrototype job, string reason, NoteSeverity severity, string adminusername);
    void RaiseLocalDepartmentBanEvent(string username, DateTimeOffset? expires, DepartmentPrototype department, string reason, NoteSeverity severity, string adminusername);
}
*/

public sealed class BansNotificationsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();
    private string _webhookUrl = string.Empty;
    private string _serverName = string.Empty;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("bans_notifications");
        SubscribeLocalEvent<BanEvent>(OnBan);
        SubscribeLocalEvent<JobBanEvent>(OnJobBan);
        SubscribeLocalEvent<DepartmentBanEvent>(OnDepartmentBan);
        _config.OnValueChanged(AndromedaCCVars.DiscordBanWebhook, value => _webhookUrl = value, true);
        _config.OnValueChanged(CVars.GameHostName, value => _serverName = value, true);
    }

    /*
    public void RaiseLocalBanEvent(string username, DateTimeOffset? expires, string reason, NoteSeverity severity, string adminusername)
    {
        RaiseLocalEvent(new BanEvent(username, expires, reason, severity, adminusername));
    }

    public void RaiseLocalJobBanEvent(string username, DateTimeOffset? expires, JobPrototype job, string reason, NoteSeverity severity, string adminusername)
    {
        RaiseLocalEvent(new JobBanEvent(username, expires, job, reason, severity, adminusername));
    }

    public void RaiseLocalDepartmentBanEvent(string username, DateTimeOffset? expires, DepartmentPrototype department, string reason, NoteSeverity severity, string adminusername)
    {
        RaiseLocalEvent(new DepartmentBanEvent(username, expires, department, reason, severity, adminusername));
    }
    */

    private async void SendDiscordMessage(WebhookPayload payload)
    {
        var request = await _httpClient.PostAsync(_webhookUrl,
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        _sawmill.Debug($"Discord webhook json: {JsonSerializer.Serialize(payload)}");

        var content = await request.Content.ReadAsStringAsync();
        if (!request.IsSuccessStatusCode)
        {
            _sawmill.Error($"Discord returned bad status code when posting message: {request.StatusCode}\nResponse: {content}");
            return;
        }
    }

    public void OnBan(BanEvent e)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        var expires = e.Expires == null ? Loc.GetString("discord-permanent") : Loc.GetString("discord-expires-at", ("date", e.Expires));
        var message = Loc.GetString("discord-ban-msg",
            ("username", e.Username),
            ("expires", expires),
            ("reason", e.Reason));

        var color = e.Severity switch
        {
            NoteSeverity.None => 0x6aa84f,
            NoteSeverity.Minor => 0x45818e,
            NoteSeverity.Medium => 0xf1c232,
            NoteSeverity.High => 0xff0000,
            _ => 0xff0000,
        };

        var payload = new WebhookPayload
        {

            Username = _serverName,
            //AvatarUrl = string.IsNullOrWhiteSpace(_avatarUrl) ? null : _avatarUrl,
            Embeds = new List<Embed>
            {
                new()
                {
                    Description = message,
                    Color = color,
                    Footer = new EmbedFooter
                    {
                        Text = $"{e.AdminUsername}",
                        //IconUrl = string.IsNullOrWhiteSpace(_footerIconUrl) ? null : _footerIconUrl
                    },
                },
            },
        };

        SendDiscordMessage(payload);
    }

    public void OnJobBan(JobBanEvent e)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        var expires = e.Expires == null ? Loc.GetString("discord-permanent") : Loc.GetString("discord-expires-at", ("date", e.Expires));
        var message = Loc.GetString("discord-jobban-msg",
            ("username", e.Username),
            ("role", e.Job.LocalizedName),
            ("expires", expires),
            ("reason", e.Reason));


        var color = e.Severity switch
        {
            NoteSeverity.None => 0x6aa84f,
            NoteSeverity.Minor => 0x45818e,
            NoteSeverity.Medium => 0xf1c232,
            NoteSeverity.High => 0xff0000,
            _ => 0xff0000,
        };

        var payload = new WebhookPayload
        {
            Username = _serverName,
            //AvatarUrl = string.IsNullOrWhiteSpace(_avatarUrl) ? null : _avatarUrl,
            Embeds = new List<Embed>
            {
                new()
                {
                    Description = message,
                    Color = color,
                    Footer = new EmbedFooter
                    {
                        Text = $"{e.AdminUsername}",
                        //IconUrl = string.IsNullOrWhiteSpace(_footerIconUrl) ? null : _footerIconUrl
                    },
                },
            },
        };

        SendDiscordMessage(payload);
    }

    public void OnDepartmentBan(DepartmentBanEvent e)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        /*
        var payload = new WebhookPayload();
        var departamentLocName = Loc.GetString(string.Concat("department-", e.Department.ID));
        var expires = e.Expires == null ? Loc.GetString("discord-permanent") : Loc.GetString("discord-expires-at", ("date", e.Expires));
        var text = Loc.GetString("discord-departmentban-msg",
            ("username", e.Username),
            ("department", departamentLocName),
            ("expires", expires),
            ("reason", e.Reason));

        payload.Content = text;
        SendDiscordMessage(payload);
        */
    }

    private struct WebhookPayload
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; } = "";

        [JsonPropertyName("embeds")]
        public List<Embed>? Embeds { get; set; } = null;

        [JsonPropertyName("allowed_mentions")]
        public Dictionary<string, string[]> AllowedMentions { get; set; } =
            new()
            {
                { "parse", Array.Empty<string>() },
            };

        public WebhookPayload()
        {
        }
    }

    private struct Embed
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("color")]
        public int Color { get; set; } = 0;

        [JsonPropertyName("footer")]
        public EmbedFooter? Footer { get; set; } = null;

        public Embed()
        {
        }
    }

    private struct EmbedFooter
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";

        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        public EmbedFooter()
        {
        }
    }

    private struct WebhookData
    {
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; } = null;

        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; } = null;

        public WebhookData()
        {
        }
    }
}