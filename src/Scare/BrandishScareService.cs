using Pyrophobia.Behaviors;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pyrophobia.Scare;

/// <summary>
/// F02a / D03-D05 / D09-D10: server tick - brandishing players periodically
/// scare hostiles that are actively targeting them via vanilla InstaFleeFrom.
/// (Main thread. Hardcoded knobs until F04)
/// </summary>
internal sealed class BrandishScareService
{
    internal const float Range = 12f;
    internal const float FleeChance = 0.25f;
    internal const int IntervalMs = 1000;

    private readonly ICoreServerAPI sapi;

    public BrandishScareService(ICoreServerAPI sapi)
    {
        this.sapi = sapi;
    }

    public void OnTick(float dt)
    {
        foreach (IPlayer player in sapi.World.AllOnlinePlayers)
        {
            if (player.Entity is not EntityPlayer playerEntity || !playerEntity.Alive)
            {
                continue;
            }

            if (!CollectibleBehaviorBrandishTorch.IsBrandishing(playerEntity))
            {
                continue;
            }

            ScareAround(playerEntity);
        }
    }

    private void ScareAround(EntityPlayer playerEntity)
    {
        long playerId = playerEntity.EntityId;
        Entity[] nearby = sapi.World.GetEntitiesAround(
            playerEntity.Pos.XYZ,
            Range,
            Range,
            e => e is EntityAgent agent && agent.Alive && agent.EntityId != playerId);

        foreach (Entity entity in nearby)
        {
            EntityBehaviorTaskAI? taskAi = entity.GetBehavior<EntityBehaviorTaskAI>();
            if (taskAi == null)
            {
                continue;
            }

            if (!IsAggressivelyTargeting(taskAi.TaskManager, playerId))
            {
                continue;
            }

            if (sapi.World.Rand.NextDouble() >= FleeChance)
            {
                continue;
            }

            TryInstaFlee(taskAi.TaskManager, playerEntity);
        }
    }

    private static bool IsAggressivelyTargeting(AiTaskManager tasks, long playerEntityId)
    {
        foreach (IAiTask? task in tasks.ActiveTasksBySlot)
        {
            if (task is AiTaskBaseTargetable targetable &&
                targetable.AggressiveTargeting &&
                targetable.TargetEntity?.EntityId == playerEntityId)
            {
                return true;
            }

            if (task is AiTaskBaseTargetableR targetableR &&
                targetableR.AggressiveTargeting &&
                targetableR.TargetEntity?.EntityId == playerEntityId)
            {
                return true;
            }
        }

        return false;
    }

    private static void TryInstaFlee(AiTaskManager tasks, Entity playerEntity)
    {
        tasks.GetTask<AiTaskFleeEntity>()?.InstaFleeFrom(playerEntity);
        tasks.GetTask<AiTaskFleeEntityR>()?.InstaFleeFrom(playerEntity);
    }
}
