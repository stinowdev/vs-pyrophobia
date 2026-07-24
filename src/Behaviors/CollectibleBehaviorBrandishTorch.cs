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

    /// <summary>
    /// PreventDefault marks the use as handled so the caller sets
    /// <c>Controls.HandUse</c>. With a block targeted or Shift held we
    /// PassThrough, which keeps placement, relighting, and CanIgnite vanilla.
    /// </summary>
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

    /// <summary>
    /// Keep HandUse active for the hold duration, even when aim later crosses
    /// a block. PreventSubsequent stops CanIgnite from treating that as ignite.
    /// </summary>
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

    /// <summary>
    /// Accept cancellation (release / swap) and ease the stance out.
    /// </summary>
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

    /// <summary>
    /// Brandishing produces no effect on stop. PreventSubsequent keeps
    /// CanIgnite's stop handler from starting fires after a long hold that
    /// began as a brandish and later aimed at a block.
    /// </summary>
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

    /// <summary>
    /// Only active brandishes get the stance animation; vanilla uses such as
    /// the ignite gesture keep their own animation.
    /// </summary>
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

    /// <summary>
    /// Lit main-hand torch only. Slot identity keeps mirrored calls for other
    /// slots or entities on their vanilla path.
    /// </summary>
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

    private static bool IsBrandishing(EntityAgent byEntity) =>
        BrandishingByEntityId.ContainsKey(byEntity.EntityId);

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
