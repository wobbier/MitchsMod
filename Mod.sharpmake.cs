using System.IO;
using Sharpmake;
using System;
using System.Collections.Generic;
using Sharpmake;
using static CommonTarget;

[Generate]
public class SharpmakeProjectBase : CSharpProject
{
    public SharpmakeProjectBase()
        : base(typeof(CommonTarget))
    {
        Name = "SharpmakeProject";
        SourceRootPath = @"./";

        ProjectSchema = CSharpProjectSchema.NetFramework;
        string[] things = { ".xml", ".map", ".config", ".bat", ".txt", ".xsd", ".h.template", ".resx", ".cur" };
        NoneExtensions.Remove(things);
        SourceFilesExtensions = new Strings("Engine.sharpmake.cs");

        ContentExtension.Add("GenerateSolution.bat", "macOS.yml", "Windows.yml");

        SourceFiles.Add(@"[project.SharpmakeCsPath]/Mod.sharpmake.cs");
        //SourceFiles.Add(@"[project.SharpmakeCsPath]/Engine/Tools/HUB/MitchHub.sharpmake.cs");

        DependenciesCopyLocal = DependenciesCopyLocalTypes.None;
        AddTargets(CommonTarget.GetDefaultTargets());
    }

    [Configure]
    public virtual void ConfigureAll(Configuration conf, CommonTarget target)
    {
        conf.Output = Configuration.OutputType.DotNetClassLibrary;
        conf.ProjectFileName = @"[project.Name]_[target.Platform]";
        conf.SolutionFolder = "Tools";

        conf.TargetPath = "$(SolutionDir).build/Sharpmake/[target.Optimization]/";
        conf.ProjectPath = @"[project.SharpmakeCsPath]/.tmp/project/[target.Framework]";
        CSharpProjectExtensions.AddAspNetReferences(conf);
        conf.ReferencesByPath.Add(@"[project.SharpmakeCsPath]/Tools/Sharpmake/Sharpmake.dll");
        conf.ReferencesByPath.Add(@"[project.SharpmakeCsPath]/Tools/Sharpmake/Sharpmake.Generators.dll");
    }
}


public abstract class BaseProject : Project
{
    public BaseProject()
        : base(typeof(CommonTarget))
    {
        Name = "BaseProject";
        SourceRootPath = @"Source";
        IsFileNameToLower = false;
        IsTargetFileNameToLower = false;
        AddTargets(CommonTarget.GetDefaultTargets());
    }

    public static class ConfigurePriorities
    {
        public const int All = -75;
        public const int Platform = -50;
        public const int Optimization = -25;
        /*     SHARPMAKE DEFAULT IS 0     */
        public const int Blobbing = 10;
        public const int BuildSystem = 30;
    }

    [ConfigurePriority(ConfigurePriorities.All)]
    [Configure]
    public virtual void ConfigureAll(Project.Configuration conf, CommonTarget target)
    {

    }

    #region Platfoms

    [ConfigurePriority(ConfigurePriorities.Platform)]
    [Configure(SubPlatformType.Win64)]
    public virtual void ConfigureWin64(Configuration conf, CommonTarget target)
    {

    }

    [ConfigurePriority(ConfigurePriorities.Platform)]
    [Configure(SubPlatformType.UWP)]
    public virtual void ConfigureUWP(Configuration conf, CommonTarget target)
    {

    }

    [ConfigurePriority(ConfigurePriorities.Platform)]
    [Configure(SubPlatformType.macOS)]
    public virtual void ConfigureMac(Configuration conf, CommonTarget target)
    {

    }

    #endregion

    #region Optimizations

    [ConfigurePriority(ConfigurePriorities.Optimization)]
    [Configure(Optimization.Debug)]
    public virtual void ConfigureDebug(Configuration conf, CommonTarget target)
    {
        conf.DefaultOption = Options.DefaultTarget.Debug;

        conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDebugDLL);
    }

    [ConfigurePriority(ConfigurePriorities.Optimization)]
    [Configure(Optimization.Release)]
    public virtual void ConfigureRelease(Configuration conf, CommonTarget target)
    {
        conf.DefaultOption = Options.DefaultTarget.Release;
        conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDLL);
    }

    #endregion
}


[Generate]
public class MitchMod : BaseProject
{
    public MitchMod()
        : base()
    {
        Name = "MitchMod";
    }

    public override void ConfigureAll(Project.Configuration conf, CommonTarget target)
    {
        base.ConfigureAll(conf, target);
        conf.Output = Configuration.OutputType.Lib;
    }
}


public class CommonTarget : Sharpmake.ITarget
{
    public Platform Platform;
    public DevEnv DevEnv;
    public Optimization Optimization;
    public DotNetFramework Framework;
    public DotNetOS DotNetOS;

    [Fragment, Flags]
    public enum SubPlatformType
    {
        Win64 = 1 << 0,
        macOS = 1 << 1,
        UWP   = 1 << 2,
    }
    public SubPlatformType SubPlatform = SubPlatformType.Win64;

