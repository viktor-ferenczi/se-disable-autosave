using System;
using HarmonyLib;
using Sandbox.Engine.Networking;
using VRage.Game;

namespace ClientPlugin.Patches;

/// <summary>
///     The Save As command of the Load World screen produces a new world save by copying the
///     save folder on disk, then loading the checkpoint of the copy, amending it and writing
///     it back. It never goes through <see cref="Sandbox.Game.World.MySession" />, so it needs
///     its own patch.
///     The copying runs on a worker task in MyGuiScreenSaveAs.SaveResult.SaveAsync. That method
///     is private in a private nested class, so it is patched by name instead of by attribute,
///     and a missing target only disables this one code path.
/// </summary>
public static class WorldCopyPatch
{
    private const string SaveAsyncMethod = "Sandbox.Game.Screens.MyGuiScreenSaveAs+SaveResult:SaveAsync";

    // Set only while the worker task of the Save As command is copying a world
    [ThreadStatic] private static bool copyingWorld;

    public static void Apply(Harmony harmony)
    {
        var saveAsync = AccessTools.Method(SaveAsyncMethod);
        if (saveAsync == null)
        {
            Plugin.Log($"Could not find {SaveAsyncMethod}, Save As in the Load World screen is not covered");
            return;
        }

        harmony.Patch(saveAsync,
            prefix: new HarmonyMethod(typeof(WorldCopyPatch), nameof(SaveAsyncPrefix)),
            finalizer: new HarmonyMethod(typeof(WorldCopyPatch), nameof(SaveAsyncFinalizer)));
    }

    public static void SaveAsyncPrefix()
    {
        copyingWorld = true;
    }

    public static void SaveAsyncFinalizer()
    {
        copyingWorld = false;
    }

    private static void DisableAutosave(MyObjectBuilder_Checkpoint checkpoint)
    {
        if (!copyingWorld)
            return;

        var settings = checkpoint?.Settings;
        if (settings == null || settings.AutoSaveInMinutes == 0)
            return;

        settings.AutoSaveInMinutes = 0;
        Plugin.Log($"Disabled autosave in the world save copied as: {checkpoint.SessionName}");
    }

    /// <summary>
    ///     Disables autosave in the checkpoint the Save As command has just loaded from the copy,
    ///     before that checkpoint is written back to the new save folder.
    /// </summary>
    [HarmonyPatch(typeof(MyLocalCache))]
    public static class MyLocalCachePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MyLocalCache.LoadCheckpoint))]
        // ReSharper disable once InconsistentNaming
        public static void LoadCheckpointPostfix(MyObjectBuilder_Checkpoint __result)
        {
            DisableAutosave(__result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MyLocalCache.LoadCheckpointFromCloud))]
        // ReSharper disable once InconsistentNaming
        public static void LoadCheckpointFromCloudPostfix(MyObjectBuilder_Checkpoint __result)
        {
            DisableAutosave(__result);
        }
    }
}
