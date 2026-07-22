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

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
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
        base.Dispose();
    }
}
