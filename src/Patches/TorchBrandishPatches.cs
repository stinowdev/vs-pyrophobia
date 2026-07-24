using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

//Note: This class is heavily commented because I had trouble working
//with animations and Harmony. So I'm leaving the comments in for
//future reference.

namespace Pyrophobia.Patches;

/// <summary>
/// F01 / D07: holding right-click with a lit main-hand torch and no block
/// targeted raises the torch into a brandish stance.
/// The stance animation comes from
/// <see cref="CollectibleObject.GetHeldTpUseAnimation"/>: while a use action
/// is active, <c>EntityPlayer.HandleSeraphHandAnimations</c> feeds its result
/// to <c>PlayerAnimationManager</c>, which appends <c>-fp</c> in first person.
/// Both animation variants are registered by <c>player-brandish-animation.json</c>.
/// </summary>
internal static class TorchBrandishPatches
{
    private const string TorchBrandishAnimation = "pyrophobia-brandishtorch";
    private const string BrandishingAttribute = "pyrophobia-brandishing";

    private static readonly object Gate = new object();
    private static bool applied;

    internal static void Apply(Harmony harmony, ILogger logger)
    {
        lock (Gate)
        {
            if (applied) return;
            applied = true;

            Patch(harmony, logger, typeof(BlockTorch), nameof(BlockTorch.OnHeldInteractStart),
                new[]
                {
                    typeof(ItemSlot), typeof(EntityAgent), typeof(BlockSelection),
                    typeof(EntitySelection), typeof(bool), typeof(EnumHandHandling).MakeByRefType()
                }, nameof(OnHeldInteractStartPrefix));

            Patch(harmony, logger, typeof(BlockTorch), nameof(BlockTorch.OnHeldInteractStep),
                new[]
                {
                    typeof(float), typeof(ItemSlot), typeof(EntityAgent),
                    typeof(BlockSelection), typeof(EntitySelection)
                }, nameof(OnHeldInteractStepPrefix));

            Patch(harmony, logger, typeof(CollectibleObject), nameof(CollectibleObject.OnHeldInteractCancel),
                new[]
                {
                    typeof(float), typeof(ItemSlot), typeof(EntityAgent),
                    typeof(BlockSelection), typeof(EntitySelection), typeof(EnumItemUseCancelReason)
                }, nameof(OnHeldInteractCancelPrefix));

            Patch(harmony, logger, typeof(CollectibleObject), nameof(CollectibleObject.OnHeldInteractStop),
                new[]
                {
                    typeof(float), typeof(ItemSlot), typeof(EntityAgent),
                    typeof(BlockSelection), typeof(EntitySelection)
                }, nameof(OnHeldInteractStopPrefix));

            MethodInfo? useAnimation = AccessTools.Method(
                typeof(CollectibleObject),
                nameof(CollectibleObject.GetHeldTpUseAnimation),
                new[] { typeof(ItemSlot), typeof(Entity) });
            MethodInfo? useAnimationPostfix = AccessTools.Method(
                typeof(TorchBrandishPatches),
                nameof(GetHeldTpUseAnimationPostfix));

            if (useAnimation == null || useAnimationPostfix == null)
            {
                throw new MissingMethodException(
                    $"[{PyrophobiaModSystem.ModId}] Could not resolve CollectibleObject.GetHeldTpUseAnimation.");
            }

            harmony.Patch(useAnimation, postfix: new HarmonyMethod(useAnimationPostfix));
            logger.Debug("[{0}] Patched torch brandishing lifecycle and held-use animation selection.",
                PyrophobiaModSystem.ModId);
        }
    }

    internal static void Reset()
    {
        lock (Gate) applied = false;
    }

    private static void Patch(
        Harmony harmony,
        ILogger logger,
        Type targetType,
        string methodName,
        Type[] argumentTypes,
        string prefixName)
    {
        MethodInfo? original = AccessTools.Method(targetType, methodName, argumentTypes);
        MethodInfo? prefix = AccessTools.Method(typeof(TorchBrandishPatches), prefixName);

        if (original == null || prefix == null)
        {
            throw new MissingMethodException(
                $"[{PyrophobiaModSystem.ModId}] Could not resolve patch target {methodName} or prefix {prefixName}.");
        }

        harmony.Patch(original, prefix: new HarmonyMethod(prefix));
    }

