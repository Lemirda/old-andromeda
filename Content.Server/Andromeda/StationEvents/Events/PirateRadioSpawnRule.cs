using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.RoundEnd;
using System.Linq;
using Robust.Server.Player;

namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly IPlayerManager _playerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var shuttleMap = _mapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };

        _map.TryLoad(shuttleMap, component.PirateRadioShuttlePath, out _, options);
    }

    protected override void Ended(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.AdditionalRule != null)
            GameTicker.EndGameRule(component.AdditionalRule.Value);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent args)
    {
        var informantsQuery = EntityManager.EntityQuery<InformantSindicateComponent>();
        var informantsCount = informantsQuery.Count();

        if (informantsCount == 0)
            return;

        args.AddLine($"Количество информаторов синдиката: {informantsCount}");

        //Log.Info($"Найдено информаторов: {informantsCount}");

        foreach (var informant in informantsQuery)
        {
            //Log.Info($"Обработка информатора с EntityUid: {informant.Owner}");

            if (!EntityManager.TryGetComponent<MetaDataComponent>(informant.Owner, out var metaDataComponent))
            {
                //Log.Error($"MetaDataComponent не найден для EntityUid: {informant.Owner}");
                continue;
            }

            var characterName = metaDataComponent.EntityName;

            if (!_playerSystem.TryGetSessionByEntity(informant.Owner, out var playerSession))
            {
                //Log.Error($"Сессия игрока не найдена для EntityUid: {informant.Owner}");
                continue;
            }

            var playerName = playerSession.Name;
            args.AddLine($"Информатор: {characterName} ({playerName})");

            //Log.Info($"Добавлена информация: Информатор: {characterName} ({playerName})");
        }
    }
}