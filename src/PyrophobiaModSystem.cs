using HarmonyLib;
using Pyrophobia.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Pyrophobia;

/// <summary>
/// Entry point for the mod.
/// </summary>
public class PyrophobiaModSystem : ModSystem
{
    public const string ModId = "pyrophobia";

    // -------------- Shared --------------

    private Harmony? harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        harmony = new Harmony(ModId);
        TorchBrandishPatches.Apply(harmony, api.Logger);
    }

    // -------------- Client --------------

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        api.Logger.Notification("[{0}] client side loaded.", ModId);
    }

    // -------------- Server --------------

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        api.Logger.Notification("[{0}] server side loaded.", ModId);
    }

    // -------------- Cleanup --------------

    public override void Dispose()
    {
        if (harmony != null)
        {
            harmony.UnpatchAll(ModId);
            harmony = null;
        }

        TorchBrandishPatches.Reset();
        base.Dispose();
    }
}
