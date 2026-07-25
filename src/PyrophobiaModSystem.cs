using System;
using Pyrophobia.Behaviors;
using Pyrophobia.Scare;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pyrophobia;

/// <summary>
/// Pyrophobia entry point. F01 attaches in <see cref="AssetsFinalize"/> (D08).
/// F02a scare tick is server-only (D03).
/// </summary>
public class PyrophobiaModSystem : ModSystem
{
    public const string ModId = "pyrophobia";

    private ICoreServerAPI? sapi;
    private BrandishScareService? scare;
    private long scareListenerId;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.RegisterCollectibleBehaviorClass(
            CollectibleBehaviorBrandishTorch.ClassName,
            typeof(CollectibleBehaviorBrandishTorch));
    }

    /// <summary>
    /// Prepend brandish onto lit torches so it runs before <c>CanIgnite</c>.
    /// </summary>
    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        int attached = 0;
        foreach (Block block in api.World.Blocks)
        {
            if (block.Code == null || block.Id == 0)
            {
                continue;
            }

            if (block is not BlockTorch torch || torch.IsExtinct)
            {
                continue;
            }

            if (block.Code.Path.Contains("torch", StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            if (block.HasBehavior<CollectibleBehaviorBrandishTorch>())
            {
                continue;
            }

            CollectibleBehaviorBrandishTorch brandish = new(block);
            CollectibleBehavior[] existing = block.CollectibleBehaviors;
            CollectibleBehavior[] next = new CollectibleBehavior[existing.Length + 1];
            next[0] = brandish;
            Array.Copy(existing, 0, next, 1, existing.Length);
            block.CollectibleBehaviors = next;
            attached++;
        }

        api.Logger.Notification(
            "[{0}] BrandishTorch behavior attached to {1} lit torch block(s) ({2}).",
            ModId,
            attached,
            api.Side);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        api.Logger.Notification("[{0}] client side loaded.", ModId);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        sapi = api;
        scare = new BrandishScareService(api);
        scareListenerId=api.Event.RegisterGameTickListener(scare.OnTick,
            e => api.Logger.Error("[{0}] brandish scare tick failed: {1}", ModId, e),
            BrandishScareService.IntervalMs);
        api.Logger.Notification("[{0}] server side loaded (F02a scare every {1}ms).", ModId, BrandishScareService.IntervalMs);
    }

    public override void Dispose()
    {
        if (sapi != null && scareListenerId != 0)
        {
            sapi.Event.UnregisterGameTickListener(scareListenerId);
            scareListenerId = 0;
        }

        scare = null;
        sapi = null;
        CollectibleBehaviorBrandishTorch.ResetStanceState();
        base.Dispose();
    }
}
