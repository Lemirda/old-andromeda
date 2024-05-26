using Content.Server.Access.Systems;
using Content.Server.Paper;
using Content.Server.Popups;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server.DeltaV.Paper;

public sealed class SignatureSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    private const string SignatureStampState = "paper_stamp-signature";

    public override void Initialize()
    {
        SubscribeLocalEvent<PaperComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
    }

    private void OnGetAltVerbs(EntityUid uid, PaperComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var pen = args.Using;
        if (pen == null || !_tagSystem.HasTag(pen.Value, "Write"))
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TrySignPaper((uid, component), args.User);
            },
            Text = Loc.GetString("paper-sign-verb"),
            DoContactInteraction = true,
            Priority = 10
        };
        args.Verbs.Add(verb);
    }

    public bool TrySignPaper(Entity<PaperComponent?> paper, EntityUid signer)
    {
        var paperComp = paper.Comp;
        if (!Resolve(paper, ref paperComp))
            return false;

        var signatureName = DetermineEntitySignature(signer);

        var stampInfo = new StampDisplayInfo()
        {
            StampedName = signatureName,
            StampedColor = Color.DarkSlateGray,
        };

        if (!paperComp.StampedBy.Contains(stampInfo) && _paper.TryStamp(paper, stampInfo, SignatureStampState, paperComp))
        {
            var signedSelfMessage = Loc.GetString("paper-signed-self", ("target", paper));
            _popup.PopupEntity(signedSelfMessage, signer, signer);

            _audio.PlayPvs(paperComp.Sound, signer);

            _paper.UpdateUserInterface(paper, paperComp);

            return true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("paper-signed-failure", ("target", paper)), signer, signer, PopupType.SmallCaution);
            return false;
        }
    }

    private string DetermineEntitySignature(EntityUid uid)
    {
        if (_idCard.TryFindIdCard(uid, out var id) && !string.IsNullOrWhiteSpace(id.Comp.FullName))
        {
            return id.Comp.FullName;
        }

        return Name(uid);
    }
}