namespace Pulsar.Server.Utilities
{
    public static class ServerVersion
    {
        public const string AppName = "Pulsar STNull";

        public const string Current = "2.4.5";

        public static string Display => $"v{Current}";

        public static string WindowTitle => $"{AppName} - {Display}";
    }
}
