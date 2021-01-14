namespace Engine.DatabaseInteraction
{
    internal class DatabaseVersion
    {
        public string VersionText { get; }

        public bool Supported { get; }

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public DatabaseVersion(string version, int major, int minor, int patch, bool supported)
        {
            VersionText = version;
            Supported = supported;

            Major = major;
            Minor = minor;
            Patch = patch;
        }
    }
}
