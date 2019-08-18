namespace DevelopWorkspace.Base.Model
{
    using System.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class that shares common properties, methods, and intefaces
    /// among viewmodels that represent documents in Edi
    /// (text file edits, Start Page, Prgram Settings).
    /// </summary>
    public abstract class AddinBaseViewModel : PaneViewModel
    {
        abstract public DataTemplate GetDataTemplate();

        public static AddinBaseViewModel GetViewModel(string addinAssemblyPath, string typeViewModel)
        {
            System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.UnsafeLoadFrom(addinAssemblyPath);
            Type typViewModel = addinAssembly.GetType(typeViewModel);
            System.Reflection.ConstructorInfo ctorViewModel = typViewModel.GetConstructor(Type.EmptyTypes);
            return ctorViewModel.Invoke(new Object[] { }) as AddinBaseViewModel;
        }

        public static void ScanAddins(string addinScanPath)
        {
            var addinAssemblyList = System.IO.Directory.EnumerateFiles(addinScanPath, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
            AddinsCache.AddinsTableDataTable cacheTable = new AddinsCache.AddinsTableDataTable();
            AddinsCache.AddinsTableRow cacheRow;

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
                IEnumerable<Type> allTypes = addinAssembly.GetTypes().Where(t => t != typeof(AddinBaseViewModel) && typeof(AddinBaseViewModel).IsAssignableFrom(t));
                foreach (Type currentType in allTypes)
                {
                    cacheRow = cacheTable.NewAddinsTableRow();
                    cacheTable.Rows.Add(cacheRow);

                    cacheRow.AssemblyLocation = addinAssembly.Location;
                    cacheRow.AssemblyFullName = addinAssembly.FullName;
                    cacheRow.AddinType = currentType.FullName;

                    System.Diagnostics.Debug.Print(addinAssembly.Location);
                    System.Diagnostics.Debug.Print(addinAssembly.FullName);
                    System.Diagnostics.Debug.Print(currentType.FullName);

                    var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(currentType, typeof(AddinMetaAttribute));
                    if (classAttribute != null)
                    {
                        cacheRow.Description = classAttribute.Description;


                        System.Diagnostics.Debug.Print(classAttribute.Name);
                        System.Diagnostics.Debug.Print(classAttribute.Date);
                        System.Diagnostics.Debug.Print(classAttribute.Description);

                    }
                }

            }
            cacheTable.WriteXml(AppDomain.CurrentDomain.BaseDirectory + @"\addin.xml");



        }

        /// <summary>
        /// 如果首次载入时没有缓存文件则生成之
        /// </summary>
        /// <returns></returns>
        public static AddinsCache.AddinsTableDataTable GetCacheAddinsData()
        {
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\addin.xml") == false)
            {
                ScanAddins(AppDomain.CurrentDomain.BaseDirectory + @"\addins\");
            }
            AddinsCache.AddinsTableDataTable cacheTable = new AddinsCache.AddinsTableDataTable();
            cacheTable.ReadXml(AppDomain.CurrentDomain.BaseDirectory + @"\addin.xml");
            return cacheTable;
        }


    }


}
