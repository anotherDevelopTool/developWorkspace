namespace DevelopWorkspace.Base.Model
{
    using System.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Reflection;
    using System.IO;

    /// <summary>
    /// Base class that shares common properties, methods, and intefaces
    /// among viewmodels that represent documents in Edi
    /// (text file edits, Start Page, Prgram Settings).
    /// </summary>
    public abstract class ScriptBaseViewModel : PaneViewModel
    {
        
        abstract public UserControl getView(string strXaml);
        //abstract public UserControl getView();
        //abstract public void install(string strXaml);
        //
        public UserControl getView()
        {
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
            Uri assemblyUri = new Uri(this.GetType().Assembly.CodeBase);
            string addinDir = Path.GetDirectoryName(assemblyUri.LocalPath);
            string xamlfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".xaml");
            if (File.Exists(xamlfile))
            {
                return getView(System.IO.File.ReadAllText(xamlfile));
            }
            else {
                return new UserControl();
            }
        }
        public string getResByExt(string ext)
        {
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
            Uri assemblyUri = new Uri(this.GetType().Assembly.CodeBase);
            string addinDir = Path.GetDirectoryName(assemblyUri.LocalPath);
            string extfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + "." + ext);
            if (File.Exists(extfile))
            {
                return System.IO.File.ReadAllText(extfile);
            }
            else
            {
                return "";
            }
        }
        public void install(string strXaml)
        {
            
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
            System.IO.File.Copy(this.GetType().Assembly.Location, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".dll"), true);
            if (!string.IsNullOrEmpty(strXaml)) System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".xaml"), strXaml);
        }
        public void saveResByExt(string strXaml,string ext)
        {
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
             System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + "." + ext), strXaml);
        }
        public static ScriptBaseViewModel GetViewModel(string addinAssemblyPath, string typeViewModel)
        {
            System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.UnsafeLoadFrom(addinAssemblyPath);
            Type typViewModel = addinAssembly.GetType(typeViewModel);
            System.Reflection.ConstructorInfo ctorViewModel = typViewModel.GetConstructor(Type.EmptyTypes);
            return ctorViewModel.Invoke(new Object[] { }) as ScriptBaseViewModel;
        }

        public static List<ScriptBaseViewModel> ScanAddins()
        {
            string addinScanPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"addins");
            var addinAssemblyList = System.IO.Directory.EnumerateFiles(addinScanPath, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
            List<ScriptBaseViewModel> listType = new List<ScriptBaseViewModel>();

            foreach (string currentAssembly in addinAssemblyList)
            {
                #region 注意事项
                //如果使用LOADFROM会导致同一个ASM多次被载入会造成Type信息获取出现问题
                //Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                //Load() and LoadFile() loads assembiles into different contexts. 
                //Bug Fix 如果指定目录内同一个ASM出现多次的话，会带来灾难，这块日后需要优化
                #endregion

                //cache

                if (currentAssembly.IndexOf("DevelopWorkspace.Base.dll") != -1) continue;
                System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.LoadFrom(currentAssembly);
                IEnumerable<Type> allTypes = addinAssembly.GetTypes().Where(t => t != typeof(ScriptBaseViewModel) && typeof(ScriptBaseViewModel).IsAssignableFrom(t));
                foreach (Type currentType in allTypes)
                {
                    System.Reflection.ConstructorInfo ctorViewModel = currentType.GetConstructor(Type.EmptyTypes);
                    listType.Add(ctorViewModel.Invoke(new Object[] { }) as ScriptBaseViewModel);
                }

            }
            return listType;
        }



    }


}
