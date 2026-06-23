using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using ECN.MediaPlayer;
using memory;
using NAudio.CoreAudioApi;
// using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162

namespace CMI
{
    public partial class CMI : Form
    {
        private const string fallbackEventFlagManQuery = "48 8B 3D ?? ?? ?? ?? 48 85 FF ?? ?? 32 C0 E9";
        private const string fallbackGameDataManQuery = "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 05 48 8B 40 58 C3 C3";
        private const string fallbackWorldChrManQuery = "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 0F 48 39 88";
        public static string appRootPath;
        public static string modSoundFolderPath;
        public static string soundJsonFilePath;
        public static JObject soundJson;
        private static RuntimeSettings runtimeSettings;
        private static Process mainGameProcess;
        private static IntPtr gameProcessHandle;
        private static IntPtr eventFlagMan;
        private static IntPtr worldChrMan;
        private static long gameDataMan;
        private static Scanner eventFlagManScanner;
        private static Scanner gameDataManScanner;
        private static Scanner worldChrManScanner;
        private static int masterVolume;
        private static bool shouldResetUIPlayerPosition;
        private static string lastFlagBridgeStatus;
        private static bool legacySignatureScanningEnabled;
        private static bool gameDataManLookupAvailable;
        private static bool nativeStarted;
        private static readonly object nativeStartLock = new object();
        public static readonly List<SoundEvent> soundEvents = new List<SoundEvent>();
        private static readonly MediaPlayer musicMediaPlayer = new MediaPlayer(0, true, 0);
        private static readonly MediaPlayer soundEffectsMediaPlayer = new MediaPlayer(0, true, 0);
        private static readonly MediaPlayer voiceMediaPlayer = new MediaPlayer(0, true, 0);
        private static readonly Timer cooldownTimer = new Timer();
        public static CMI cmi;
        private AudioSessionControl gameAudioSession;
        private bool audioSessionInitialized;

