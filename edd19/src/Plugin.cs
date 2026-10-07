using System;using System.Linq;using System.Windows.Forms;using EDDiscovery;using EDDiscovery.UserControls;
using IF=EDDDLLInterfaces.EDDDLLIF;
[assembly:System.Reflection.AssemblyVersion("1.1.2.0")]
[assembly:System.Reflection.AssemblyTitle("Route Tracker Commander")]
[assembly:System.Reflection.AssemblyDescription("Standalone commander-specific Route Tracker for EDDiscovery")]
namespace RouteTrackerCommander {
 public sealed class RouteTrackerCommanderEDDClass {
  internal static EDDiscoveryForm ResolveHost(IF.EDDCallBacks callbacks) {
   return callbacks.RequestHistory?.Target as EDDiscoveryForm ?? callbacks.GetShipLoadout?.Target as EDDiscoveryForm ?? Application.OpenForms.OfType<EDDiscoveryForm>().FirstOrDefault();
  }
  public void EDDTerminate() { }
  public string EDDInitialise(string flags,string folder,IF.EDDCallBacks callbacks) {
   Version hostVersion;var match=System.Text.RegularExpressions.Regex.Match(flags ?? "",@"^\d+\.\d+\.\d+(?:\.\d+)?");
   if(!Version.TryParse(match.Value,out hostVersion)|| !(hostVersion.Major==19 && hostVersion.Minor==1 && hostVersion.Build==11)) return "!This Route Tracker Commander DLL requires EDDiscovery 19.1.11. Install the matching package.";
   var form=ResolveHost(callbacks);if(form==null)return "!EDDiscovery main window unavailable";
   RegisterPanel(form);return "1.1.2.0";
  }
  private static void RegisterPanel(EDDiscoveryForm form) {
   int id=EDDConfig.Instance.FindCreatePanelID("RouteTrackerCommander.Native");
   form.AddPanel(id,typeof(CommanderRouteTracker),null,"Route Tracker — Commander","RouteTrackerCommander",
    "Route Tracker with separate routes and settings for each commander",System.Drawing.SystemIcons.Application.ToBitmap(),false);
  }
 }
}
