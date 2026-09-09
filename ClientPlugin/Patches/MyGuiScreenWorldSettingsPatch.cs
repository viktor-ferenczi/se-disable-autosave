using System;
using HarmonyLib;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;

namespace ClientPlugin.Patches;

/// <summary>
///     Keeps the Autosave option off in the world settings screen, which is used both by the
///     New World screen and by the Edit Settings command of the Load World screen.
///     The checkbox itself is unchecked and disabled, so the screen does not show a setting
///     the plugin is going to override anyway.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenWorldSettings))]
public static class MyGuiScreenWorldSettingsPatch
{
    private static readonly AccessTools.FieldRef<MyGuiScreenWorldSettings, MyGuiControlCheckbox> AutoSaveCheckbox =
        ResolveAutoSaveCheckbox();

    private static AccessTools.FieldRef<MyGuiScreenWorldSettings, MyGuiControlCheckbox> ResolveAutoSaveCheckbox()
    {
        try
        {
            return AccessTools.FieldRefAccess<MyGuiScreenWorldSettings, MyGuiControlCheckbox>("m_autoSave");
        }
        catch (Exception e)
        {
            Plugin.Log($"Could not access the Autosave checkbox, it will stay enabled on the screen: {e.Message}");
            return null;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("GetSettingsFromControls")]
    // ReSharper disable once InconsistentNaming
    public static void GetSettingsFromControlsPostfix(MyGuiScreenWorldSettings __instance, bool __result)
    {
        if (!__result)
            return;

        var settings = __instance.Settings;
        if (settings == null || settings.AutoSaveInMinutes == 0)
            return;

        settings.AutoSaveInMinutes = 0;
        Plugin.Log("Disabled autosave in the world settings");
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetSettingsToControls")]
    // ReSharper disable once InconsistentNaming
    public static void SetSettingsToControlsPostfix(MyGuiScreenWorldSettings __instance)
    {
        var checkbox = AutoSaveCheckbox?.Invoke(__instance);
        if (checkbox == null)
            return;

        checkbox.IsChecked = false;
        checkbox.Enabled = false;
        checkbox.SetToolTip($"Autosave is kept disabled by the {Plugin.Name} plugin.");
    }
}
