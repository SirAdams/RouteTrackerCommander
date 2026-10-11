using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EDDiscovery.UserControls;
using EliteDangerousCore;
using EliteDangerousCore.DB;

// Compile with edd19/src/*.cs and shared/*.cs, the real EDD19 references,
// /target:exe /main:OptimizationTests. Pass the EDD19 installation directory
// as argument 1. This suite neither starts a host nor initializes its database.
internal static class OptimizationTests
{
    private static int checks;
    private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

    [STAThread]
    private static void Main(string[] args)
    {
        string dependencies = args.Length > 0 ? Path.GetFullPath(args[0]) : AppDomain.CurrentDomain.BaseDirectory;
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            string name = new AssemblyName(e.Name).Name;
            string path = Path.Combine(dependencies, name + (name == "EDDiscovery" ? ".exe" : ".dll"));
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try
        {
            Run();
            Console.WriteLine("PASS: " + checks + " route optimization assertions; no database or network used");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Environment.ExitCode = 1;
        }
    }

    private static void Assert(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }

    private static void Near(double actual, double expected, string message)
    {
        Assert(Math.Abs(actual - expected) <= 1e-9 * Math.Max(1, Math.Abs(expected)),
            message + ": expected " + expected + ", actual " + actual);
    }

    private static void Set(CommanderSurveyorPanel panel, string field, object value)
    {
        typeof(CommanderSurveyorPanel).GetField(field, Hidden).SetValue(panel, value);
    }

    private static int Count(CommanderSurveyorPanel panel, string field)
    {
        return Convert.ToInt32(typeof(CommanderSurveyorPanel).GetField(field, Hidden).GetValue(panel));
    }

    private static SavedRouteClass Route(string name, params SavedRouteClass.SystemEntry[] points)
    {
        return new SavedRouteClass(name, new List<SavedRouteClass.SystemEntry>(points));
    }

    private static SavedRouteClass.SystemEntry Point(string name, double x, string note = "")
    {
        return new SavedRouteClass.SystemEntry(name, note, x, 0, 0);
    }

    private static ISystem Here(string name, double x, double y = 0)
    {
        return new SystemClass(name, null, x, y, 0);
    }

    private static string Name(ISystem system)
    {
        return system == null ? null : system.Name;
    }

    private static void SameSystem(ISystem actual, ISystem expected, string label)
    {
        Assert(Name(actual) == Name(expected), label + " system name");
        Assert((actual == null) == (expected == null), label + " system presence");
        if (actual != null)
        {
            Near(actual.X, expected.X, label + " X");
            Near(actual.Y, expected.Y, label + " Y");
            Near(actual.Z, expected.Z, label + " Z");
        }
    }

    private static SavedRouteClass.ClosestInfo MatchesHost(CommanderSurveyorPanel panel,
        SavedRouteClass route, ISystem position, int manual, string label)
    {
        Set(panel, "manualTarget", manual);
        SavedRouteClass.ClosestInfo actual = panel.GetRouteClosest(position);
        SavedRouteClass.ClosestInfo expected = route.ClosestTo(position, manual);
        Assert((actual == null) == (expected == null), label + " closest presence");
        Near(panel.GetRouteDistance(), route.CumulativeDistance(), label + " total distance");
        if (expected == null) return actual;
        SameSystem(actual.firstsystem, expected.firstsystem, label + " first");
        SameSystem(actual.finalsystem, expected.finalsystem, label + " final");
        SameSystem(actual.lastsystem, expected.lastsystem, label + " last");
        SameSystem(actual.nextsystem, expected.nextsystem, label + " next");
        Assert(actual.waypoint == expected.waypoint, label + " waypoint index");
        Assert(actual.lastsystemwaypointnote == expected.lastsystemwaypointnote, label + " previous note");
        Assert(actual.nextsystemwaypointnote == expected.nextsystemwaypointnote, label + " next note");
        Near(actual.deviation, expected.deviation, label + " deviation");
        Near(actual.cumulativewpdist, expected.cumulativewpdist, label + " remaining distance");
        Near(actual.disttowaypoint, expected.disttowaypoint, label + " waypoint distance");
        return actual;
    }

