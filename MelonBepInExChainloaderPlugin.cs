using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using System.Reflection;

[assembly: MelonInfo(typeof(MelonBepInExChainloader.ChainloaderPlugin), "MelonBepInExChainloader", "1.6.0", "Porous")]
[assembly: MelonColor(255, 255, 255, 255)]

namespace MelonBepInExChainloader
{
    public class ChainloaderPlugin : MelonPlugin
    {
        public override void OnApplicationLateStart()
        {
            MelonLogger.Msg("[MelonBepInExChainloader] Preparing BepInEx 6 IL2CPP...");

            try
            {
                
                string bepinDir = Path.Combine(MelonEnvironment.GameRootDirectory, "BepInEx");
                string coreDir = Path.Combine(bepinDir, "core");
                string pluginsDir = Path.Combine(bepinDir, "plugins");
                string configDir = Path.Combine(bepinDir, "config");
                string cacheDir = Path.Combine(bepinDir, "cache");

                Directory.CreateDirectory(bepinDir);
                Directory.CreateDirectory(coreDir);
                Directory.CreateDirectory(pluginsDir);
                Directory.CreateDirectory(configDir);
                Directory.CreateDirectory(cacheDir);

                string il2cppDll = Path.Combine(coreDir, "BepInEx.Unity.IL2CPP.dll");
                string coreDll = Path.Combine(coreDir, "BepInEx.Core.dll");

                if (!File.Exists(il2cppDll) || !File.Exists(coreDll))
                {
                    MelonLogger.Error("[MelonBepInExChainloader] Missing one or more core BepInEx DLLs.");
                    return;
                }

                
                Assembly coreAssembly = Assembly.LoadFrom(coreDll);

                Type pathsType = coreAssembly.GetType("BepInEx.Paths");
                pathsType.GetProperty("GameRootPath").SetValue(null, MelonEnvironment.GameRootDirectory);
                pathsType.GetProperty("BepInExRootPath").SetValue(null, bepinDir);
                pathsType.GetProperty("BepInExConfigPath").SetValue(null, Path.Combine(configDir, "BepInEx.cfg"));
                pathsType.GetProperty("PluginPath").SetValue(null, pluginsDir);
                pathsType.GetProperty("CachePath").SetValue(null, cacheDir);

                
                Assembly il2cppAssembly = Assembly.LoadFrom(il2cppDll);

                Type chainloaderType = il2cppAssembly.GetType("BepInEx.Unity.IL2CPP.IL2CPPChainloader");
                PropertyInfo instanceProp = chainloaderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

                object chainloader = instanceProp.GetValue(null);
                if (chainloader == null)
                {
                    chainloader = Activator.CreateInstance(chainloaderType);
                    instanceProp.SetValue(null, chainloader);
                }

                
                MethodInfo initializeMethod = chainloaderType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo executeMethod = chainloaderType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);

                initializeMethod.Invoke(chainloader, new object[] { MelonEnvironment.GameExecutablePath });
                executeMethod.Invoke(chainloader, null);

                MelonLogger.Msg("[MelonBepInExChainloader] IL2CPPChainloader initialized and executed successfully.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MelonBepInExChainloader] Failed to initialize IL2CPPChainloader!");
                MelonLogger.Error(ex.ToString());
            }
        }
    }
}