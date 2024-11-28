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

public class EmbeddedWebView2 {

    private class AssemblyTypeSet {
        public string LibraryName { get;set; }
        public string[] TypeNames { get;set; }
    }

    private Dictionary<string, Type> types;

    private const string WebView2CoreDllName = "Microsoft.Web.WebView2.Core.dll";

    private const string webView2Type = "Microsoft.Web.WebView2.WinForms.WebView2";

    public EmbeddedWebView2() {

        AppDomain.CurrentDomain.AssemblyResolve += SearchAssemblyFromDirectories;

        this.types = GetTypeFromResoucesLibraryFileNames(new AssemblyTypeSet[] {

            // new AssemblyTypeSet() {
            //     LibraryName = "Microsoft.Web.WebView2.Core.dll",
            //     TypeNames = new string[] {
            //         "Microsoft.Web.WebView2.Core.CoreWebView2"
            //     }
            // },

            new AssemblyTypeSet() {
                LibraryName = "Microsoft.Web.WebView2.WinForms.dll",
                TypeNames = new string[] {
                    webView2Type,
                }
            }
        });

        Console.WriteLine(types.Count);
        Console.WriteLine(types[webView2Type].ToString());
    }

    public dynamic Create() {

        // if (types[webView2Type] == null) MessageBox.Show("test");
                    // Console.WriteLine("OK");
        dynamic webView2 = Activator.CreateInstance(types[webView2Type]);
        return webView2;
    }

    private static Assembly SearchAssemblyFromDirectories(object sender, ResolveEventArgs e)
    {
        if (e.Name.Contains("Microsoft.Web.WebView2.Core")) return GetLoadAssembly(WebView2CoreDllName);
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
        webView2 = embeddedWebView2.Create();
        webView2.Dock = DockStyle.Fill;
        
        await webView2.EnsureCoreWebView2Async(null);

        this.Controls.Add(webView2);

        webView2.CoreWebView2.Navigate("https://www.google.co.jp/");
    }
}
