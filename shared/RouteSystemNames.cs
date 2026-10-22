using System;
namespace RouteTrackerCommander {
 // Keep these operations independent of BaseUtils extension-method ABI changes.
 internal static class RouteSystemNames {
  internal static bool Equal(string left,string right) => left != null && String.Equals(left,right,StringComparison.InvariantCultureIgnoreCase);
  internal static string WithoutSystemPrefix(string body,string system) {
   if(body != null && system != null && body.Length > system.Length && body.StartsWith(system,StringComparison.InvariantCultureIgnoreCase))
    return body.Substring(system.Length).TrimStart();
   return body;
  }
 }
}