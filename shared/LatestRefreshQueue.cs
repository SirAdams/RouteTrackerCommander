using System;
using System.Threading.Tasks;
namespace RouteTrackerCommander {
 // UI-thread queue: keep only the newest pending request while a calculation awaits.
 internal sealed class LatestRefreshQueue<T> {
  private bool running, pending, stopped;
  private int revision;
  private T value;
  private Func<T,int,Task> work;
  internal int Completed { get; private set; }
  internal bool IsCurrent(int request) => !stopped && request == revision;
  internal void CancelPending() { revision++; pending=false; }
  internal void Stop() { stopped=true; CancelPending(); }
  internal void Request(T next, Func<T,int,Task> calculate) {
   if(stopped)return;
   value=next;work=calculate;revision++;pending=true;
   if(!running)Run();
  }
  private async void Run() {
   running=true;
   try {
    while(pending && !stopped) {
     var next=value;var calculate=work;int request=revision;pending=false;
     try { await calculate(next,request); Completed++; }
     catch(Exception ex) { System.Diagnostics.Trace.WriteLine("Route Tracker Commander refresh failed: "+ex); }
    }
   } finally {running=false;}
  }
 }
}