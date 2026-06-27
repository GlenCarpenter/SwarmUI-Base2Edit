using SwarmUI.Core;
using SwarmUI.Text2Image;
using Xunit;

namespace Base2Edit.Tests;

[Collection("Base2EditTests")]
public class PidUpscaleTests
{
    /// <summary>The "Edit Upscale Method" dropdown must offer PiD decoder models the same way core's
    /// RefinerUpscaleMethod does, by reusing ComfyUIBackendExtension.PidUpscaleModels. Registering a
    /// "pid"-compat model should make it appear as a "pidmodel-&lt;name&gt;" option without dropping the
    /// existing pixel/latent options (we append, not replace).</summary>
    [Fact]
    public void EditUpscaleMethod_dropdown_includes_registered_pid_models()
    {
        using SwarmUiTestContext testContext = new();

        T2IModelHandler sdHandler = new() { ModelType = "Stable-Diffusion" };
        Program.T2IModelSets = new Dictionary<string, T2IModelHandler>
        {
            ["Stable-Diffusion"] = sdHandler
        };

        T2IModelClass pidClass = new()
        {
            ID = "unit-test-pid",
            Name = "UnitTest PiD",
            CompatClass = T2IModelClassSorter.CompatPiD,
            StandardWidth = 1024,
            StandardHeight = 1024
        };
        T2IModel pidModel = new(sdHandler, "/tmp", "/tmp/UnitTest_PiD.safetensors", "UnitTest_PiD.safetensors")
        {
            ModelClass = pidClass
        };
        sdHandler.Models[pidModel.Name] = pidModel;

        List<string> values = Base2EditExtension.EditUpscaleMethod.Type.GetValues(null);

        Assert.Contains(values, v => v.StartsWith("pidmodel-") && v.Contains("UnitTest_PiD"));
        // Existing options remain (the static pixel/latent list is still merged in).
        Assert.Contains(values, v => v.StartsWith("pixel-lanczos"));
        Assert.Contains(values, v => v.StartsWith("latent-bislerp"));
    }

    /// <summary>A registered non-PiD model must NOT appear as a "pidmodel-" option: this proves the
    /// CompatClass=="pid" filter in PidUpscaleModels actually discriminates, rather than the dropdown
    /// merely being empty. A regression that dropped the filter would fail here.</summary>
    [Fact]
    public void EditUpscaleMethod_dropdown_excludes_non_pid_models()
    {
        using SwarmUiTestContext testContext = new();

        T2IModelHandler sdHandler = new() { ModelType = "Stable-Diffusion" };
        Program.T2IModelSets = new Dictionary<string, T2IModelHandler>
        {
            ["Stable-Diffusion"] = sdHandler
        };

        T2IModelClass fluxClass = new()
        {
            ID = "unit-test-flux2",
            Name = "UnitTest Flux2",
            CompatClass = T2IModelClassSorter.CompatFlux2,
            StandardWidth = 1024,
            StandardHeight = 1024
        };
        T2IModel fluxModel = new(sdHandler, "/tmp", "/tmp/UnitTest_Flux2.safetensors", "UnitTest_Flux2.safetensors")
        {
            ModelClass = fluxClass
        };
        sdHandler.Models[fluxModel.Name] = fluxModel;

        List<string> values = Base2EditExtension.EditUpscaleMethod.Type.GetValues(null);

        Assert.DoesNotContain(values, v => v.StartsWith("pidmodel-"));
        Assert.Contains(values, v => v.StartsWith("pixel-lanczos"));
    }
}