    /// <summary>
    /// PreventDefault marks the use as handled, so the caller sets
    /// <c>Controls.HandUse</c> and the use lifecycle begins. With a block
    /// targeted or Shift held the original runs untouched, which keeps
    /// placement, relighting, and the ignite gesture vanilla.
    /// </summary>
    private static bool OnHeldInteractStartPrefix(
        CollectibleObject __instance,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        ref EnumHandHandling handling)
    {
        if (!IsBrandishableTorch(__instance, slot, byEntity))
        {
            return true;
        }

        if (byEntity.Controls.ShiftKey || blockSel != null)
        {
            SetBrandishing(byEntity, false);
            return true;
        }

        SetBrandishing(byEntity, true);
        handling = EnumHandHandling.PreventDefault;
        return false;
    }

    /// <summary>
    /// Returning true keeps <c>Controls.HandUse</c> active, so the stance
    /// holds for as long as the mouse button stays down, even when the aim
    /// later crosses a block.
    /// </summary>
    private static bool OnHeldInteractStepPrefix(
        CollectibleObject __instance,
        ItemSlot slot,
        EntityAgent byEntity,
        ref bool __result)
    {
        if (!IsBrandishing(byEntity))
        {
            return true;
        }

        __result = true;
        return false;
    }

    /// <summary>
    /// Returning true accepts the cancellation (released mouse, swapped
    /// slots), which clears <c>Controls.HandUse</c> and eases the stance out.
    /// </summary>
    private static bool OnHeldInteractCancelPrefix(
        CollectibleObject __instance,
        ItemSlot slot,
        EntityAgent byEntity,
        ref bool __result)
    {
        if (!IsBrandishing(byEntity))
        {
            return true;
        }

        SetBrandishing(byEntity, false);
        __result = true;
        return false;
    }

    /// <summary>
    /// Brandishing produces no effect on stop. Skipping the original also
    /// keeps <c>CanIgnite</c>'s stop handler (which starts fires after a
    /// three second hold) away from brandish releases.
    /// </summary>
    private static bool OnHeldInteractStopPrefix(
        CollectibleObject __instance,
        ItemSlot slot,
        EntityAgent byEntity)
    {
        if (!IsBrandishing(byEntity))
        {
            return true;
        }

        SetBrandishing(byEntity, false);
        return false;
    }

    /// <summary>
    /// Consulted every frame while a use action is active or right-click is
    /// held. Only active brandishes get the stance animation; vanilla uses
    /// such as the ignite gesture keep their own animation.
    /// </summary>
    private static void GetHeldTpUseAnimationPostfix(
        CollectibleObject __instance,
        ItemSlot activeHotbarSlot,
        Entity forEntity,
        ref string __result)
    {
        if (forEntity is EntityAgent agent &&
            IsBrandishing(agent) &&
            IsBrandishableTorch(__instance, activeHotbarSlot, forEntity))
        {
            __result = TorchBrandishAnimation;
        }
    }

    /// <summary>
    /// Brandishable means a lit (not extinct or burned-out) torch held in the
    /// player's main hand. The slot identity check keeps mirrored calls for
    /// other slots or entities on their vanilla path.
    /// </summary>
    private static bool IsBrandishableTorch(
        CollectibleObject collectible,
        ItemSlot slot,
        Entity byEntity)
    {
        if (collectible is not BlockTorch torch ||
            torch.IsExtinct ||
            collectible.Code?.Path.Contains("torch", StringComparison.OrdinalIgnoreCase) != true ||
            slot.Itemstack?.Collectible != collectible ||
            byEntity is not EntityPlayer entityPlayer ||
            !ReferenceEquals(slot, entityPlayer.RightHandItemSlot))
        {
            return false;
        }

        return true;
    }

    private static bool IsBrandishing(EntityAgent byEntity)
    {
        return byEntity.Attributes.GetBool(BrandishingAttribute);
    }

    private static void SetBrandishing(EntityAgent byEntity, bool value)
    {
        byEntity.Attributes.SetBool(BrandishingAttribute, value);
    }
}