    private static void Run()
    {
        Type theme = Assembly.Load("ExtendedControls").GetType("ExtendedControls.Theme");
        theme.GetProperty("Current").SetValue(null, Activator.CreateInstance(theme));
        using (var panel = new CommanderRouteTracker())
        {
            SavedRouteClass route = Route("Straight", Point("Start", 0), Point("Middle", 10, "middle note"), Point("End", 20));
            Set(panel, "currentRoute", route);
            ISystem start = Here("Start", 0);
            SavedRouteClass.ClosestInfo closest = MatchesHost(panel, route, start, 1, "manual middle");
            Assert(closest.nextsystem.Name == "Middle", "Manual index selects Middle");
            Near(closest.disttowaypoint, 10, "Middle is 10 ly away");
            Near(closest.cumulativewpdist, 10, "Middle has 10 ly remaining");
            Near(panel.GetRouteDistance(), 20, "Straight route is 20 ly");

            int builds = Count(panel, "routeGeometryBuilds");
            int calculations = Count(panel, "routeClosestCalculations");
            MatchesHost(panel, route, start, 1, "same position repeated");
            MatchesHost(panel, route, Here("Start", 0), 1, "distinct equal position object");
            Assert(Count(panel, "routeGeometryBuilds") == builds, "Repeated redraw reuses route geometry");
            Assert(Count(panel, "routeClosestCalculations") == calculations, "Equal positions reuse closest calculation across object instances");

            closest = MatchesHost(panel, route, start, 2, "manual end");
            Assert(closest.nextsystem.Name == "End", "Manual index change selects End");
            Assert(Count(panel, "routeGeometryBuilds") == builds, "Manual progress reuses fixed route geometry");
            Assert(Count(panel, "routeClosestCalculations") == calculations + 1, "Manual progress recalculates closest result");

            calculations = Count(panel, "routeClosestCalculations");
            closest = MatchesHost(panel, route, Here("Start", 5), 2, "same name changed coordinates");
            Near(closest.disttowaypoint, 15, "Coordinate change updates target distance");
            Assert(Count(panel, "routeClosestCalculations") == calculations + 1, "Current position coordinate change recalculates");
            Assert(Count(panel, "routeGeometryBuilds") == builds, "Current position change does not rebuild route geometry");

            MatchesHost(panel, route, start, 1, "named previous waypoint");
            calculations = Count(panel, "routeClosestCalculations");
            closest = MatchesHost(panel, route, Here("Traveler", 0), 1, "same coordinates different name");
            Assert(closest.lastsystem == null, "Different current name does not report arrival at previous waypoint");
            Assert(Count(panel, "routeClosestCalculations") == calculations + 1, "Name changes invalidate arrival-dependent closest result");

            closest = MatchesHost(panel, route, Here("Far away", 100, 100), -1, "off route");
            Assert(closest.nextsystem.Name == "End" && closest.deviation == -1, "Off-route location selects nearest waypoint");

            SavedRouteClass replacement = Route("Replacement", Point("New start", 0), Point("New middle", 30), Point("New end", 60));
            Set(panel, "currentRoute", replacement);
            builds = Count(panel, "routeGeometryBuilds");
            closest = MatchesHost(panel, replacement, start, 1, "route replaced same count");
            Assert(closest.nextsystem.Name == "New middle", "Replacement target cannot come from previous route");
            Near(panel.GetRouteDistance(), 60, "Replacement length cannot come from previous route");
            Assert(Count(panel, "routeGeometryBuilds") == builds + 1, "Replacement route rebuilds geometry even with identical waypoint count");

            builds = Count(panel, "routeGeometryBuilds");
            replacement.Systems.Add(Point("Added end", 80));
            closest = MatchesHost(panel, replacement, start, 3, "waypoint appended");
            Assert(closest.nextsystem.Name == "Added end", "Changed route count makes appended waypoint available");
            Near(panel.GetRouteDistance(), 80, "Appended route length updates");
            Assert(Count(panel, "routeGeometryBuilds") == builds + 1, "Changed route count rebuilds geometry");

            builds = Count(panel, "routeGeometryBuilds");
            calculations = Count(panel, "routeClosestCalculations");
            replacement.Systems[3].X = 100;
            panel.InvalidateRouteGeometry();
            closest = MatchesHost(panel, replacement, start, 3, "explicit geometry invalidation");
            Near(closest.disttowaypoint, 100, "Explicit invalidation updates edited waypoint coordinate");
            Near(panel.GetRouteDistance(), 100, "Explicit invalidation updates route length");
            Assert(Count(panel, "routeGeometryBuilds") == builds + 1, "Explicit invalidation rebuilds geometry");
            Assert(Count(panel, "routeClosestCalculations") == calculations + 1, "Explicit invalidation clears equal-position closest cache");

            SavedRouteClass partial = Route("Unknown coordinates", Point("Start", 0),
                new SavedRouteClass.SystemEntry("Unknown", "unknown note"), Point("Middle", 10, "known note"), Point("End", 20));
            Set(panel, "currentRoute", partial);
            closest = MatchesHost(panel, partial, Here("Along", 5), -1, "missing coordinates automatic");
            Assert(closest.nextsystem.Name == "Middle" && closest.waypoint == 2, "Automatic waypoint retains original route index past unknown coordinates");
            closest = MatchesHost(panel, partial, start, 2, "missing coordinates manual");
            Assert(closest.nextsystem.Name == "End", "Manual index retains host's known-coordinate indexing");
            Near(panel.GetRouteDistance(), 20, "Unknown-coordinate entries are excluded from geometry");

            SavedRouteClass duplicate = Route("Repeated names", Point("Start", 0),
                Point("Repeat", 10, "first repeat"), Point("Repeat", 30, "second repeat"), Point("End", 40));
            Set(panel, "currentRoute", duplicate);
            closest = MatchesHost(panel, duplicate, Here("Repeat", 30), 2, "duplicate names manual");
            Assert(closest.nextsystem.X == 30 && closest.nextsystemwaypointnote == "second repeat", "Duplicate target keeps the selected occurrence and note");
            Near(closest.cumulativewpdist, 30, "Duplicate names preserve host's first-name-match remaining distance");
            MatchesHost(panel, duplicate, Here("Along", 25), -1, "duplicate names automatic");
            MatchesHost(panel, duplicate, Here("Beyond", 80, 30), -1, "duplicate names off route");

            SavedRouteClass unknown = Route("No coordinates", new SavedRouteClass.SystemEntry("Unknown"));
            Set(panel, "currentRoute", unknown);
            Assert(MatchesHost(panel, unknown, start, -1, "no known coordinates") == null, "Route without known coordinates has no target");
            SavedRouteClass single = Route("One point", Point("Only", 7, "single note"));
            Set(panel, "currentRoute", single);
            closest = MatchesHost(panel, single, start, -1, "one known coordinate");
            Assert(closest.nextsystem.Name == "Only" && closest.deviation == -1, "One-point automatic route selects its only waypoint");
            MatchesHost(panel, single, start, 500, "manual index clamped by host");
        }
    }
}
