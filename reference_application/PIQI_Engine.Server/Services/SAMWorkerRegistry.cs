using PIQI.Components.Models;
using PIQI.Components.SAMs;
using PIQI.Components.Services;
using PIQI_Engine.Server.Engines.SAMs;
using System.Collections.Concurrent;
using System.Reflection;

namespace PIQI_Engine.Server.Services;

/// <summary>
/// Maintains a registry of ISAMWorker implementations, allowing dynamic loading from assemblies
/// and retrieval by mnemonic. Supports concurrent access for thread safety.
/// </summary>
public class SAMWorkerRegistry
{
    // Maps mnemonic to a factory that creates an ISAMWorker with dependencies
    private readonly ConcurrentDictionary<string, Func<SAM, SAMService, ISAMWorker>> _workerFactories = new();

    /// <summary>
    /// Registers a worker factory for a specific mnemonic.
    /// </summary>
    public void RegisterFactory(string mnemonic, Func<SAM, SAMService, ISAMWorker> factory)
    {
        _workerFactories[mnemonic] = factory;
    }

    /// <summary>
    /// Loads and registers all ISAMWorker implementations from the specified assembly.
    /// Uses the static StaticMnemonic property for discovery.
    /// </summary>
    /// <param name="assembly">Assembly to scan for ISAMWorker types.</param>
    /// <param name="samService">SAMService instance to pass to workers.</param>
    /// <param name="samResolver">Function to resolve SAM instances by mnemonic.</param>
    public void LoadFromAssembly(Assembly assembly, SAMService samService, Func<string, SAM> samResolver)
    {
        var samTypes = assembly.GetTypes()
            .Where(t => typeof(ISAMWorker).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var type in samTypes)
        {
            // Discover the static property "StaticMnemonic"
            var staticMnemonicProp = type.GetProperty("StaticMnemonic", BindingFlags.Public | BindingFlags.Static);
            if (staticMnemonicProp == null) continue;

            var mnemonic = staticMnemonicProp.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(mnemonic)) continue;

            // Register a factory that creates the worker with dependencies
            _workerFactories[mnemonic] = (sam, samServiceParam) =>
                (ISAMWorker)Activator.CreateInstance(type, sam, samServiceParam)!;
        }
    }

    /// <summary>
    /// Creates a new ISAMWorker instance for the given mnemonic, using the provided SAM and SAMService.
    /// </summary>
    public ISAMWorker CreateWorker(string mnemonic, SAM sam, SAMService samService)
    {
        if (_workerFactories.TryGetValue(mnemonic, out var factory))
            return factory(sam, samService);

        // Use SAM_Default as the fallback
        return new SAM_Default(sam, samService);
    }
}