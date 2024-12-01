using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

public class Program
{
    [STAThread]
    static void Main()
    {
        AssemblyHelper helper = new AssemblyHelper();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class AssemblyHelper {

    public AssemblyHelper() {
        AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
    }

    private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args) {
        
        string name = new AssemblyName(args.Name).Name;
        Assembly loadedAsm = GetLoadAssembly(name + ".dll");
        return loadedAsm;
    }

    public static string GetEmbeddedString(string name) {

        Assembly asm = Assembly.GetExecutingAssembly();

        using (Stream stream = asm.GetManifestResourceStream(name)) {
            using (StreamReader text = new StreamReader (stream)) {
                return text.ReadToEnd();
            }
        }
    }

    public static void EmbeddedAssemblyCopyTo(string dllName, string path) {
        Assembly asm = Assembly.GetExecutingAssembly();

        using (Stream stream = asm.GetManifestResourceStream(dllName)) {
            using (FileStream fileStream = new FileStream(path, FileMode.Create)) {
                stream.CopyTo(fileStream);
            }
        }
    }

    private static Assembly GetLoadAssembly(string name) {
        Assembly asm = Assembly.GetExecutingAssembly();

        using (Stream stream = asm.GetManifestResourceStream(name)) {
            using (BinaryReader binary = new BinaryReader(stream)) {
                int length = (int)stream.Length;
                Assembly loadAsm = Assembly.Load(binary.ReadBytes(length));
                return loadAsm;
            }
        }

        return null;
    }
}

public class MainForm: Form {

    private dynamic webView2;

    public MainForm() {
        this.Load += MainForm_Load;
        this.BackColor = Color.White;  
    }

    public CoreWebView2Environment environment;

    public async void MainForm_Load(object sender, EventArgs e) {
        
        const string Loaderdll = "WebView2Loader.dll";

        string userfolder = Path.Combine(Path.GetTempPath(), "webv2cache");
        AssemblyHelper.EmbeddedAssemblyCopyTo(Loaderdll, Path.Combine(userfolder, Loaderdll));

        WebView2 webView2 = new WebView2();

        webView2.NavigationCompleted += NavigationCompleted;
        webView2.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;


        CoreWebView2Environment.SetLoaderDllFolderPath(userfolder);
        environment = await CoreWebView2Environment.CreateAsync(null, userfolder);

        await webView2.EnsureCoreWebView2Async(environment);

        webView2.CoreWebView2.WebResourceRequested += WebResourceRequested;

        if (webView2 != null) {

            webView2.Dock = DockStyle.Fill;
            this.Controls.Add(webView2);
            string source = AssemblyHelper.GetEmbeddedString("app.html");
            // webView2.CoreWebView2.NavigateToString(source);
            webView2.CoreWebView2.Navigate(@"app.html");


            // webView2.CoreWebView2.Navigate("https://www.google.co.jp/");
        }
    }
// CoreWebView2NavigationCompletedEventArgs> NavigationCompleted


    public void WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs args) {
        Console.WriteLine(args.Request.Uri);


    }

    public void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args) {

        WebView2 wv = (WebView2)sender;

        Console.WriteLine("test");

        CoreWebView2FileSystemHandle  a = environment.CreateWebFileSystemDirectoryHandle(@"C:\", CoreWebView2FileSystemHandlePermission.ReadOnly);
        // Console.WriteLine(a.Kind);

        List<object> test = new List<object>() { a };

        // wv.CoreWebView2.postMessageWithAdditionalObjects("test", null);
        string jsonMessage = "{ \"action\": \"greet\", \"message\": \"Hello from host!\" }";

        wv.CoreWebView2.PostWebMessageAsJson(jsonMessage, test);


    }

    public void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs args) {

        WebView2 wv = (WebView2)sender;

        Console.WriteLine("bbbbbb");

        // var a = environment.CreateWebFileSystemDirectoryHandle(@"C:\Users\nanas\Downloads\新しいフォルダー (2)", CoreWebView2FileSystemHandlePermission.ReadOnly);

    }
}
