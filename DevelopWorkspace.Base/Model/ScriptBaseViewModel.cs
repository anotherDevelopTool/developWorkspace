namespace DevelopWorkspace.Base.Model
{
    using System.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Reflection;
    using System.IO;
    using Newtonsoft.Json;
    using System.Text;
    using System.Windows.Input;

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
        public static event AddinInstalledEventHandler AddinInstalledEvent;

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
                return System.IO.File.ReadAllText(extfile, Encoding.UTF8);
            }
            else
            {
                return "";
            }
        }
        public string getResPathByExt(string ext)
        {
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
            Uri assemblyUri = new Uri(this.GetType().Assembly.CodeBase);
            string addinDir = Path.GetDirectoryName(assemblyUri.LocalPath);
            string extfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + "." + ext);
            return extfile;
        }
        public void install(string strXaml)
        {
            
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
            File.Copy(this.GetType().Assembly.Location, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".dll"), true);
            string json = JsonConvert.SerializeObject(classAttribute, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".json"), json);
            if (!string.IsNullOrEmpty(strXaml)) System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + ".xaml"), strXaml);
            AddinInstalledEvent(null, new AddinInstalledEventArgs(classAttribute));

        }
        public void saveResByExt(string strXaml,string ext)
        {
            var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(AddinMetaAttribute));
             System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + "." + ext), strXaml, Encoding.UTF8);
        }
        public static ScriptBaseViewModel GetViewModel(string addinAssemblyPath, string typeViewModel)
        {
            System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.UnsafeLoadFrom(addinAssemblyPath);
            Type typViewModel = addinAssembly.GetType(typeViewModel);
            System.Reflection.ConstructorInfo ctorViewModel = typViewModel.GetConstructor(Type.EmptyTypes);
            return ctorViewModel.Invoke(new Object[] { }) as ScriptBaseViewModel;
        }
        public static ScriptBaseViewModel LoadViewModel(AddinMetaAttribute attribute)
        {
            string addinAssemblyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins",attribute.Name + ".dll");
            System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.LoadFrom(addinAssemblyPath);
            var currentType = addinAssembly.GetTypes().Where(t => t != typeof(ScriptBaseViewModel) && typeof(ScriptBaseViewModel).IsAssignableFrom(t)).First();
            System.Reflection.ConstructorInfo ctorViewModel = currentType.GetConstructor(Type.EmptyTypes);
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
        public static List<AddinMetaAttribute> ScanAddinsJson()
        {
            string addinScanPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins");
            var addinJsonList = System.IO.Directory.EnumerateFiles(addinScanPath, "*.json", System.IO.SearchOption.TopDirectoryOnly);
            List<AddinMetaAttribute> listType = new List<AddinMetaAttribute>();

            foreach (string currentJson in addinJsonList)
            {
                string json = File.ReadAllText(currentJson, Encoding.UTF8);
                AddinMetaAttribute attribute = (AddinMetaAttribute)JsonConvert.DeserializeObject(json, typeof(AddinMetaAttribute));
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    listType.Add(attribute);
                }
            }
            return listType;
        }

        RelayCommand _closeCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
                }

                return _closeCommand;
            }
        }

        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            //清除占用资源
            if (this.clearance != null ) clearance(null);
        }

    }


}
