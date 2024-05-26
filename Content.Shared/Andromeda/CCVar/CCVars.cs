using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.Andromeda.CCVar;

// ReSharper disable once InconsistentNaming
[CVarDefs]
public sealed class AndromedaCCVars : CVars
{
    /*
     * Server
     */


    /*
     * Game
     */


    /*
     * Discord
     */

    /// <summary>
    /// URL of the Discord webhook which will show bans in game.
    /// </summary>
    public static readonly CVarDef<string> DiscordBanWebhook =
        CVarDef.Create("discord.ban_webhook", string.Empty, CVar.SERVERONLY);

    public static readonly CVarDef<string> DiscordAdminWebhook =
        CVarDef.Create("discord.admin_webhook", string.Empty, CVar.SERVERONLY);
}