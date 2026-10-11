using System;
using System.Collections.Generic;
using System.Drawing;
using EliteDangerousCore;
using EliteDangerousCore.DB;
using ExtendedControls;
namespace EDDiscovery.UserControls {
 public partial class CommanderSurveyorPanel {
  private readonly RouteTrackerCommander.LatestRefreshQueue<ISystem> summaryRefresh=new RouteTrackerCommander.LatestRefreshQueue<ISystem>();
  private readonly RouteTrackerCommander.LatestRefreshQueue<ISystem> signalsRefresh=new RouteTrackerCommander.LatestRefreshQueue<ISystem>();
  private SavedRouteClass cachedGeometryRoute;
  private int cachedGeometryCount;
  private List<Tuple<ISystem,int,SavedRouteClass.SystemEntry>> cachedRouteSystems;
  private double cachedRouteDistance;
  private bool closestValid;
  private string cachedPositionName;
  private double cachedX,cachedY,cachedZ;
  private int cachedManualTarget;
  private SavedRouteClass.ClosestInfo cachedClosest;
  internal int routeGeometryBuilds,routeClosestCalculations;
  private readonly Dictionary<ExtPictureBox,TextRenderState> textRenders=new Dictionary<ExtPictureBox,TextRenderState>();
  private TextRenderState routeRender;
  private bool routeRenderValid;
  private int routeRenderManual,routeRenderCount;
  private bool routeRenderButtons;
  internal void InvalidateRouteGeometry() {
   cachedGeometryRoute=null;cachedRouteSystems=null;closestValid=false;
   textRenders.Clear();routeRenderValid=false;
  }
  private void EnsureRouteGeometry() {
   if(currentRoute==null)return;
   if(!Object.ReferenceEquals(currentRoute,cachedGeometryRoute)||cachedGeometryCount!=currentRoute.Systems.Count) {
    cachedGeometryRoute=currentRoute;cachedGeometryCount=currentRoute.Systems.Count;
    cachedRouteSystems=currentRoute.SystemsWithCoordinates();
    cachedRouteDistance=currentRoute.CumulativeDistance(null,cachedRouteSystems);
    closestValid=false;routeGeometryBuilds++;
   }
  }
  internal double GetRouteDistance() {EnsureRouteGeometry();return currentRoute==null?0:cachedRouteDistance;}
  internal SavedRouteClass.ClosestInfo GetRouteClosest(ISystem sys) {
   if(currentRoute==null || sys==null)return null;
   EnsureRouteGeometry();
   if(!closestValid || cachedPositionName!=sys.Name || cachedX!=sys.X || cachedY!=sys.Y || cachedZ!=sys.Z || cachedManualTarget!=currentRouteManualTarget) {
    cachedClosest=currentRoute.ClosestTo(sys,currentRouteManualTarget);
    cachedPositionName=sys.Name;cachedX=sys.X;cachedY=sys.Y;cachedZ=sys.Z;cachedManualTarget=currentRouteManualTarget;
    closestValid=true;routeClosestCalculations++;
   }
   return cachedClosest;
  }
  private List<Tuple<ISystem,int,SavedRouteClass.SystemEntry>> GetRouteSystems() {EnsureRouteGeometry();return cachedRouteSystems;}
  private struct TextRenderState {
   internal string Text; internal int Width,BoxWidth; internal Font Font,ControlFont; internal Color Fore,Back,Buttons; internal bool Wrap,Dividers; internal StringAlignment Alignment;
   internal bool Same(TextRenderState other) => Text==other.Text && Width==other.Width && BoxWidth==other.BoxWidth && Object.Equals(Font,other.Font) && Object.Equals(ControlFont,other.ControlFont) && Buttons==other.Buttons && Fore==other.Fore && Back==other.Back && Wrap==other.Wrap && Dividers==other.Dividers && Alignment==other.Alignment;
  }
  private TextRenderState RenderState(string text) => new TextRenderState {Text=text,Width=extPictureBoxSystemDetails.Width,Font=displayfont??Font,ControlFont=Font,Buttons=Theme.Current.ButtonBackColor,Fore=IsTransparentModeOn?Theme.Current.SPanelColor:Theme.Current.LabelColor,Back=IsTransparentModeOn?Color.Transparent:BackColor,Wrap=extCheckBoxWordWrap.Checked,Dividers=IsSet(CtrlList.showdividers),Alignment=alignment};
  private bool ShouldRenderText(ExtPictureBox pb,string text) {
   if(!pb.Visible){textRenders.Remove(pb);return false;}
   var next=RenderState(text);next.BoxWidth=pb.Width;
   if(textRenders.TryGetValue(pb,out var previous)&&next.Same(previous))return false;
   textRenders[pb]=next;return true;
  }
  private bool ShouldRenderRoute(string text,bool hasPosition) {
   if(!extPictureBoxRoute.Visible){routeRenderValid=false;return false;}
   var next=RenderState(text);next.BoxWidth=extPictureBoxRoute.Width;bool buttons=hasPosition&&!IsCurrentlyTransparent;int count=currentRoute?.Systems.Count??0;
   if(routeRenderValid && next.Same(routeRender) && routeRenderManual==currentRouteManualTarget && routeRenderCount==count && routeRenderButtons==buttons)return false;
   routeRender=next;routeRenderManual=currentRouteManualTarget;routeRenderCount=count;routeRenderButtons=buttons;routeRenderValid=true;return true;
  }
  private void CalculateThenDrawScanSummary(ISystem sys) {
   if(IsClosed || IsDisposed || RouteTrackerWithoutRoute || ctrlset==null || !IsSet(CtrlList.showscansum)){summaryRefresh.CancelPending();return;}
   summaryRefresh.Request(sys,CalculateThenDrawScanSummaryCore);
  }
  private void CalculateThenDrawSystemSignals(ISystem sys) {
   if(IsClosed || IsDisposed || RouteTrackerWithoutRoute){signalsRefresh.CancelPending();return;}
   signalsRefresh.Request(sys,CalculateThenDrawSystemSignalsCore);
  }
 }
}