    [Fragment, Flags]
    public enum Mode
    {
        Game = 1 << 0,
        Editor = 1 << 1
    }
    public Mode SelectedMode = Mode.Game;

    public CommonTarget() { }

    public CommonTarget(
        Platform platform,
        DevEnv devEnv,
        Optimization optimization,
        DotNetFramework dotNetFramework,
        DotNetOS dotNetOS
    )
    {
        Platform = platform;
        DevEnv = devEnv;
        Optimization = optimization;
        Framework = dotNetFramework;
        DotNetOS = dotNetOS;
    }

    public static CommonTarget[] GetDefaultTargets()
    {
        switch (Util.GetExecutingPlatform())
        {
            case Platform.win64:
                {
                    var baseTarget = new CommonTarget(
                        Platform.win64,
                        DevEnv.vs2019,
                        Optimization.Debug | Optimization.Release,
                        DotNetFramework.v4_8,
                        dotNetOS: 0);
                    baseTarget.SubPlatform = SubPlatformType.Win64;

                    var uwpTarget = new CommonTarget(
                        Platform.win64,
                        DevEnv.vs2019,
                        Optimization.Debug | Optimization.Release,
                        DotNetFramework.v4_8,
                        dotNetOS: 0);
                    uwpTarget.SubPlatform = SubPlatformType.UWP;

                    var editorTarget = new CommonTarget(
                        Platform.win64,
                        DevEnv.vs2019,
                        Optimization.Debug | Optimization.Release,
                        DotNetFramework.v4_8,
                        dotNetOS: 0);
                    editorTarget.SubPlatform = SubPlatformType.Win64;
                    editorTarget.SelectedMode = Mode.Editor;

                    return new[] { baseTarget, uwpTarget, editorTarget };
                }
            case Platform.mac:
                {
                    var macOSTarget = new CommonTarget(
                        Platform.mac,
                        DevEnv.xcode,
                        Optimization.Debug | Optimization.Release,
                        DotNetFramework.v4_8,
                        dotNetOS: 0);
                    macOSTarget.SubPlatform = SubPlatformType.macOS;
                    var macEditor = new CommonTarget(
                        Platform.mac,
                        DevEnv.xcode,
                        Optimization.Debug | Optimization.Release,
                        DotNetFramework.v4_8,
                        dotNetOS: 0);
                    macEditor.SelectedMode = Mode.Editor;
                    macEditor.SubPlatform = SubPlatformType.macOS;

                    return new[] { macOSTarget, macEditor };
                }
            default:
                {
                    throw new NotImplementedException("The platform (" + Util.GetExecutingPlatform() + ") is not currently supported!");
                }
        }
    }

    public override string Name
    {
        get
        {
            if (SelectedMode == Mode.Editor)
            {
                var nameParts = new List<string>
                {
                    SelectedMode.ToString(),
                    Optimization.ToString(),
                };
                return string.Join("_", nameParts);
            }
            else
            {
                var nameParts = new List<string>
                {
                    SelectedMode.ToString(),
                    SubPlatform.ToString(),
                    Optimization.ToString(),
                };
                return string.Join("_", nameParts);
            }
        }
    }

    public string DirectoryName
    {
        get
        {
            var dirNameParts = new List<string>()
            {
                SubPlatform.ToString(),
                Optimization.ToString(),
                SelectedMode.ToString(),
            };

            return string.Join("_", dirNameParts);
        }
    }

    public ITarget ToDefaultDotNetOSTarget()
    {
        return ToSpecificDotNetOSTarget(DotNetOS.Default);
    }

    public ITarget ToSpecificDotNetOSTarget(DotNetOS dotNetOS)
    {
        if (DotNetOS == 0 || DotNetOS == dotNetOS)
            return this;

        return Clone(dotNetOS);
    }
}


[Generate]
public class MitchModSolution : Solution
{
    public MitchModSolution()
        : base(typeof(CommonTarget))
    {
        Name = "MitchsMod";

        AddTargets(CommonTarget.GetDefaultTargets());

        IsFileNameToLower = false;
    }

    [Configure]
    public virtual void ConfigureAll(Solution.Configuration conf, CommonTarget target)
    {
        conf.SolutionPath = @"[solution.SharpmakeCsPath]";
        conf.SolutionFileName = "[solution.Name]";

        conf.AddProject<MitchMod>(target);
        if (target.Platform == Platform.win64)
        {
            conf.AddProject<SharpmakeProjectBase>(target);
        }
    }
}


public static class Main
{
    [Sharpmake.Main]
    public static void SharpmakeMain(Sharpmake.Arguments arguments)
    {
        KitsRootPaths.SetUseKitsRootForDevEnv(DevEnv.vs2019, KitsRootEnum.KitsRoot10, Options.Vc.General.WindowsTargetPlatformVersion.v10_0_19041_0);
        arguments.Generate<MitchModSolution>();
    }
}