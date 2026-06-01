using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Text2Image;
using Xunit;

namespace Base2Edit.Tests;

/// <summary>
/// Covers per-stage &lt;param&gt; overrides: section-scoped values (as core writes them when a
/// &lt;param[..]&gt; tag appears inside an &lt;edit&gt; / &lt;edit[n]&gt; prompt section) and the
/// precedence chain own-&lt;param&gt; > card > inherited-&lt;param&gt; > default, with parent-to-child
/// bubble-down and no child-to-parent bubble-up.
/// </summary>
[Collection("Base2EditTests")]
public class Base2EditParamOverrideTests
{
    private static T2IParamInput BuildBaseInput()
    {
        WorkflowTestHarness.Base2EditSteps();
        T2IParamInput input = new(null);
        input.Set(T2IParamTypes.Prompt, "test");
        input.Set(Base2EditExtension.ApplyEditAfter, "Base");
        input.Set(T2IParamTypes.Seed, 1L);
        input.Set(T2IParamTypes.Width, 512);
        input.Set(T2IParamTypes.Height, 512);
        input.Set(Base2EditExtension.EditModel, ModelPrep.UseBase);
        return input;
    }

    private static WorkflowGenerator MakeGenerator(T2IParamInput input) => new()
    {
        UserInput = input,
        Features = [],
        ModelFolderFormat = "/"
    };

    private static int GlobalEditSection => Base2EditExtension.SectionID_Edit;
    private static int StageSection(int stageId) => Base2EditExtension.EditSectionIdForStage(stageId);

    private static List<StageSpec> Parse(T2IParamInput input) =>
        Base2EditSpecParser.Parse(MakeGenerator(input));

    private static StageSpec Stage(List<StageSpec> stages, int id) => stages.Single(s => s.Id == id);

    // Tier 1 beats tier 2: a stage's own <param> overrides that stage's explicit card field.
    [Fact]
    public void OwnParam_overrides_cardField()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\",\"Steps\":\"30\"}]");
        input.Set(Base2EditExtension.EditSteps, 50, StageSection(1));

        Assert.Equal(50, Stage(Parse(input), 1).Steps);
    }

    // Tier 2 still applies when there is no <param>: the card field is used as before.
    [Fact]
    public void NoParam_usesCardField()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\",\"Steps\":\"30\"}]");

        Assert.Equal(30, Stage(Parse(input), 1).Steps);
    }

    // Tier 3: a global <edit> <param> applies to a stage that has no card field of its own.
    [Fact]
    public void GlobalEditParam_appliesToStageWithoutCardField()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");
        input.Set(Base2EditExtension.EditSteps, 50, GlobalEditSection);

        Assert.Equal(50, Stage(Parse(input), 1).Steps);
    }

    // Tier 2 beats tier 3: an explicit per-stage card field beats the global <edit> default.
    [Fact]
    public void CardField_beatsGlobalEditParam()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\",\"Steps\":\"30\"}]");
        input.Set(Base2EditExtension.EditSteps, 50, GlobalEditSection);

        Assert.Equal(30, Stage(Parse(input), 1).Steps);
    }

    // Tier 4: no override and no card field falls back to the existing default.
    [Fact]
    public void NoOverride_fallsBackToDefault()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");

        Assert.Equal(20, Stage(Parse(input), 1).Steps);
    }

    // Bubble-down: a parent edit stage's <param> is inherited by a child that sets nothing itself.
    [Fact]
    public void ParentParam_bubblesDownToChild()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"},"
            + "{\"ApplyAfter\":\"Edit Stage 1\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");
        input.Set(Base2EditExtension.EditSteps, 40, StageSection(1));

        List<StageSpec> stages = Parse(input);
        Assert.Equal(40, Stage(stages, 1).Steps);
        Assert.Equal(40, Stage(stages, 2).Steps);
    }

    // A child's own <param> overrides the value it would otherwise inherit from its parent.
    [Fact]
    public void ChildParam_overridesInheritedParentParam()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"},"
            + "{\"ApplyAfter\":\"Edit Stage 1\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");
        input.Set(Base2EditExtension.EditSteps, 40, StageSection(1));
        input.Set(Base2EditExtension.EditSteps, 60, StageSection(2));

        List<StageSpec> stages = Parse(input);
        Assert.Equal(40, Stage(stages, 1).Steps);
        Assert.Equal(60, Stage(stages, 2).Steps);
    }

    // No bubble-up: a child's <param> never leaks to its parent.
    [Fact]
    public void ChildParam_doesNotBubbleUpToParent()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"},"
            + "{\"ApplyAfter\":\"Edit Stage 1\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");
        input.Set(Base2EditExtension.EditSteps, 60, StageSection(2));

        List<StageSpec> stages = Parse(input);
        Assert.Equal(20, Stage(stages, 1).Steps);
        Assert.Equal(60, Stage(stages, 2).Steps);
    }

    // A sibling never reads another stage's section.
    [Fact]
    public void Param_doesNotLeakToSibling()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"},"
            + "{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\"}]");
        input.Set(Base2EditExtension.EditSteps, 77, StageSection(1));

        List<StageSpec> stages = Parse(input);
        Assert.Equal(77, Stage(stages, 1).Steps);
        Assert.Equal(20, Stage(stages, 2).Steps);
    }

    // editmodel via <param> overrides the card Model and resolves through ModelPrep.
    [Fact]
    public void OwnEditModelParam_overridesCardModel()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseRefiner + "\"}]");
        input.Set(Base2EditExtension.EditModel, ModelPrep.UseBase, StageSection(1));

        StageSpec stage1 = Stage(Parse(input), 1);
        Assert.Equal(ModelSource.Base, stage1.ModelSource);
    }

    // A non-int param (CFG, double) routes through the same precedence and normalization.
    [Fact]
    public void OwnCfgScaleParam_overridesCardField_andNormalizes()
    {
        using SwarmUiTestContext _ = new();
        T2IParamInput input = BuildBaseInput();
        input.Set(Base2EditExtension.EditStages,
            "[{\"ApplyAfter\":\"Base\",\"Model\":\"" + ModelPrep.UseBase + "\",\"CfgScale\":\"7\"}]");
        input.Set(Base2EditExtension.EditCFGScale, 3.55, StageSection(1));

        Assert.Equal(3.5, Stage(Parse(input), 1).CfgScale);
    }
}
