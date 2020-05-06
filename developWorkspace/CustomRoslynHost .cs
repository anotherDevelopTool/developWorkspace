using DevelopWorkspace.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DevelopWorkspace.Main.AppConfig;

namespace DevelopWorkspace.Main
{
    class CustomRoslynHost : RoslynHost
    {
        static CustomRoslynHost _customRoslynHost = null;
        public CustomRoslynHost(NuGetConfiguration nuGetConfiguration = null,
            IEnumerable<Assembly> additionalAssemblies = null,
            RoslynHostReferences references = null) : base(nuGetConfiguration, additionalAssemblies, references)
        {
        }
        public static async  Task<CustomRoslynHost> instance()
        {
            if (_customRoslynHost == null)
            {
                Base.Services.BusyWorkIndicatorService($"load Roslyn's IntelliSense...");

                await Task.Run(() => {
                    List<Assembly> defaultRefList = new List<Assembly>();
                    Assembly[] assemblyList = (from assembly in GetListOfEntryAssemblyWithReferences() where defaultRefList.FirstOrDefault(refname => refname.Equals(System.IO.Path.GetFileName(assembly.Location))) == null select assembly).ToArray();
                    ScriptConfig scriptConfig = JsonConfig<ScriptConfig>.load(StartupSetting.instance.homeDir);
                    //string[] customAssemblyList = (from custom in scriptConfig.Settings.RefAssemblies where checkAssemblyValid(custom) select getAssembly(custom)).ToArray();
                    string[] customAssemblyList = (from custom in scriptConfig.Settings.RefAssemblies select getAssembly(custom)).Where(assemblyPath => !assemblyPath.Equals("")).ToArray();
                    //Todo 并不是任何dll都可以放到roslyn editor里面,目前发现有些dll放到里面时会crash掉，有些会让智能提示功能部分失效
                    //似乎是路径没有指定正确不能正常load导致的问题
                    _customRoslynHost = new CustomRoslynHost(
                        additionalAssemblies: new[]
                            {
                                Assembly.Load("RoslynPad.Roslyn.Windows"),
                                Assembly.Load("RoslynPad.Editor.Windows"),
                                Assembly.LoadFrom(System.IO.Path.Combine(StartupSetting.instance.homeDir,"DevelopWorkspace.Base.dll")),
                                Assembly.LoadFrom(System.IO.Path.Combine(StartupSetting.instance.homeDir,"DevelopWorkspace.exe"))
                            },
                        references: RoslynHostReferences.Default.With(
                            imports: new[] {
                            "System.Windows",
                            "System.Windows.Controls",
                            "System.Windows.Data",
                            "System.Windows.Threading",
                            "System.ComponentModel",
                            "System.Threading",
                            "System.Net.Http"
                            },
                            assemblyReferences: assemblyList,
                            assemblyPathReferences: customAssemblyList
                        )
                    );
                }).ConfigureAwait(false);
            }
            return _customRoslynHost;

        }

        private static string getAssembly(string definedAssemblyName)
        {
            string searchingPath = definedAssemblyName;
            if (Regex.IsMatch(definedAssemblyName, "^[a-z]:", RegexOptions.IgnoreCase))
            {
                if (File.Exists(searchingPath))
                {
                    return searchingPath;
                }
            }
            else
            {
                //优先runtime目录开始查找
                string runtimePath = System.IO.Path.GetDirectoryName(new Uri(Assembly.GetAssembly(typeof(string)).CodeBase).LocalPath);
                searchingPath = System.IO.Path.Combine(runtimePath, definedAssemblyName);
                if (File.Exists(searchingPath)) return searchingPath;
                //其次在执行目录及子目录下查找
                foreach (string searchingDir in StartupSetting.instance.searchDirs) {
                    searchingPath = System.IO.Path.Combine(searchingDir, definedAssemblyName);
                    if (File.Exists(searchingPath)) return searchingPath;
                }
            }
            //防止UI主线程等待instance方法结束前在instance内又发行UI操作造成死锁
            Task.Run(() =>
            {
                DevelopWorkspace.Base.Logger.WriteLine($"{definedAssemblyName} is invalid,please check Settings.RefAssemblies in ScriptConfig.json", Level.WARNING);

            });
            return "";
        }
        private static List<Assembly> GetListOfEntryAssemblyWithReferences()
        {
            List<Assembly> listOfAssemblies = new List<Assembly>();
            var mainAsm = Assembly.GetEntryAssembly();
            listOfAssemblies.Add(mainAsm);

            foreach (var refAsmName in mainAsm.GetReferencedAssemblies())
            {
                listOfAssemblies.Add(Assembly.Load(refAsmName));
            }
            return listOfAssemblies;
        }
        protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project previousProject = null)
        {
            var name = args.Name ?? "Program";
            var id = ProjectId.CreateNewId(name);

            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Script, languageVersion: LanguageVersion.Latest);

            compilationOptions = compilationOptions.WithScriptClassName(name);

            solution = solution.AddProject(ProjectInfo.Create(
                id,
                VersionStamp.Create(),
                name,
                name,
                LanguageNames.CSharp,
                isSubmission: true,
                parseOptions: parseOptions,
//              hostObjectType: typeof(MyClass),
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : DefaultReferences,
                projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null));

            var project = solution.GetProject(id);
            return project;
        }
    }
}
