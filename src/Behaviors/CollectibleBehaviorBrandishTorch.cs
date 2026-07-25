using System;
using System.Collections.Concurrent;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Pyrophobia.Behaviors;

/// <summary>
/// F01 / D08: brandish a lit main-hand torch (universal, main thread).
/// Prepended ahead of <c>CanIgnite</c>; stance is in-memory, not Attributes.
/// </summary>
public class CollectibleBehaviorBrandishTorch : CollectibleBehavior
{
    public const string ClassName = "BrandishTorch";
    private const string TorchBrandishAnimation = "pyrophobia-brandishtorch";

    // Cleared in ModSystem.Dispose.
    private static readonly ConcurrentDictionary<long, byte> BrandishingByEntityId = new();

    public CollectibleBehaviorBrandishTorch(CollectibleObject collObj)
        : base(collObj)
    {
    }

    internal static void ResetStanceState() => BrandishingByEntityId.Clear();

    /// <summary>Query for F02 scare checks (populated on both sides via use sync).</summary>
    internal static bool IsBrandishing(EntityAgent byEntity) =>
        BrandishingByEntityId.ContainsKey(byEntity.EntityId);

    public override void OnHeldInteractStart(
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handHandling,
        ref EnumHandling handling)
    {
        if (!IsBrandishableTorch(slot, byEntity))
        {
            return;
        }

        if (byEntity.Controls.ShiftKey || blockSel != null)
        {
            SetBrandishing(byEntity, false);
            return;
        }

        SetBrandishing(byEntity, true);
        handHandling = EnumHandHandling.PreventDefault;
        handling = EnumHandling.PreventDefault;
    }

    public override bool OnHeldInteractStep(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        ref EnumHandling handling)
    {
        if (!IsBrandishing(byEntity))
        {
            return false;
        }

        handling = EnumHandling.PreventSubsequent;
        return true;
    }

    public override bool OnHeldInteractCancel(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        EnumItemUseCancelReason cancelReason,
        ref EnumHandling handled)
    {
        if (!IsBrandishing(byEntity))
        {
            return true;
        }

        SetBrandishing(byEntity, false);
        handled = EnumHandling.PreventSubsequent;
        return true;
    }

    public override void OnHeldInteractStop(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        ref EnumHandling handling)
    {
        if (!IsBrandishing(byEntity))
        {
            return;
        }

        SetBrandishing(byEntity, false);
        handling = EnumHandling.PreventSubsequent;
    }

    public override string GetHeldTpUseAnimation(
        ItemSlot activeHotbarSlot,
        Entity forEntity,
        ref EnumHandling bhHandling)
    {
        if (forEntity is EntityAgent agent &&
            IsBrandishing(agent) &&
            IsBrandishableTorch(activeHotbarSlot, forEntity))
        {
            bhHandling = EnumHandling.PreventDefault;
            return TorchBrandishAnimation;
        }

        return null!;
    }

    private bool IsBrandishableTorch(ItemSlot slot, Entity byEntity)
    {
        if (collObj is not BlockTorch torch ||
            torch.IsExtinct ||
            collObj.Code?.Path.Contains("torch", StringComparison.OrdinalIgnoreCase) != true ||
            slot.Itemstack?.Collectible != collObj ||
            byEntity is not EntityPlayer entityPlayer ||
            !ReferenceEquals(slot, entityPlayer.RightHandItemSlot))
        {
            return false;
        }

        return true;
    }

    private static void SetBrandishing(EntityAgent byEntity, bool value)
    {
        if (value)
        {
            BrandishingByEntityId[byEntity.EntityId] = 0;
        }
        else
        {
            BrandishingByEntityId.TryRemove(byEntity.EntityId, out _);
        }
    }
}
