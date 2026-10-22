using System;using System.Linq;using System.Reflection;using System.Windows.Forms;using System.IO;using EDDiscovery;using EliteDangerousCore.DB;
class Smoke {
 [STAThread]static int Main(){int ticks=0;bool opened=false;Exception failure=null;
 Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
 Application.ThreadException+=(s,e)=>{failure=e.Exception;Console.WriteLine(e.Exception);Application.Exit();};
 using(OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions{EnableHighResolution=false,Backend=OpenTK.PlatformBackend.PreferNative})){
  EliteDangerousCore.EliteConfigInstance.InstanceOptions=EDDOptions.Instance;
  UserDatabase.Instance.Initialize();SystemsDatabase.Instance.Initialize();SystemsDatabase.Instance.SetGridIDs("None");
  UserDatabase.Instance.PutSetting("RouteTrackerCommander_UpdateChecksEnabled",false);
  UserDatabase.Instance.PutSetting("DLLAllowed","+"+Path.Combine(EDDOptions.Instance.DLLAppDirectory(),"RouteTrackerCommander.dll"));
  var timer=new Timer{Interval=1000};timer.Tick+=(s,e)=>{ticks++;var main=Application.OpenForms.OfType<EDDiscoveryForm>().FirstOrDefault();
   if(main!=null&&main.Visible&&!opened){
    if(main.Icon==null || !main.ShowIcon || !main.ShowInTaskbar){failure=new Exception("Taskbar icon not enabled");main.Close();return;} Console.WriteLine("PASS window icon and taskbar visibility enabled"); if(main.DLLManager.Count!=1){failure=new Exception("Plugin did not initialise in actual host");main.Close();return;}
    var tabs=(MajorTabControl)typeof(EDDiscoveryForm).GetField("tabControlMain",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(main);
    int id=EDDConfig.Instance.FindCreatePanelID("RouteTrackerCommander.Native");
    var page=tabs.EnsureMajorTabIsPresent((PanelInformation.PanelIDs)id,true);
    if(page==null||page.Controls.Count==0||page.Controls[0].GetType().Name!="CommanderRouteTracker"){failure=new Exception("Native DLC panel did not open");main.Close();return;}
    var panel=page.Controls[0];var type=panel.GetType().BaseType;
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;
    Action<string,object> set=(name,value)=>type.GetField(name,flags).SetValue(panel,value);
    Func<string,object> get=name=>type.GetField(name,flags|BindingFlags.Public).GetValue(panel);
    var points=new System.Collections.Generic.List<SavedRouteClass.SystemEntry>{new SavedRouteClass.SystemEntry("Sol","",0,0,0),new SavedRouteClass.SystemEntry("Next","",10,0,0),new SavedRouteClass.SystemEntry("End","",20,0,0)};
#if EDD20
    var here=new EliteDangerousCore.SystemClass(0,0,0,"Sol");
#else
    var here=new EliteDangerousCore.SystemClass("Sol",null,0,0,0);
#endif
    set("currentRoute",new SavedRouteClass("Render fixture",points));set("manualTarget",1);set("cur_sys",here);set("routecontrolsettings","");
    type.GetMethod("SetVisibility",flags).Invoke(panel,new object[]{false});
    type.GetMethod("DrawRoute",flags).Invoke(panel,new object[]{here});
    if(!(bool)get("routeRenderValid")||(int)get("routeRenderManual")!=1)throw new Exception("Visible route did not render");
    int before=(int)get("routeClosestCalculations");
    type.GetMethod("DrawRoute",flags).Invoke(panel,new object[]{here});
    if((int)get("routeClosestCalculations")!=before)throw new Exception("Unchanged display recomputed route geometry");
    set("cur_sys",new EliteDangerousCore.SystemClass(10,0,0,"Next"));
    type.GetMethod("CheckManualTarget",flags).Invoke(panel,new object[0]);
    if((int)get("manualTarget")!=2)throw new Exception("Arrival did not advance manual target");
    set("cur_sys",here);set("manualTarget",1);
    Console.WriteLine("PASS manual target arrival on new host API");
    panel.Visible=false;set("manualTarget",2);type.GetMethod("DrawRoute",flags).Invoke(panel,new object[]{here});
    if((bool)get("routeRenderValid"))throw new Exception("Hidden render was not deferred");
    panel.Visible=true;
    if(!(bool)get("routeRenderValid")||(int)get("routeRenderManual")!=2)throw new Exception("Showing panel did not restore current route render");
    var info=PanelInformation.GetPanelInfoByPanelID((PanelInformation.PanelIDs)id);
    if(!Object.ReferenceEquals(info.TabIcon,PanelInformation.PanelTypeIcons[PanelInformation.PanelIDs.RouteTracker]))throw new Exception("Original icon not preserved");
    Console.WriteLine("PASS cached redraw, hidden-panel restoration and original icon");
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