        public CMI()
        {
            InitializeComponent();
            CenterToScreen();
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        private static byte[] ReadValue(IntPtr address, int length)
        {
            byte[] result = new byte[length];
            ReadProcessMemory(gameProcessHandle, address, result, length, out IntPtr _);
            return result;
        }

        private static int ReadInt(IntPtr address)
        {
            return BitConverter.ToInt32(ReadValue(address, 4), 0);
        }

        private static long ReadLong(IntPtr address)
        {
            return BitConverter.ToInt64(ReadValue(address, 8), 0);
        }

        private static byte ReadByte(IntPtr address)
        {
            return ReadValue(address, 1)[0];
        }

        private static int ReadVolume(long baseAddress, int offset)
        {
            return ReadByte((IntPtr)ReadLong((IntPtr)baseAddress + 0x58) + offset) * 10;
        }

        private static bool IsHPZero()
        {
            if (worldChrMan == IntPtr.Zero) return false;
            try
            {
                IntPtr addr = (IntPtr)(ReadLong(worldChrMan) + 0x10EF8);
                addr = (IntPtr)ReadLong(addr);
                addr = (IntPtr)(ReadLong(addr) + 0x190);
                addr = (IntPtr)ReadLong(addr);
                addr = (IntPtr)(ReadLong(addr) + 0x138);
                return ReadInt(addr) == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsHPInvalid()
        {
            if (worldChrMan == IntPtr.Zero) return false;
            try
            {
                IntPtr addr = (IntPtr)(ReadLong(worldChrMan) + 0x10EF8);
                addr = (IntPtr)ReadLong(addr);
                addr = (IntPtr)(ReadLong(addr) + 0x190);
                addr = (IntPtr)ReadLong(addr);
                addr = (IntPtr)(ReadLong(addr) + 0x138);
                return addr.ToInt64() == 312;
            }
            catch
            {
                return false;
            }
        }

        /*
        private static bool ReadIsLoadingScreenActive(long baseAddress, int offset)
        {
            return ReadByte((IntPtr)ReadLong((IntPtr)baseAddress + 0x3D6B7D0) + offset) != 0;
        }
        */

        /*
        private static bool ReadIsLoadingScreenActive(IntPtr baseAddress, int offset)
        {
            return ReadByte((IntPtr)ReadLong(baseAddress + 0x3D6B7D0) + offset) != 0;
        }
        */

        private void SendStatusLogMessage(string message)
        {
            int messageIndex = statusLogTextBox.Text.LastIndexOf(message, StringComparison.Ordinal);
            if (messageIndex == statusLogTextBox.TextLength - message.Length - 2) return;
            statusLogTextBox.AppendText($"{message}\r\n");
            statusLogTextBox.SelectionStart = statusLogTextBox.TextLength;
            statusLogTextBox.ScrollToCaret();
        }

        private static Process[] GetGameProcesses()
        {
            return Process.GetProcessesByName(runtimeSettings.ProcessName);
        }

        private static Scanner ConfigureMemoryScanner(string searchQuery)
        {
            Scanner memoryScanner = new Scanner(mainGameProcess, gameProcessHandle, searchQuery);
            memoryScanner.setModule(mainGameProcess.MainModule);
            return memoryScanner;
        }

        private static void ConfigureMemoryScanners()
        {
            if (!legacySignatureScanningEnabled) return;
            eventFlagManScanner = ConfigureMemoryScanner(
                runtimeSettings.Memory.EventFlagManQuery ?? fallbackEventFlagManQuery);
            gameDataManScanner = ConfigureMemoryScanner(
                runtimeSettings.Memory.GameDataManQuery ?? fallbackGameDataManQuery);
            worldChrManScanner = ConfigureMemoryScanner(
                runtimeSettings.Memory.WorldChrManQuery ?? fallbackWorldChrManQuery);
        }

        private static IntPtr TryGetQueryResultAsPointer(Memory scanner)
        {
            try
            {
                IntPtr pointer = (IntPtr)scanner.FindPattern();
                if (pointer == IntPtr.Zero) return IntPtr.Zero;
                pointer += ReadInt(pointer + 3) + 7;
                return pointer;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private static bool SetEventFlagMan()
        {
            if (!legacySignatureScanningEnabled || eventFlagManScanner == null) return false;
            eventFlagMan = TryGetQueryResultAsPointer(eventFlagManScanner);
            return eventFlagMan != IntPtr.Zero;
        }

        private static bool SetGameDataMan()
        {
            if (!gameDataManLookupAvailable || gameDataManScanner == null) return false;
            IntPtr gameDataManPointer = TryGetQueryResultAsPointer(gameDataManScanner);
            if (gameDataManPointer == IntPtr.Zero)
            {
                gameDataMan = 0;
                gameDataManLookupAvailable = false;
                return false;
            }
            try
            {
                gameDataMan = ReadLong(gameDataManPointer);
                return gameDataMan != 0;
            }
            catch
            {
                gameDataMan = 0;
                gameDataManLookupAvailable = false;
                return false;
            }
        }

        private static bool SetWorldChrMan()
        {
            if (!legacySignatureScanningEnabled || worldChrManScanner == null) return false;
            worldChrMan = TryGetQueryResultAsPointer(worldChrManScanner);
            return worldChrMan != IntPtr.Zero;
        }

        private bool PostAttachToGameSetup()
        {
            legacySignatureScanningEnabled = runtimeSettings.Memory.EnableLegacySignatureScanning;
            gameDataManLookupAvailable = legacySignatureScanningEnabled;
            if (!legacySignatureScanningEnabled)
            {
                SendStatusLogMessage("Legacy memory signature scanning is disabled. This avoids Nightreign stutter from repeated failed scans.");
                SendStatusLogMessage("AlwaysActive events, EventFlagId bridge, and Windows/session volume fallback remain available.");
                return false;
            }

            ConfigureMemoryScanners();
            bool eventFlagReady = SetEventFlagMan();
            bool worldChrReady = SetWorldChrMan();
            bool gameDataReady = SetGameDataMan();
            if (!eventFlagReady)
                SendStatusLogMessage("EventFlagMan signature not found. EventFlagId and legacy pointer events will stay inactive.");
            if (!worldChrReady)
                SendStatusLogMessage("WorldChrMan signature not found. HP/death guards are disabled.");
            if (!gameDataReady)
                SendStatusLogMessage("GameDataMan signature not found. Falling back to Windows/session volume only.");
            return eventFlagReady;
        }

        private async Task AttachToGame()
        {
            Process[] gameProcesses;
            while (true)
            {
                gameProcesses = GetGameProcesses();
                if (gameProcesses.Length == 0)
                {
                    SendStatusLogMessage($"{runtimeSettings.DisplayName} is not currently running, waiting...");
                    await Task.Delay(2000);
                }
                else break;
            }
            SendStatusLogMessage($"Attaching to {runtimeSettings.DisplayName}...");
            mainGameProcess = gameProcesses[0];
            gameProcessHandle = OpenProcess(0x00000010, false, mainGameProcess.Id);
            SendStatusLogMessage($"{runtimeSettings.DisplayName} process handle: {gameProcessHandle}");
            PostAttachToGameSetup();
            BeginGameStateTimer();
        }

        private bool ReadSoundJSON()
        {
            try
            {
                SendStatusLogMessage($"Reading sound configuration file: \"{soundJsonFilePath}\"");
                soundJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(soundJsonFilePath));
                return true;
            }
            catch
            {
                SendStatusLogMessage("Failed to read sound configuration, cannot attach to game");
                return false;
            }
        }

        public void LoadSoundEvents()
        {
            soundEvents.Clear();
            soundEventsListBox.Nodes.Clear();
            foreach (JProperty soundEvent in soundJson.Properties())
                soundEvents.Add(SoundEvent.Deserialize(soundEvent.Name, (JObject)soundEvent.Value));
            foreach (SoundEvent soundEvent in soundEvents)
            {
                foreach (PropertyInfo prop in typeof(SoundEvent).GetProperties().Skip(6))
                {
                    TreeNode propertyNode = new TreeNode { Text = prop.Name };
                    propertyNode.Nodes.Add(Convert.ToString(prop.GetValue(soundEvent)) ?? "");
                    soundEvent.EventNode.Nodes.Add(propertyNode);
                }
                soundEventsListBox.Nodes.Add(soundEvent.EventNode);
            }
        }

        private int GetCurrentVolume(int valueOffset, SoundEvent soundEvent = null, int savedVolume = -1)
        {
            int currentVolume = savedVolume > 0 ? savedVolume : 100;
            if (gameDataMan != 0)
            {
                try
                {
                    currentVolume = ReadVolume(gameDataMan, valueOffset);
                }
                catch
                {
                    currentVolume = savedVolume == -1 ? 100 : savedVolume;
                }
            }
            int gameWinVolume = GetGameWinVolume();
            if (soundEvent != null)
            {
                currentVolume = (int)(currentVolume * (gameWinVolume / 100.0));
                currentVolume = (int)((double)currentVolume / 100 * masterVolume / 100 * 100);
                soundEvent.MediaPlayer.Volume = currentVolume;
            }
            if (savedVolume == -1 && soundEvent != null) savedVolume = soundEvent.Volume;
            if (currentVolume == savedVolume) return currentVolume;
            string volumeHost = soundEvent == null ? "Master" : soundEvent.Name;
            SendStatusLogMessage($"{volumeHost} volume changed to {currentVolume}%");
            return currentVolume;
        }

        private int GetGameWinVolume()
        {
            if (!audioSessionInitialized) InitializeAudioSession();
            if (gameAudioSession == null) return 100;
            return (int)(gameAudioSession.SimpleAudioVolume.Volume * 100);
        }

        private void InitializeAudioSession()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            AudioSessionManager sessionManager = defaultDevice.AudioSessionManager;
            for (int i = 0; i < sessionManager.Sessions.Count; i++)
            {
                AudioSessionControl session = sessionManager.Sessions[i];
                uint process = session.GetProcessID;
                if (process != mainGameProcess.Id) continue;
                audioSessionInitialized = true;
                gameAudioSession = session;
                break;
            }
        }

        private void UpdateUISoundPlayerState(SoundEvent soundEvent)
        {
            string originalUiSoundPlayerURL = uiSoundPlayer.URL;
            if (!soundEvent.Activated || soundEvent.MediaPlayer.CurrentSong != soundEvent.SoundPath)
            {
                uiSoundPlayer.Ctlcontrols.stop();
                uiSoundPlayer.URL = originalUiSoundPlayerURL;
                return;
            }
            uiSoundPlayer.settings.autoStart = false;
            uiSoundPlayer.URL = soundEvent.MediaPlayer.CurrentSong;
            uiSoundPlayer.Ctlcontrols.currentPosition = shouldResetUIPlayerPosition ? 0 : soundEvent.MediaPlayer.Position;
            uiSoundPlayer.settings.volume = 0;
            uiSoundPlayer.Ctlenabled = false;
            uiSoundPlayer.Ctlcontrols.play();
        }

        private void SelectEventNode(TreeNode eventNode, bool resetUIPlayerPos)
        {
            if (resetUIPlayerPos) shouldResetUIPlayerPosition = true;
            soundEventsListBox.SelectedNode = null;
            soundEventsListBox.SelectedNode = eventNode;
            shouldResetUIPlayerPosition = false;
        }

        private void SelectCurrentlyActivatedEventNode()
        {
            TreeNode currActivatedEventNode = soundEvents.LastOrDefault(i => i.Activated)?.EventNode;
            SelectEventNode(currActivatedEventNode ?? soundEvents[0].EventNode, true);
        }

        private void UpdateGameState()
        {
            masterVolume = GetCurrentVolume(0x20, null, masterVolume);
            foreach (SoundEvent soundEvent in soundEvents)
            {
                soundEvent.Activated = soundEvent.IsActivated();
                soundEvent.SetEventNodeIcon(soundEvent.Activated ? 2 : 1);
            }
            foreach (SoundEvent soundEvent in soundEvents)
            {
                soundEvent.Volume = GetCurrentVolume(soundEvent.Type, soundEvent);
                if (soundEvent.ShouldStopEvent())
                {
                    soundEvent.StopEvent();
                    SelectCurrentlyActivatedEventNode();
                }
                if (!soundEvent.ShouldPlayEvent()) continue;
                soundEvent.PlayEvent();
                SelectEventNode(soundEvent.EventNode, true);
            }
        }

        private void BeginGameStateTimer()
        {
            gameStateTimer.Tick += (s, e) =>
            {
                if (soundEventFadeTimer.Enabled) return;
                if (gameDataMan == 0 && gameDataManLookupAvailable) SetGameDataMan();
                UpdateGameState();
                if (!mainGameProcess.HasExited) return;
                if (runtimeSettings.UsesNativeFlagBridge) NightreignFlagBridge.Close();
                Environment.Exit(0);
            };
            // TODO: Double check
            // gameStateTimer.Interval = 10;
            gameStateTimer.Start();
        }

        private void CMI_Click(object sender, EventArgs e)
        {
            statusLogGroupBox.Focus();
        }

        private async void CMI_Shown(object sender, EventArgs e)
        {
#if HIDE_WINDOW
    Hide();
#endif
            if (!ReadSoundJSON()) return;
            LoadSoundEvents();
            await AttachToGame();
        }

        private void SoundEventsListBox_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null) UpdateUISoundPlayerState(soundEvents[e.Node.Index]);
        }

#if IS_EXECUTABLE
        [STAThread]
#endif
        public static void Main()
        {
            RunApplication(null);
        }

        public static int StartFromNative(string appRootOverride)
        {
            lock (nativeStartLock)
            {
                if (nativeStarted) return 0;
                nativeStarted = true;
            }

            Thread uiThread = new Thread(() => RunApplication(appRootOverride))
            {
                IsBackground = false,
                Name = "CMI Nightreign UI"
            };
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            return 0;
        }

        private static void RunApplication(string appRootOverride)
        {
            try
            {
                string location = Assembly.GetExecutingAssembly().Location;
                appRootPath = string.IsNullOrWhiteSpace(appRootOverride)
                    ? $"{Path.GetDirectoryName(location)}"
                    : appRootOverride;
                runtimeSettings = RuntimeSettings.Load(appRootPath);
                modSoundFolderPath = Path.Combine(appRootPath, runtimeSettings.SoundFolder);
                soundJsonFilePath = Path.Combine(appRootPath, runtimeSettings.SoundJson);
            }
            catch
            {
                Environment.Exit(0);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // TODO: Figure out what's causing the textbox dispose exception...
            cmi = new CMI();
            Application.Run(cmi);
        }

        /*
        private static bool IsLoadingScreenActive()
        {
            ProcessModule mainModule = mainGameProcess.MainModule;
            return mainModule != null && ReadIsLoadingScreenActive(mainModule.BaseAddress, 0x728);
        }
        */

        private static int ReadOptionalHexInt(JObject obj, string key)
        {
            JToken token = obj.GetValue(key);
            if (token == null) return 0;
            string value = token.ToString();
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(2);
            return int.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        private static int ReadOptionalInt(JObject obj, string key)
        {
            JToken token = obj.GetValue(key);
            return token == null || string.IsNullOrWhiteSpace(token.ToString()) ? 0 : Convert.ToInt32(token.ToString());
        }

        private static int? ReadOptionalNullableInt(JObject obj, string key)
        {
            JToken token = obj.GetValue(key);
            return token == null || string.IsNullOrWhiteSpace(token.ToString()) ? (int?)null : Convert.ToInt32(token.ToString());
        }

        private static bool ReadOptionalBool(JObject obj, string key)
        {
            JToken token = obj.GetValue(key);
            return token != null && bool.Parse(token.ToString());
        }

        private static bool ReadEventFlag(int eventFlagId)
        {
            if (!runtimeSettings.UsesNativeFlagBridge)
            {
                string disabledStatus = $"EventFlagId {eventFlagId} is ignored because EventFlagIdReader is {runtimeSettings.EventFlagIdReader}.";
                if (disabledStatus != lastFlagBridgeStatus)
                {
                    lastFlagBridgeStatus = disabledStatus;
                    cmi?.SendStatusLogMessage(disabledStatus);
                }

                return false;
            }

            bool active;
            if (NightreignFlagBridge.TryReadFlag(eventFlagId, out active))
                return active;

            string status = NightreignFlagBridge.LastStatus;
            if (status != lastFlagBridgeStatus)
            {
                lastFlagBridgeStatus = status;
                cmi?.SendStatusLogMessage(status);
            }

            return false;
        }

        public class SoundEvent
        {
            public bool Activated { get; set; }
            public MediaPlayer MediaPlayer { get; set; }
            public string Name { get; set; }
            public TreeNode EventNode { get; set; }
            public int Volume { get; set; }
            public string SoundPath { get; set; }
            public int Pointer1 { get; set; }
            public int Pointer2 { get; set; }
            public int Startbit { get; set; }
            public int? EventFlagId { get; set; }
            public bool AlwaysActive { get; set; }
            public int Type { get; set; }
            public float FadeInSeconds { get; set; }
            public float FadeOutSeconds { get; set; }
            public bool FadeIntoNextTrack { get; set; }
            public bool Loop { get; set; }
            private readonly Timer fadeStopTimer = new Timer();
            private bool stopRequested;

            // TODO: Create a strings class for sound event property keys...

            public static SoundEvent Deserialize(string name, JObject soundEventJson)
            {
                SoundEvent soundEvent = new SoundEvent
                {
                    Name = name,
                    EventNode = new TreeNode { ImageIndex = 1, SelectedImageIndex = 1, Name = name, Text = name },
                    SoundPath = $"{modSoundFolderPath}\\{soundEventJson.GetValue("SoundPath")}",
                    Pointer1 = ReadOptionalHexInt(soundEventJson, "Pointer1"),
                    Pointer2 = ReadOptionalHexInt(soundEventJson, "Pointer2"),
                    Startbit = ReadOptionalInt(soundEventJson, "Startbit"),
                    EventFlagId = ReadOptionalNullableInt(soundEventJson, "EventFlagId"),
                    AlwaysActive = ReadOptionalBool(soundEventJson, "AlwaysActive"),
                    Type = Convert.ToInt32(soundEventJson.GetValue("Type").ToString()),
                    FadeInSeconds = Convert.ToSingle(soundEventJson.GetValue("FadeInSeconds").ToString()),
                    FadeOutSeconds = Convert.ToSingle(soundEventJson.GetValue("FadeOutSeconds").ToString()),
                    FadeIntoNextTrack = bool.Parse(soundEventJson.GetValue("FadeIntoNextTrack").ToString()),
                    Loop = bool.Parse(soundEventJson.GetValue("Loop").ToString())
                };
                soundEvent.MediaPlayer = soundEvent.GetMediaPlayer();
                return soundEvent;
            }

            public static JObject Serialize(SoundEvent soundEvent)
            {
                JObject soundEventJson = new JObject
                {
                    // TODO: Double check
                    ["SoundPath"] = Path.GetFileName(soundEvent.SoundPath),
                    ["Pointer1"] = $"0x{soundEvent.Pointer1:X}",
                    ["Pointer2"] = $"0x{soundEvent.Pointer2:X}",
                    ["Startbit"] = soundEvent.Startbit,
                    ["EventFlagId"] = soundEvent.EventFlagId.HasValue
                        ? new JValue(soundEvent.EventFlagId.Value)
                        : JValue.CreateNull(),
                    ["AlwaysActive"] = soundEvent.AlwaysActive,
                    ["Type"] = soundEvent.Type,
                    ["FadeInSeconds"] = soundEvent.FadeInSeconds,
                    ["FadeOutSeconds"] = soundEvent.FadeOutSeconds,
                    ["FadeIntoNextTrack"] = soundEvent.FadeIntoNextTrack,
                    ["Loop"] = soundEvent.Loop
                };
                return soundEventJson;
            }

            public bool IsActivated()
            {
                if (AlwaysActive) return true;
                if (EventFlagId.HasValue) return ReadEventFlag(EventFlagId.Value);
                if (eventFlagMan == IntPtr.Zero) return false;
                byte eventByte = ReadByte((IntPtr)ReadLong((IntPtr)ReadLong(eventFlagMan) + Pointer1) + Pointer2);
                char[] paddedEventBytes = Convert.ToString(eventByte, 2).PadLeft(8, '0').ToCharArray().Reverse().ToArray();
                return int.Parse(new string(paddedEventBytes).Substring(Startbit, 1)) == 1;
            }

            public void SetEventNodeIcon(int iconIndex)
            {
                if (EventNode.ImageIndex == iconIndex) return;
                EventNode.ImageIndex = iconIndex;
                EventNode.SelectedImageIndex = iconIndex;
            }

            public bool DoesOtherEventOverride()
            {
                SoundEvent overrideSoundEvent = soundEvents.LastOrDefault(i => i.Name != Name && i.Activated && i.Type == Type);
                return soundEvents.IndexOf(overrideSoundEvent) >= soundEvents.IndexOf(this);
            }

            public bool ShouldStopEvent()
            {
                return cooldownTimer.Enabled
                    || !MediaPlayer.inFade && IsHPZero()
                    || IsHPInvalid()
                    || !Activated && MediaPlayer.CurrentSong == SoundPath && !stopRequested;
            }

            public bool ShouldPlayEvent()
            {
                return !IsHPZero()
                    && !IsHPInvalid()
                    && Activated
                    && !DoesOtherEventOverride()
                    && (MediaPlayer.CurrentSong != SoundPath || stopRequested);
            }

            public void StopEvent()
            {
                bool isHPZero = IsHPZero();
                if (!cooldownTimer.Enabled && !IsHPInvalid() && !isHPZero && FadeIntoNextTrack) return;
                bool shouldStopImmediately = cooldownTimer.Enabled || IsHPInvalid() || !isHPZero && FadeOutSeconds == 0;
                if (stopRequested && !shouldStopImmediately) return;
                stopRequested = true;
                if (shouldStopImmediately)
                {
                    CompleteStop();
                    if (cooldownTimer.Enabled || !IsHPInvalid()) return;
                    cooldownTimer.Interval = 10000;
                    cooldownTimer.AutoReset = false;
                    cooldownTimer.Start();
                }
                else
                {
                    float fadeOutSeconds = isHPZero ? 5 : FadeOutSeconds;
                    MediaPlayer.FadeTime = fadeOutSeconds;
                    MediaPlayer.Fade(0);
                    fadeStopTimer.Stop();
                    fadeStopTimer.Elapsed -= FadeStopTimerOnElapsed;
                    fadeStopTimer.Elapsed += FadeStopTimerOnElapsed;
                    fadeStopTimer.Interval = Math.Max(1, fadeOutSeconds * 1000);
                    fadeStopTimer.AutoReset = false;
                    fadeStopTimer.Start();
                }
            }

            public void PlayEvent()
            {
                CancelPendingStop();
                MediaPlayer.Player1.settings.setMode("loop", Loop);
                MediaPlayer.Player2.settings.setMode("loop", Loop);
                MediaPlayer.Crossfade = true;
                MediaPlayer.FadeTime = FadeInSeconds;
                MediaPlayer.CrossfadeTime = FadeInSeconds;
                MediaPlayer.Play(SoundPath, FadeInSeconds > 0);
                // TODO: We also need to correctly set the UI player position...
            }

            private void CancelPendingStop()
            {
                stopRequested = false;
                fadeStopTimer.Stop();
                fadeStopTimer.Elapsed -= FadeStopTimerOnElapsed;
            }

            private void FadeStopTimerOnElapsed(object s, ElapsedEventArgs e)
            {
                CompleteStop();
            }

            private void CompleteStop()
            {
                fadeStopTimer.Stop();
                fadeStopTimer.Elapsed -= FadeStopTimerOnElapsed;
                if (MediaPlayer.CurrentSong == SoundPath)
                {
                    MediaPlayer.CurrentSong = null;
                    MediaPlayer.Stop(false);
                }
                stopRequested = false;
            }

            private MediaPlayer GetMediaPlayer()
            {
                MediaPlayer mediaPlayer;
                switch (Type)
                {
                    case 4:
                        mediaPlayer = musicMediaPlayer;
                        break;
                    case 5:
                        mediaPlayer = soundEffectsMediaPlayer;
                        break;
                    case 6:
                        mediaPlayer = voiceMediaPlayer;
                        break;
                    default:
                        mediaPlayer = null;
                        break;
                }
                return mediaPlayer;
            }
        }

        private void AddEventButton_Click(object sender, EventArgs e)
        {
            AddEventForm form = new AddEventForm();
            form.ShowDialog();
        }

        private void SoundEventsListBox_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode draggedNode = (TreeNode)e.Item;
            if (draggedNode.Parent == null) DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void SoundEventsListBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void SoundEventsListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            Point pt = soundEventsListBox.PointToClient(new Point(e.X, e.Y));
            TreeNode node = soundEventsListBox.GetNodeAt(pt);
            soundEventsListBox.SelectedNode = node;
        }

        public static void CommitUpdatedSoundEvents()
        {
            soundJson = new JObject();
            foreach (SoundEvent currSoundEvent in soundEvents)
            {
                JObject obj = SoundEvent.Serialize(currSoundEvent);
                if (soundJson.ContainsKey(currSoundEvent.Name))
                {
                    int index = soundJson.IndexOf(currSoundEvent.Name);
                    JObject soundEventJson = new JObject { [currSoundEvent.Name] = obj };
                    soundJson.Remove(currSoundEvent.Name);
                    soundJson.InsertAt(index, soundEventJson);
                }
                else soundJson.Add(currSoundEvent.Name, obj);
            }
            string json = soundJson.ToString(Formatting.Indented);
            File.WriteAllText(soundJsonFilePath, json);
            cmi.LoadSoundEvents();
        }

        private void SoundEventsListBox_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = soundEventsListBox.PointToClient(new Point(e.X, e.Y));
            TreeNode destinationNode = soundEventsListBox.GetNodeAt(pt);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (draggedNode == null || destinationNode == null || draggedNode == destinationNode) return;
            int destIndex = destinationNode.Index;
            int sourceIndex = draggedNode.Index;
            if (destIndex == sourceIndex) return;
            // TODO: Function
            SoundEvent soundEvent = soundEvents[sourceIndex];
            soundEvents.RemoveAt(sourceIndex);
            soundEvents.Insert(destIndex, soundEvent);
            CommitUpdatedSoundEvents();
        }

        private void SoundEventsListBox_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            soundEventsListBox.SelectedNode = e.Node;
            soundEventRightClickMenu.Show(soundEventsListBox, e.Location);
        }

        private void SoundEventDeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (soundEventsListBox.SelectedNode == null) return;
            soundEvents.RemoveAt(soundEventsListBox.SelectedNode.Index);
            CommitUpdatedSoundEvents();
        }

        private void SoundEventEditMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = soundEventsListBox.SelectedNode;
            if (node == null) return;
            AddEventForm form = new AddEventForm(true, soundEvents[node.Index]);
            form.ShowDialog();
        }
    }
}
