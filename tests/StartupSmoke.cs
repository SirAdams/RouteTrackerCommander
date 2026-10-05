using System;using System.Linq;using System.Reflection;using System.Windows.Forms;using System.IO;using EDDiscovery;using EliteDangerousCore.DB;
class Smoke {
 [STAThread]static int Main(){int ticks=0;bool opened=false;Exception failure=null;
 Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
 Application.ThreadException+=(s,e)=>{failure=e.Exception;Console.WriteLine(e.Exception);Application.Exit();};
 using(OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions{EnableHighResolution=false,Backend=OpenTK.PlatformBackend.PreferNative})){
  EliteDangerousCore.EliteConfigInstance.InstanceOptions=EDDOptions.Instance;
  UserDatabase.Instance.Initialize();SystemsDatabase.Instance.Initialize();SystemsDatabase.Instance.SetGridIDs("None");
  UserDatabase.Instance.PutSetting("DLLAllowed","+"+Path.Combine(EDDOptions.Instance.DLLAppDirectory(),"RouteTrackerCommander.dll"));
  var timer=new Timer{Interval=1000};timer.Tick+=(s,e)=>{ticks++;var main=Application.OpenForms.OfType<EDDiscoveryForm>().FirstOrDefault();
   if(main!=null&&main.Visible&&!opened){
    if(main.DLLManager.Count!=1){failure=new Exception("Plugin did not initialise in actual host");main.Close();return;}
    var tabs=(MajorTabControl)typeof(EDDiscoveryForm).GetField("tabControlMain",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(main);
    int id=EDDConfig.Instance.FindCreatePanelID("RouteTrackerCommander.Native");
    var page=tabs.EnsureMajorTabIsPresent((PanelInformation.PanelIDs)id,true);
    if(page==null||page.Controls.Count==0||page.Controls[0].GetType().Name!="CommanderRouteTracker"){failure=new Exception("Native DLC panel did not open");main.Close();return;}
    opened=true;Console.WriteLine("PASS actual DLL init and native panel display");
   }
   if(main!=null&&opened&&ticks>12)main.Close();
   if(ticks>45){failure=new Exception("Startup/shutdown timed out");Application.Exit();}
  };timer.Start();
  try{var t=typeof(EDDiscoveryForm).Assembly.GetType("EDDiscovery.EDDApplicationContext");Application.Run((ApplicationContext)Activator.CreateInstance(t,true));}
  catch(Exception ex){failure=ex;Console.WriteLine(ex);}finally{timer.Stop();UserDatabase.Instance.Stop();SystemsDatabase.Instance.Stop();}
 }
 Console.WriteLine(opened&&failure==null?"PASS host startup, DLC panel and shutdown":"FAIL host smoke test");return opened&&failure==null?0:1;
 }
}
