using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
namespace DevelopWorkspace.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            DevelopWorkspace.Base.Model.AddinBaseViewModel.ScanAddins(@"C:\Users\husband\Desktop\developWorkspace_prototype\developWorkspace\bin\Debug\addins\");
        }

        [TestMethod]
        public void TestMethod2()
        {
            if (typeof(I).IsAssignableFrom(typeof(A))) {
                System.Diagnostics.Debug.Print("Yes");
            }
        }
        [TestMethod]
        public void TestMethod3()
        {
            DevelopWorkspace.Base.Model.AddinBaseViewModel.GetCacheAddinsData();
        }

    }
    interface I { /* ... */ }
    class A : I { /* ... */ }
    class B : A { /* ... */ }



}
