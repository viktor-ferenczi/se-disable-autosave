using System.IO;
using HarmonyLib;
using Sandbox.Game.World;
using VRage.Game;

namespace ClientPlugin.Patches;

/// <summary>
///     Turns the Autosave option off while the checkpoint of a world save is being assembled,
///     but only when that save folder does not exist yet. That is exactly the moment a new
///     world save comes into existence.
///     Covers new worlds started from classic content, custom worlds, procedural worlds,
///     scenarios, campaigns and missions, as well as the in-game "Save As" command, which
///     always targets a save folder which does not exist yet. Saving a world which already
///     has its save folder is left alone.
/// </summary>
[HarmonyPatch(typeof(MySession), nameof(MySession.GetCheckpoint), typeof(string), typeof(bool))]
public static class MySessionPatch
{
    // ReSharper disable once InconsistentNaming
    public static void Postfix(MySession __instance, MyObjectBuilder_Checkpoint __result)
    {
        var settings = __result?.Settings;
        if (settings == null || settings.AutoSaveInMinutes == 0)
            return;

        var path = __instance.CurrentPath;
        if (string.IsNullOrEmpty(path) || Directory.Exists(path))
            return;

        settings.AutoSaveInMinutes = 0;

        // GetCheckpoint works on a clone of the session settings,
        // so stop the running session from autosaving as well
        if (__instance.Settings != null)
            __instance.Settings.AutoSaveInMinutes = 0;

        Plugin.Log($"Disabled autosave in new world save: {path}");
    }
}
