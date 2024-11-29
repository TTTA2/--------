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

public class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

using System.Runtime.InteropServices;

public abstract class BaseDllClass : IDisposable
{
    [DllImport("kernel32")]
    private static extern IntPtr LoadLibrary(string dllFilePath);

    [DllImport("kernel32")]
    private static extern IntPtr GetProcAddress(IntPtr module, string functionName);

    [DllImport("kernel32")]
    private static extern bool FreeLibrary(IntPtr module);

    private IntPtr _dllPtr;

    public BaseDllClass(string dllFilePath)
    {
        if (!File.Exists(dllFilePath))
            throw new Exception("Not found dll.");
        // DLLのロード
        _dllPtr = LoadLibrary(dllFilePath);
        if (_dllPtr == IntPtr.Zero)
            throw new Exception("Failed load dll.");
    }

    protected T LoadFunction<T>(string functionName)
    {
        // 関数ポインタを取得
        IntPtr functionPtr = GetProcAddress(_dllPtr, functionName);
　　　　 if (functionPtr == IntPtr.Zero)
            throw new Exception("function not found.");

        // 関数ポインタをdelegateに変換する
        return (T)(object)Marshal.GetDelegateForFunctionPointer(functionPtr, typeof(T));
    }
    
    public void Dispose()
    {
        FreeLibrary(_dllPtr);
    }
}

public class EmbeddedWebView2 {

    private class AssemblyTypeSet {
        public string LibraryName { get;set; }
        public string[] TypeNames { get;set; }
    }

    private Dictionary<string, Type> types;

    private const string WebView2CoreAssemblyName = "Microsoft.Web.WebView2.Core";
    private const string WebView2CoreDllName = "Microsoft.Web.WebView2.Core.dll";

    private const string webView2CoreEnvironmentClass = "Microsoft.Web.WebView2.Core.CoreWebView2Environment";
    private const string webView2CoreEnvironmentOptionClass = "Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions";
    private const string webView2Type = "Microsoft.Web.WebView2.WinForms.WebView2";

    private const string webView2CoreWebView2CreationPropertiesClass = "Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties";

    public EmbeddedWebView2() {

        AppDomain.CurrentDomain.AssemblyResolve += SearchAssemblyFromDirectories;

        this.types = GetTypeFromResoucesLibraryFileNames(new AssemblyTypeSet[] {
            new AssemblyTypeSet() {
                LibraryName = WebView2CoreDllName,
                TypeNames = new string[] {
                    webView2CoreEnvironmentClass,
                    webView2CoreEnvironmentOptionClass,
                }
            },
            new AssemblyTypeSet() {
                LibraryName = "Microsoft.Web.WebView2.WinForms.dll",
                TypeNames = new string[] {
                    webView2Type,
                    webView2CoreWebView2CreationPropertiesClass,
                }
            }
        });
    }

    public async Task<dynamic> CreateEnvironment() {

        string userfolder = Path.Combine(Path.GetTempPath(), "webv2cache");

        // MethodInfo createAsync = 
        //     types[webView2CoreEnvironmentClass].GetMethod(
        //         "CreateAsync", 
        //         null,
        //         BindingFlags.Static | BindingFlags.Public, new Type[] {
        //         typeof(string),
        //         },
        //         null
        //     );

        Type e = types[webView2CoreEnvironmentOptionClass];

        MethodInfo createAsync = 
            types[webView2CoreEnvironmentClass].GetMethod(
                "CreateAsync", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string), e }, null );

       if (createAsync != null) {

            var parameters = createAsync.GetParameters();

            Task environment = (Task)(createAsync.Invoke(null, new object[] { null, null, parameters[2].DefaultValue }));
            await environment;
            dynamic property = environment.GetType().GetProperty("Result");
            dynamic value = property.GetValue(environment);

            return value;
       }

       return null;
    }

    public dynamic CreateProp() {
        string userfolder = Path.Combine(Path.GetTempPath(), "webv2cache");
        dynamic prop = Activator.CreateInstance(types[webView2CoreWebView2CreationPropertiesClass]);
        prop.UserDataFolder = userfolder;
        return prop;
    }
    
    public async Task<dynamic> Create() {

        // MethodInfo createAsync = 
            // types[webView2CoreEnvironmentClass].GetMethod(
                // "CreateAsync", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string), e }, null );

        // MethodInfo constr = types[webView2CoreEnvironmentClass].GetConstructor(new Type[]);

        // Console.WriteLine(constr);

        // dynamic env = await CreateEnvironment();

        
        MethodInfo SetLoaderDllFolderPath = types[webView2CoreEnvironmentClass].GetMethod(
        "SetLoaderDllFolderPath", BindingFlags.Static | BindingFlags.Public);

        dynamic env = await CreateEnvironment();

        dynamic webView2 = Activator.CreateInstance(types[webView2Type]);
        webView2.CreationProperties = this.CreateProp();



        await webView2.EnsureCoreWebView2Async(env);

        SetLoaderDllFolderPath.Invoke(null, new object[] { @"C:\Users\nanas\Downloads\新しいフォルダー (2)" });

        return webView2;

        // dynamic webView2Environment = await Activator.CreateInstance(types[webView2CoreEnvironmentType])
        // webView2Environment.method("CreateAsync", null, userfolder);
	    // object ret = method.Invoke(obj, new string[] { "abc", "def" });
                        //    .CoreWebView2Environment.CreateAsync(null, userfolder);
    }

    private static Assembly SearchAssemblyFromDirectories(object sender, ResolveEventArgs e) {
        if (e.Name.Contains(WebView2CoreAssemblyName)) return GetLoadAssembly(WebView2CoreDllName);
        Console.WriteLine("Search:" + e.Name);
        return null;
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

    private Dictionary<string, Type> GetTypeFromResoucesLibraryFileNames(AssemblyTypeSet[] typeSets) {

        Assembly asm = Assembly.GetExecutingAssembly();
        Dictionary<string, Type> typeDic = new Dictionary<string, Type>();

        for (int i = 0; i < typeSets.Length; i++) {

            AssemblyTypeSet typeset = typeSets[i];

            using (Stream stream = asm.GetManifestResourceStream(typeset.LibraryName)) {
                using (BinaryReader binary = new BinaryReader(stream)) {

                    int length = (int)stream.Length;
                    Assembly loadAsm = Assembly.Load(binary.ReadBytes(length));
                    Module module = loadAsm.GetModule(typeset.LibraryName);

                    foreach (string name in typeset.TypeNames) {
                        typeDic.Add(name, loadAsm.GetType(name));
                    }
                }
            }
        }

        return typeDic;
    }

}

public class MainForm: Form {

    private dynamic webView2;

    public MainForm() {
        this.Load += MainForm_Load;
        this.BackColor = Color.White;  
    }

    public async void MainForm_Load(object sender, EventArgs e) {        

        EmbeddedWebView2 embeddedWebView2 = new EmbeddedWebView2();
        webView2 = await embeddedWebView2.Create();

        if (webView2 != null) {

            webView2.Dock = DockStyle.Fill;
            this.Controls.Add(webView2);
            webView2.CoreWebView2.Navigate("https://www.google.co.jp/");
        }

        Console.WriteLine(webView2 != null);
    }
}
