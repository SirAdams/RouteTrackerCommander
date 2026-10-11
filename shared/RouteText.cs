namespace RouteTrackerCommander
{
    internal static class RouteText
    {
        internal static string Append(string text, string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment)) return text;
            fragment = fragment.Trim();
            if (string.IsNullOrEmpty(text)) return fragment;
            bool punctuation = fragment[0] == ',' || fragment[0] == ';' || fragment[0] == '，' || fragment[0] == '；';
            return text.TrimEnd() + (punctuation ? "" : " ") + fragment;
        }
    }
}
