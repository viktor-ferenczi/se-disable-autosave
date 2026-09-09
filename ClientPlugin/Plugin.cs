using System.Reflection;
using ClientPlugin.Patches;
using HarmonyLib;
using VRage.Plugins;
using VRage.Utils;

// Define assembly version when compiled by Pulsar
#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "DisableAutosave";

    public static Plugin Instance { get; private set; }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        Instance = this;

        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        WorldCopyPatch.Apply(harmony);
    }

    public void Dispose()
    {
        // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.
        Instance = null;
    }

    public void Update()
    {
    }

    public static void Log(string message)
    {
        MyLog.Default?.WriteLineAndConsole($"{Name}: {message}");
    }
}
