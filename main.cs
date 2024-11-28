using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;

public class Program
{
    static void Main(string[] args)
    {
        // MainForm f = new MainForm();
        // f.Show();

        Assembly  asm = Assembly.LoadFrom("Microsoft.Web.WebView2.Core.dll");

        using (var resx = new ResXResourceWriter("test.resx")) {
			resx.AddResource("test", asm);	
			// resx.AddResource("key2", "value2");	
			// resx.AddResource("key3", "value3");
			// ...
		}

        // Assembly exasm = Assembly.GetExecutingAssembly();
        // Stream dllstrm = exasm.GetManifestResourceStream("WebView2Test.Resources.value1");

        // int dlllen = (int)dllstrm.Length;
        // BinaryReader dllbr = new BinaryReader(dllstrm);

        // Console.WriteLine(dlllen);

        // Assembly zipasm = Assembly.Load(dllbr.ReadBytes(dlllen));
        // Type a = zipasm.GetType("Microsoft.Web.WebView2.Core.CoreWebView2");

    }
}

public class MainForm: Form
{
    public MainForm()
    {
        // Assembly exasm = Assembly.GetExecutingAssembly();
        // Stream dllstrm = exasm.GetManifestResourceStream("WebView2Test.Resources.Microsoft.Web.WebView2.Core.dll");

        // int dlllen = (int)dllstrm.Length;
        // BinaryReader dllbr = new BinaryReader(dllstrm);

        // Assembly zipasm = Assembly.Load(dllbr.ReadBytes(dlllen));
        // Type a = zipasm.GetType("Microsoft.Web.WebView2.Core.CoreWebView2");



        //this.webView2.EnsureCoreWebView2Async(null);

    }
}
