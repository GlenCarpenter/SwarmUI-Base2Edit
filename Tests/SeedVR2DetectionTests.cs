using SwarmUI.Text2Image;
using Xunit;

namespace Base2Edit.Tests;

[Collection("Base2EditTests")]
public class SeedVR2DetectionTests
{
    [Fact]
    public void SeedVR2Detected_isTrue_onceSeedVR2ParamRegistered()
    {
        WorkflowTestHarness.Base2EditSteps();
        UnitTestStubs.EnsureSeedVR2ModelStubRegistered();

        Assert.True(Base2EditExtension.SeedVR2Detected());
    }

    [Fact]
    public void ApplyEditAfter_offersRefinerAndSeedVR2_whenInstalled()
    {
        WorkflowTestHarness.Base2EditSteps();
        UnitTestStubs.EnsureSeedVR2ModelStubRegistered();

        List<string> values = Base2EditExtension.ApplyEditAfter.Type.GetValues(null);

        Assert.Contains("Refiner", values);
        Assert.Contains("SeedVR2", values);
    }

    [Fact]
    public void OnPreLaunch_revealsApplyEditAfterDropdown_whenSeedVR2Installed()
    {
        WorkflowTestHarness.Base2EditSteps();
        UnitTestStubs.EnsureSeedVR2ModelStubRegistered();

        new Base2EditExtension().OnPreLaunch();

        Assert.True(Base2EditExtension.ApplyEditAfter.Type.VisibleNormally);
    }
}
