using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
//using Content.Shared.Mindshield.Components;
using Content.Shared.Overlays;
using Content.Shared.PDA;
using Content.Shared.Security.Components;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowSecurityIconsSystem : EquipmentHudSystem<ShowSecurityIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    [ValidatePrototypeId<StatusIconPrototype>]
    private const string JobIconForNoId = "JobIconNoId";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusIconComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, StatusIconComponent _, ref GetStatusIconsEvent @event)
    {
        if (!IsActive || @event.InContainer)
        {
            return;
        }

        var securityIcons = DecideSecurityIcon(uid);

        @event.StatusIcons.AddRange(securityIcons);
    }

    private IReadOnlyList<StatusIconPrototype> DecideSecurityIcon(EntityUid uid)
    {
        var result = new List<StatusIconPrototype>();

        var jobIconToGet = JobIconForNoId;
        if (_accessReader.FindAccessItemsInventory(uid, out var items))
        {
            foreach (var item in items)
            {
                // ID Card
                if (TryComp(item, out IdCardComponent? id))
                {
                    jobIconToGet = id.JobIcon;
                    break;
                }

                // PDA
                if (TryComp(item, out PdaComponent? pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id))
                {
                    jobIconToGet = id.JobIcon;
                    break;
                }
            }
        }

        if (_prototypeMan.TryIndex<StatusIconPrototype>(jobIconToGet, out var jobIcon))
            result.Add(jobIcon);
        else
            Log.Error($"Invalid job icon prototype: {jobIcon}");

        /* A-13 Not allowing SecHUD to see mindshield implants
        //if (TryComp<MindShieldComponent>(uid, out var comp))
        //{
        //    if (_prototypeMan.TryIndex<StatusIconPrototype>(comp.MindShieldStatusIcon.Id, out var icon))
        //        result.Add(icon);
        }*/

        if (TryComp<CriminalRecordComponent>(uid, out var record))
        {
            if(_prototypeMan.TryIndex<StatusIconPrototype>(record.StatusIcon.Id, out var criminalIcon))
                result.Add(criminalIcon);
        }

        return result;
    }
}
