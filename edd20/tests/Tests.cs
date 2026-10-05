using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using EDDiscovery.UserControls;
using EliteDangerousCore.DB;
class Tests {
 static int checks;
 static void Assert(bool value,string reason){checks++;if(!value)throw new Exception(reason);}
 [STAThread] static void Main(string[] args) {
 var dependencies=args.Length>0?Path.GetFullPath(args[0]):AppDomain.CurrentDomain.BaseDirectory;
 AppDomain.CurrentDomain.AssemblyResolve+=(sender,e)=>{var n=new AssemblyName(e.Name).Name;var p=Path.Combine(dependencies,n+(n=="EDDiscovery"?".exe":".dll"));return File.Exists(p)?Assembly.LoadFrom(p):null;};
 try {Run();} catch(Exception ex){Console.WriteLine(ex);Environment.ExitCode=1;} finally {Cleanup();} }
 static void Cleanup(){UserDatabase.Instance.Stop();}
 static readonly BindingFlags hidden=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Field(object p,string name){return typeof(CommanderSurveyorPanel).GetField(name,hidden).GetValue(p);}
 static void Call(object p,string name,params object[] args){typeof(CommanderSurveyorPanel).GetMethod(name,hidden).Invoke(p,args);}
 static void SetField(object p,string name,object value){typeof(CommanderSurveyorPanel).GetField(name,hidden).SetValue(p,value);}
 static void StartupRegression(){
 // Bind the real host callback without constructing/showing a main window or calling its logic.
 var host=(EDDiscovery.EDDiscoveryForm)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(EDDiscovery.EDDiscoveryForm));
 System.GC.SuppressFinalize(host);
 Assert(System.Windows.Forms.Application.OpenForms.Count==0,"Regression needs startup without open windows");
 foreach(var name in new[]{"RequestHistory","GetShipLoadout"}) {
  var field=typeof(EDDDLLInterfaces.EDDDLLIF.EDDCallBacks).GetField(name);
  var method=typeof(EDDiscovery.EDDiscoveryForm).GetMethod("DLL"+name,hidden);
  Assert(method!=null,"Host callback method exists: "+name);
  object boxed=new EDDDLLInterfaces.EDDDLLIF.EDDCallBacks();
  field.SetValue(boxed,Delegate.CreateDelegate(field.FieldType,host,method));
  Assert(Object.ReferenceEquals(ResolveHostForTest((EDDDLLInterfaces.EDDDLLIF.EDDCallBacks)boxed),host),"Host found before window shown via "+name);
 }
 Assert(ResolveHostForTest(new EDDDLLInterfaces.EDDDLLIF.EDDCallBacks())==null,"Missing callbacks handled safely");
 }
 static void ShutdownRegression(){
 var path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"RouteTrackerCommander.dll");
 var caller=new EliteDangerousCore.DLL.EDDDLLCaller();
 Assert(caller.Load(path),"Actual host loader loads plugin DLL");
 Assert(caller.UnLoad(),"Actual host unload calls EDDTerminate without binder error");
 Assert(!caller.UnLoad(),"Repeated host unload safe");
 Assert(caller.Load(path),"Plugin reload after unload");
 Assert(!caller.Init("19.1.11",null,AppDomain.CurrentDomain.BaseDirectory,new EDDDLLInterfaces.EDDDLLIF.EDDCallBacks()),"Missing host rejected by initialise");
 Assert(caller.UnLoad(),"Shutdown safe after failed initialise");
 var plugin=new RouteTrackerCommander.RouteTrackerCommanderEDDClass();
 plugin.EDDTerminate();plugin.EDDTerminate();
 Assert(true,"Terminate itself is repeatable");
 }
 static EDDiscovery.EDDiscoveryForm ResolveHostForTest(EDDDLLInterfaces.EDDDLLIF.EDDCallBacks callbacks) {
 return (EDDiscovery.EDDiscoveryForm)typeof(RouteTrackerCommander.RouteTrackerCommanderEDDClass).GetMethod("ResolveHost",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{callbacks}); }
 static void Run(){
 ShutdownRegression(); StartupRegression();
 var plugin=new RouteTrackerCommander.RouteTrackerCommanderEDDClass();
 Assert(plugin.EDDInitialise("19.1.11.0",null,new EDDDLLInterfaces.EDDDLLIF.EDDCallBacks()).Contains("requires"),"Wrong host version rejected clearly");



 var themeType=Assembly.Load("ExtendedControls").GetType("ExtendedControls.Theme");
 themeType.GetProperty("Current").SetValue(null,Activator.CreateInstance(themeType));
 var folder=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-state",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
 EliteDangerousCore.EliteConfigInstance.InstanceOptions=EDDiscovery.EDDOptions.Instance;
 typeof(EDDiscovery.EDDOptions).GetProperty("UserDatabasePath").SetValue(EDDiscovery.EDDOptions.Instance,Path.Combine(folder,"user.sqlite"));
 UserDatabase.Instance.Initialize();
 EDDiscovery.EDDProfiles.Instance.LoadProfiles(null);
 var points=new List<SavedRouteClass.SystemEntry>{new SavedRouteClass.SystemEntry("Sol","",0,0,0),new SavedRouteClass.SystemEntry("Next","",10,0,0),new SavedRouteClass.SystemEntry("End","",20,0,0)};
 Assert(new SavedRouteClass("Alpha",points).Add(),"Create fixture A");
 Assert(new SavedRouteClass("Beta",points).Add(),"Create fixture B");
 using(var panel=new CommanderRouteTracker()) {
 Call(panel,"ApplyCommander",1,true);
 Call(panel,"LoadRoute","Alpha",1);
 panel.PutSetting("routecontrol","autocopy;showfuel;");panel.PutSetting("wordwrap",true);
 panel.PutSetting("font","Arial,12");
 Call(panel,"ApplyCommander",1,true);
 Assert((int)Field(panel,"manualTarget")==1,"Same commander retains manual point");
 ((System.Collections.Generic.SortedList<string,string>)Field(panel,"drawsystemtext")).Add("body","Old commander planet");
 SetField(panel,"drawsystemsignallist","Old signals");SetField(panel,"drawsystemvalue",123L);
 SetField(panel,"scansummarytext","Old summary");SetField(panel,"starclass","Old star");
 SetField(panel,"bodies_found",9);SetField(panel,"all_found",true);
 SetField(panel,"lastsystemonroute","Next");SetField(panel,"instartjump",true);
 Call(panel,"ApplyCommander",2,true);
 Assert(panel.GetSetting("route","")=="","New commander has no route");
 Assert(Field(panel,"currentRoute")==null,"Previous route removed from memory");
 Assert(((System.Collections.Generic.SortedList<string,string>)Field(panel,"drawsystemtext")).Count==0,"Previous body search results cleared");
 Assert((string)Field(panel,"drawsystemsignallist")=="","Previous signals cleared");
 Assert((long)Field(panel,"drawsystemvalue")==0,"Previous scan value cleared");
 Assert((string)Field(panel,"scansummarytext")=="","Previous summary cleared");
 Assert((string)Field(panel,"starclass")=="" && (int)Field(panel,"bodies_found")==0 && !(bool)Field(panel,"all_found"),"Previous title statistics cleared");
 Assert(((System.Windows.Forms.Control)Field(panel,"extPictureBoxRoute")).Visible,"No-route message visible");
 Assert(!((System.Windows.Forms.Control)Field(panel,"extPictureBoxScrollSystemDetails")).Visible,"No-route body details hidden");
 Assert(!((System.Windows.Forms.Control)Field(panel,"extPictureBoxFuel")).Visible,"No-route fuel hidden");
 Assert(((string[])Field(panel,"searchesactivetext")).Length==0,"Tracker does not inherit default planetary searches");
 // A stored legacy search must not reappear while this commander has no route.
 panel.PutSetting("Searches","legacy search");Call(panel,"PopulateCtrlList");Call(panel,"SetVisibility",false);
 Assert(!((System.Windows.Forms.Control)Field(panel,"extPictureBoxScrollSystemDetails")).Visible,"Stored searches cannot replace empty route view");
 Call(panel,"CalculateThenDrawSystemSignals",new EliteDangerousCore.SystemClass(0,0,0,"Sol"));
 Call(panel,"CalculateThenDrawScanSummary",new EliteDangerousCore.SystemClass(0,0,0,"Sol"));
 Assert(((System.Collections.Generic.SortedList<string,string>)Field(panel,"drawsystemtext")).Count==0,"No-route updates do not run scan queries");
 Assert(Field(panel,"lastsystemonroute")==null,"Clipboard cache cleared");
 Assert(!(bool)Field(panel,"instartjump"),"Jump flag reset");
 Assert(!panel.GetSetting("wordwrap",false),"New commander defaults");
 Call(panel,"LoadRoute","Missing route",-1);Call(panel,"DrawRoute",(object)null);Call(panel,"SetVisibility",false);
 Assert(Field(panel,"currentRoute")==null && (int)Field(panel,"manualTarget")==-1,"Deleted route falls back to empty state");
 Assert(((System.Windows.Forms.Control)Field(panel,"extPictureBoxRoute")).Visible,"Deleted route shows no-route message");
 Call(panel,"LoadRoute","Beta",-1);Call(panel,"SetVisibility",false);
 Assert(((System.Windows.Forms.Control)Field(panel,"extPictureBoxScrollSystemDetails")).Visible,"Selected route restores configured detail view");
 panel.PutSetting("routecontrol","showJumps;");
 Call(panel,"ApplyCommander",1,true);
 Assert(((SavedRouteClass)Field(panel,"currentRoute")).Name=="Alpha","Route A reloaded");
 Assert((int)Field(panel,"manualTarget")==1,"Manual point A reloaded");
 Assert((string)Field(panel,"routecontrolsettings")=="autocopy;showfuel;","Copy and display settings A reloaded");
 Assert(panel.GetSetting("wordwrap",false),"Word wrap A reloaded");
 SetField(panel,"cur_sys",new EliteDangerousCore.SystemClass(10,0,0,"Next"));
 Call(panel,"CheckManualTarget");
 Assert(panel.GetSetting("routepos",-1)==2,"Arrival progress saved immediately");
 Call(panel,"ApplyCommander",2,true);
 Assert(((SavedRouteClass)Field(panel,"currentRoute")).Name=="Beta","Route B reloaded");
 Assert((int)Field(panel,"manualTarget")==-1,"Automatic mode B retained");
 Assert((string)Field(panel,"routecontrolsettings")=="showJumps;","Copy disabled for B");
 Call(panel,"ApplyCommander",1,true);
 Assert((int)Field(panel,"manualTarget")==2,"Updated point A retained");
 Call(panel,"LoadRoute","Alpha",500);
 Assert((int)Field(panel,"manualTarget")==-1,"Invalid manual position resets to automatic");
 Call(panel,"LoadRoute","Alpha",1);
 }
 using(var reopened=new CommanderRouteTracker()) {
 Call(reopened,"ApplyCommander",1,true);
 Assert(((SavedRouteClass)Field(reopened,"currentRoute")).Name=="Alpha","Reopened panel restores route");
 Assert((int)Field(reopened,"manualTarget")==1,"Reopened panel restores point");
 Call(reopened,"ApplyCommander",-1,true);
 Assert(Field(reopened,"currentRoute")==null,"Hidden commander separated");
 Call(reopened,"ApplyCommander",1,true);
 Assert(((SavedRouteClass)Field(reopened,"currentRoute")).Name=="Alpha","Real commander restored after hidden");
 }
 using(var fresh=new CommanderRouteTracker()) {
 Call(fresh,"ApplyCommander",3,false);
 Assert(Field(fresh,"ctrlset")==null && Field(fresh,"currentRoute")==null,"Initial commander selection is safe before control settings load");
 }
 using(var surveyor=new CommanderSurveyorPanel()) {
 Call(surveyor,"PopulateCtrlList");SetField(surveyor,"routecontrolsettings","");Call(surveyor,"SetVisibility",false);
 Assert(((string[])Field(surveyor,"searchesactivetext")).Length>0,"Surveyor retains default body searches");
 Assert(((System.Windows.Forms.Control)Field(surveyor,"extPictureBoxScrollSystemDetails")).Visible,"Surveyor without a route still shows body details");
 Assert(!((System.Windows.Forms.Control)Field(surveyor,"extPictureBoxRoute")).Visible,"Surveyor without a route has no route message");
 }
 using(var legacy=new UserControlCommonBase()){legacy.DBBaseName="RouteTracker";Assert(legacy.GetSetting("route","")=="","Original tracker untouched");}
 Console.WriteLine("PASS: "+checks+" assertions; isolated database: "+folder);
 }
}
