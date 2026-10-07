using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
namespace RouteTrackerCommander
{
    internal sealed class ReleaseUpdate
    {
        public Version Version { get; set; }
        public string ReleaseUrl { get; set; }
        public string AssetUrl { get; set; }
        public bool Preview { get; set; }
    }
    internal static class ReleaseChecker
    {
        internal const string Repository = "https://github.com/SirAdams/RouteTrackerCommander";
        private static readonly object gate = new object();
        private static Task<string> cached;
        private static DateTime lastAttempt;
        internal static Task<string> GetReleases(bool force)
        {
            lock (gate)
            {
                if (cached != null && !cached.IsCompleted) return cached;
                if (!force && cached != null && DateTime.UtcNow - lastAttempt < TimeSpan.FromHours(cached.Status == TaskStatus.RanToCompletion ? 12 : 1)) return cached;
                lastAttempt = DateTime.UtcNow;
                return cached = Fetch();
            }
        }
        private static async Task<string> Fetch()
        {
            using (var client = new HttpClient(new HttpClientHandler { SslProtocols = System.Security.Authentication.SslProtocols.Tls12 }))
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.MaxResponseContentBufferSize = 1024 * 1024;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RouteTrackerCommander/1.1.2");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
                return await client.GetStringAsync("https://api.github.com/repos/SirAdams/RouteTrackerCommander/releases?per_page=30").ConfigureAwait(false);
            }
        }
        internal static ReleaseUpdate SelectRelease(string json, Version installed, string hostSuffix)
        {
            var releases = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 }.DeserializeObject(json) as object[];
            if (releases == null) throw new FormatException("Invalid release response");
            ReleaseUpdate best = null;
            foreach (var item in releases)
            {
                var release = item as Dictionary<string, object>;
                if (release == null || Flag(release, "draft")) continue;
                string tag = Text(release, "tag_name");
                if (!System.Text.RegularExpressions.Regex.IsMatch(tag, @"^v?\d+\.\d+\.\d+(?:\.\d+)?$")) continue;
                Version parsed;
                if (!Version.TryParse(tag.TrimStart('v'), out parsed)) continue;
                var version = new Version(parsed.Major, parsed.Minor, parsed.Build, Math.Max(0, parsed.Revision));
                if (version <= installed || (best != null && version <= best.Version)) continue;
                var assets = release.ContainsKey("assets") ? release["assets"] as object[] : null;
                if (assets == null) continue;
                var assetName = "RouteTrackerCommander-" + tag.TrimStart('v') + "-" + hostSuffix + ".zip";
                var assetUrl = Repository + "/releases/download/" + tag + "/" + assetName;
                if (!assets.OfType<Dictionary<string, object>>().Any(a => Text(a, "name") == assetName && Text(a, "browser_download_url") == assetUrl)) continue;
                best = new ReleaseUpdate { Version = version, Preview = Flag(release, "prerelease"), ReleaseUrl = Repository + "/releases/tag/" + tag, AssetUrl = assetUrl };
            }
            return best;
        }
        private static string Text(Dictionary<string, object> item, string key) { object value; return item.TryGetValue(key, out value) ? value as string ?? "" : ""; }
        private static bool Flag(Dictionary<string, object> item, string key) { object value; return item.TryGetValue(key, out value) && value is bool && (bool)value; }
    }
}
