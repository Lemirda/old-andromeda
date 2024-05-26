using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Andromeda.Botany;

/// <summary>
/// This class holds constants that are shared between client and server.
/// </summary>

public sealed class SharedPlantExtractor
{
    public static readonly string BeakerContainerId = "beakerContainer";

    public static readonly string InputContainerId = "inputContainer";

    public static readonly string BufferId = "buffer";
}

public enum PlantExtractorMode
{
    Transfer,
    Discard,
}

public enum PlantExtractorReagentAmount
{
    U1 = 1,
    U5 = 5,
    U10 = 10,
    U25 = 25,
    U33 = 33,
    All,
}

public static class PlantExtractorReagentAmountToFixedPoint
{
    public static FixedPoint2 GetFixedPoint(this PlantExtractorReagentAmount amount)
    {
        if (amount == PlantExtractorReagentAmount.All)
            return FixedPoint2.MaxValue;
        else
            return FixedPoint2.New((int) amount);
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorSetModeMessage : BoundUserInterfaceMessage
{
    public readonly PlantExtractorMode PlantExtractorMode;

    public PlantExtractorSetModeMessage(PlantExtractorMode mode)
    {
        PlantExtractorMode = mode;
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorReagentAmountButtonMessage : BoundUserInterfaceMessage
{
    public readonly ReagentId ReagentId;
    public readonly PlantExtractorReagentAmount Amount;
    public readonly bool FromBuffer;

    public PlantExtractorReagentAmountButtonMessage(ReagentId reagentId, PlantExtractorReagentAmount amount, bool fromBuffer)
    {
        ReagentId = reagentId;
        Amount = amount;
        FromBuffer = fromBuffer;
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorEjectChamberAllMessage : BoundUserInterfaceMessage
{
    public PlantExtractorEjectChamberAllMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorEjectChamberContentMessage : BoundUserInterfaceMessage
{
    public NetEntity EntityId;
    public PlantExtractorEjectChamberContentMessage(NetEntity entityId)
    {
        EntityId = entityId;
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorWorkStartedMessage : BoundUserInterfaceMessage
{
    public PlantExtractorWorkStartedMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorWorkCompletedMessage : BoundUserInterfaceMessage
{
    public PlantExtractorWorkCompletedMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class PlantExtractorStartMessage : BoundUserInterfaceMessage
{
    public PlantExtractorStartMessage()
    {
    }
}

[Serializable, NetSerializable]
public enum PlantExtractorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PlantExtractorBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly ContainerInfo? BeakerContainerInfo;

    /// <summary>
    /// A list of the reagents and their amounts within the buffer, if applicable.
    /// </summary>

    public readonly IReadOnlyList<ReagentQuantity> BufferReagents;
    public readonly PlantExtractorMode Mode;
    public readonly FixedPoint2? BufferCurrentVolume;
    public bool IsBusy;
    public bool CanExtract;
    public NetEntity[] ChamberContent;

    public PlantExtractorBoundUserInterfaceState(
        PlantExtractorMode mode, ContainerInfo? beakerContainerInfo, IReadOnlyList<ReagentQuantity> bufferReagents, FixedPoint2 bufferCurrentVolume, bool isBusy, bool canExtract,
        NetEntity[] chamberContent)
    {
        BeakerContainerInfo = beakerContainerInfo;
        BufferReagents = bufferReagents;
        Mode = mode;
        BufferCurrentVolume = bufferCurrentVolume;
        IsBusy = isBusy;
        CanExtract = canExtract;
        ChamberContent = chamberContent;
    }
}