using System;
using System.IO;
using Newtonsoft.Json;

namespace CMI
{
    public sealed class RuntimeSettings
    {
        public string ProcessName { get; set; } = "nightreign";
        public string DisplayName { get; set; } = "ELDEN RING NIGHTREIGN";
        public string SoundFolder { get; set; } = "sound";
        public string SoundJson { get; set; } = "sound.json";
        public string EventFlagIdReader { get; set; } = "NativeBridge";
        public MemorySettings Memory { get; set; } = new MemorySettings();

        public static RuntimeSettings Load(string appRootPath)
        {
            string path = ResolveConfigPath(appRootPath);
            RuntimeSettings settings = null;
            if (File.Exists(path))
            {
                settings = JsonConvert.DeserializeObject<RuntimeSettings>(File.ReadAllText(path));
            }

            settings = settings ?? new RuntimeSettings();
            settings.ApplyDefaults();
            return settings;
        }

        private static string ResolveConfigPath(string appRootPath)
        {
            string overridePath = Environment.GetEnvironmentVariable("CMI_CONFIG_FILE");
            if (!string.IsNullOrWhiteSpace(overridePath))
            {
                if (Path.IsPathRooted(overridePath)) return overridePath;
                return Path.Combine(appRootPath, overridePath);
            }

            string genericConfigPath = Path.Combine(appRootPath, "CMI.config.json");
            if (File.Exists(genericConfigPath)) return genericConfigPath;
            return Path.Combine(appRootPath, "CMINightreign.config.json");
        }

        private void ApplyDefaults()
        {
            if (string.IsNullOrWhiteSpace(ProcessName)) ProcessName = "nightreign";
            if (string.IsNullOrWhiteSpace(DisplayName)) DisplayName = "ELDEN RING NIGHTREIGN";
            if (string.IsNullOrWhiteSpace(SoundFolder)) SoundFolder = "sound";
            if (string.IsNullOrWhiteSpace(SoundJson)) SoundJson = "sound.json";
            if (string.IsNullOrWhiteSpace(EventFlagIdReader)) EventFlagIdReader = "NativeBridge";
            Memory = Memory ?? new MemorySettings();
        }

        public bool UsesNativeFlagBridge =>
            string.Equals(EventFlagIdReader, "NativeBridge", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(EventFlagIdReader, "NightreignBridge", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class MemorySettings
    {
        public bool EnableLegacySignatureScanning { get; set; } = false;
        public string EventFlagManQuery { get; set; }
        public string GameDataManQuery { get; set; }
        public string WorldChrManQuery { get; set; }
        public string Notes { get; set; } =
            "EventFlagId reads can use the ME3 native flag bridge. Legacy pointer entries remain available for compatibility.";
    }
}
