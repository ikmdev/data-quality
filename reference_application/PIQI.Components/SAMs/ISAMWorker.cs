using PIQI.Components.Models;
using PIQI.Components.Services;
using System.Reflection;

namespace PIQI.Components.SAMs;

public interface ISAMWorker
{
    string Mnemonic { get; }
    Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request);

}

