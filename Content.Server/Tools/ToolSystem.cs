using Content.Server.Atmos.EntitySystems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups; //A-13 Eblan system update
using Content.Server.Tools.Components;
using Content.Shared.CombatMode.Pacification; //A-13 Eblan system update
using Content.Shared.Tools.Components; //A-13 Eblan system update
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server.Tools
{
    // TODO move tool system to shared, and make it a friend of Tool Component.
    public sealed partial class ToolSystem : SharedToolSystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!; //A-13 Eblan system update
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        private const string WeldingQualityId = "Welding"; //A-13 Eblan system update

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ToolComponent, ToolUseAttemptEvent>(OnToolUseAttempt); //A-13 Eblan system update
            InitializeWelders();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            UpdateWelders(frameTime);
        }

        //A-13 Eblan system update start
        private void OnToolUseAttempt(EntityUid uid, ToolComponent component, ToolUseAttemptEvent args)
        {
            if (component.Qualities.Contains(WeldingQualityId) && EntityManager.HasComponent<EblanComponent>(args.User))
            {
                args.Cancel();
                _popupSystem.PopupCursor(Loc.GetString("Вы наиграли слишком мало времени, вы не можете пользоваться сваркой."), args.User, PopupType.Large);
            }
        }
        //A-13 Eblan system update end

        protected override bool IsWelder(EntityUid uid)
        {
            return HasComp<WelderComponent>(uid);
        }
    }
}
