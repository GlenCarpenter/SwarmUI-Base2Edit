using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.WebAPI;

namespace Base2Edit.Tests;

internal static class UnitTestStubs
{
    /// <summary>Stub standing in for the SeedVR2 sister extension's "SeedVR2 Model" param, so Base2Edit's
    /// SeedVR2 detection (which looks up the param by name) can be exercised without that extension loaded.</summary>
    public static T2IRegisteredParam<string> SeedVR2ModelStub;

    public static void EnsureSeedVR2ModelStubRegistered()
    {
        if (SeedVR2ModelStub is not null)
        {
            return;
        }

        SeedVR2ModelStub = T2IParamTypes.Register<string>(new T2IParamType(
            Name: Base2EditExtension.SeedVR2ModelParamName,
            Description: "Stub param registered only for unit tests.",
            Default: "seedvr2-auto",
            FeatureFlag: "seedvr2_upscaler",
            GetValues: (_) => ["seedvr2-auto"]
        ));
    }

    public static void EnsureComfySetClipDeviceRegistered()
    {
        if (ComfyUIBackendExtension.SetClipDevice is not null)
        {
            return;
        }

        ComfyUIBackendExtension.SetClipDevice = T2IParamTypes.Register<string>(new T2IParamType(
            Name: "Set CLIP Device (UnitTest Stub)",
            Description: "Stub param registered only for unit tests.",
            Default: "cpu",
            FeatureFlag: "set_clip_device",
            Group: T2IParamTypes.GroupAdvancedModelAddons,
            IsAdvanced: true,
            Toggleable: true,
            GetValues: (_) => ["cpu"]
        ));
    }

    public static void EnsureComfySamplerSchedulerRegistered()
    {
        if (ComfyUIBackendExtension.SamplerParam is not null
            && ComfyUIBackendExtension.SchedulerParam is not null
            && ComfyUIBackendExtension.RefinerSamplerParam is not null
            && ComfyUIBackendExtension.RefinerSchedulerParam is not null)
        {
            return;
        }

        ComfyUIBackendExtension.SamplerParam ??= T2IParamTypes.Register<string>(new T2IParamType(
            Name: "Sampler (UnitTest Stub)",
            Description: "Stub param registered only for unit tests.",
            Default: "euler",
            FeatureFlag: "comfyui",
            Group: T2IParamTypes.GroupSampling,
            Toggleable: true,
            GetValues: (_) => ["euler", "dpmpp_2m"]
        ));

        ComfyUIBackendExtension.SchedulerParam ??= T2IParamTypes.Register<string>(new T2IParamType(
            Name: "Scheduler (UnitTest Stub)",
            Description: "Stub param registered only for unit tests.",
            Default: "normal",
            FeatureFlag: "comfyui",
            Group: T2IParamTypes.GroupSampling,
            Toggleable: true,
            GetValues: (_) => ["normal", "karras"]
        ));

        ComfyUIBackendExtension.RefinerSamplerParam ??= T2IParamTypes.Register<string>(new T2IParamType(
            Name: "Refiner Sampler (UnitTest Stub)",
            Description: "Stub param registered only for unit tests.",
            Default: "euler",
            FeatureFlag: "comfyui",
            Group: T2IParamTypes.GroupSampling,
            Toggleable: true,
            GetValues: (_) => ["euler", "dpmpp_2m"]
        ));

        ComfyUIBackendExtension.RefinerSchedulerParam ??= T2IParamTypes.Register<string>(new T2IParamType(
            Name: "Refiner Scheduler (UnitTest Stub)",
            Description: "Stub param registered only for unit tests.",
            Default: "normal",
            FeatureFlag: "comfyui",
            Group: T2IParamTypes.GroupSampling,
            Toggleable: true,
            GetValues: (_) => ["normal", "karras"]
        ));
    }
}

internal sealed class SwarmUiTestContext : IDisposable
{
    private readonly Dictionary<string, T2IModelHandler> _priorModelSets;
    private readonly bool _priorIncludeHash;
    private readonly List<WorkflowGenerator.WorkflowGenStep> _priorModelGenSteps;
    private readonly ConcurrentDictionary<string, Func<string, Dictionary<string, JObject>>> _priorExtraModelProviders;

    public SwarmUiTestContext(
        bool disableImageMetadataModelHash = true,
        bool resetExtraModelProviders = true,
        bool clearModelGenSteps = true
    )
    {
        _priorModelSets = Program.T2IModelSets;
        _priorIncludeHash = Program.ServerSettings.Metadata.ImageMetadataIncludeModelHash;
        _priorModelGenSteps = [.. WorkflowGenerator.ModelGenSteps];
        _priorExtraModelProviders = ModelsAPI.ExtraModelProviders;

        if (disableImageMetadataModelHash)
        {
            Program.ServerSettings.Metadata.ImageMetadataIncludeModelHash = false;
        }

        if (resetExtraModelProviders)
        {
            ModelsAPI.ExtraModelProviders = new ConcurrentDictionary<string, Func<string, Dictionary<string, JObject>>>(
                [
                    new KeyValuePair<string, Func<string, Dictionary<string, JObject>>>("unit_test", _ => new Dictionary<string, JObject>())
                ]);
        }

        if (clearModelGenSteps)
        {
            WorkflowGenerator.ModelGenSteps = [];
        }
    }

    public void Dispose()
    {
        WorkflowGenerator.ModelGenSteps = _priorModelGenSteps;
        ModelsAPI.ExtraModelProviders = _priorExtraModelProviders;
        Program.T2IModelSets = _priorModelSets;
        Program.ServerSettings.Metadata.ImageMetadataIncludeModelHash = _priorIncludeHash;
    }
}
