using Robust.Shared.Timing;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [Dependency] private readonly IGameTiming _timing = default!;
    private TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private TimeSpan _moscowTimeThreshold = new TimeSpan(10, 0, 0); // 10:00 МСК
    private int _playerThreshold = 25;
    private string _secretPresetId = "secret";

    private void CheckAndChangeGamePreset()
    {
        var utcNow = DateTime.UtcNow;
        TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        DateTime moscowDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, moscowTimeZone);

        if (_playerManager.PlayerCount >= _playerThreshold || moscowDateTime.TimeOfDay >= _moscowTimeThreshold)
        {
            Log.Info($"В данный момент количество игроков больше или ровно 25, либо время больше 10:00 МСК.");
            if (TryFindGamePreset(_secretPresetId, out var preset))
            {
                Log.Info($"Выставляем {preset} в связи с тем, что в данный момент количество игроков больше или ровно 25, либо время больше 10:00 МСК.");
                SetGamePreset(preset);
            }
            else
            {
                Log.Error($"Не найден режим {_secretPresetId}.");
            }
        }
        else
        {
            Log.Warning($"Невозможно выставить режим в связи с тем, что в данный момент количество игроков меньше 25, либо время меньше 10:00 МСК.");
        }
    }
}