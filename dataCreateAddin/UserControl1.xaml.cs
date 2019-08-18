using MSHTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DevelopWorkspace.DataCreateAddin
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("iexplore.exe");  //直接打开IE浏览器(打开默认首页)
            //http://www.cnblogs.com/kissdodog/p/3725774.html
            //新建一个Tab，然后打开指定地址
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            object objFlags = 1;
            object objTargetFrameName = "";
            object objPostData = "";
            object objHeaders = "";
            SHDocVw.InternetExplorer webBrowser1 = (SHDocVw.InternetExplorer)shellWindows.Item(shellWindows.Count - 1);
            webBrowser1.Navigate("http://www.baidu.com", ref objFlags, ref objTargetFrameName, ref objPostData, ref objHeaders);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            //遍历所有选项卡
            foreach (SHDocVw.InternetExplorer Browser in shellWindows)
            {
                if (Browser.LocationURL.Contains("www.baidu.com"))
                {
                    IHTMLDocument2 doc2 = (IHTMLDocument2)Browser.Document;
                    IHTMLElementCollection inputs = (IHTMLElementCollection)doc2.all.tags("INPUT");
                    HTMLInputElement input1 = (HTMLInputElement)inputs.item("wd", 0);
                    input1.value = "刘德华";
                    IHTMLElement element2 = (IHTMLElement)inputs.item("su", 0);
                    element2.click();
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


//            //...如果里面有Form，要给里面的text填充信息
//            mshtml.IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)myWeb.Document;
//            mshtml.IHTMLElementCollection inputs;
//            inputs = (mshtml.IHTMLElementCollection)doc2.all.tags("INPUT");
//            mshtml.IHTMLElement element = (mshtml.IHTMLElement)inputs.item("userName", 0);
//            mshtml.IHTMLInputElement inputElement = (mshtml.IHTMLInputElement)element;
//            inputElement.value = "填充信息";

//            //...要点击里面的某个按钮
//            mshtml.IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)myWeb.Document;
//            mshtml.IHTMLElementCollection inputs;
//            inputs = (mshtml.IHTMLElementCollection)doc2.all.tags("INPUT");
//            mshtml.IHTMLElement element = (mshtml.IHTMLElement)inputs.item("SubmitBut", 0);
//            element.click();

//            1、根据元素ID获取元素的值。
//比如要获取 < img class="" id="regimg" src="/register/checkregcode.html?1287068791" width="80" height="22">这个标签里的src属性的值：
//mshtml.IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)webBrowser1.Document;
//        mshtml.IHTMLElement img = (mshtml.IHTMLElement)doc2.all.item("regimg", 0);
//        string imgUrl = (string)img.getAttribute("src");
//2、填写表单，并确定
//mshtml.IHTMLElement loginname = (mshtml.IHTMLElement)doc2.all.item("loginname", 0);
//        mshtml.IHTMLElement loginPW = (mshtml.IHTMLElement)doc2.all.item("password", 0);
//        mshtml.IHTMLElement loginBT = (mshtml.IHTMLElement)doc2.all.item("formsubmit", 0);
//        mshtml.IHTMLElement loginYZ = (mshtml.IHTMLElement)doc2.all.item("regcode", 0);
//        loginname.setAttribute("value", tbLoginName.Text);
//    loginPW.setAttribute("value", tbLoginPassWord.Password);
//    loginYZ.setAttribute("value", tbYZ.Text);
//    loginBT.click();


            //下面的代码在百度里面已经失效，因为JQUERY不用了
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            //遍历所有选项卡
            //遍历所有选项卡
            foreach (SHDocVw.InternetExplorer Browser in shellWindows)
            {
                if (Browser.LocationURL.Contains("www.baidu.com"))
                {
                    //通过操作js点击按钮
                    if (Browser.Document is IHTMLDocument2)
                    {
                        IHTMLDocument2 doc2 = Browser.Document as IHTMLDocument2;
                        HTMLScriptElement script = (HTMLScriptElement)doc2.createElement("script");
                        //script.text = "alert(123);";
                        //恰好百度用了jQuery
                        //script.text = "$(\"#wd\").val('刘德华'); $(\"#su\").click();";
                        script.text = "alert(0);";

                        HTMLBody body = doc2.body as HTMLBody;
                        body.appendChild((IHTMLDOMNode)script);
                    }
                }
            }
        }
    }
}
