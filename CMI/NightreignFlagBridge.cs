using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CMI
{
    internal static class NightreignFlagBridge
    {
        private const uint BridgeMagic = 0x46494D43;
        private const uint BridgeVersion = 1;
        private const int HeaderSize = 32;
        private const int SlotSize = 16;
        private const int MaxSlots = 64;
        private const int FileMapAllAccess = 0xF001F;
        private const string MappingName = @"Local\CMI_Nightreign_FlagBridge";

        private static readonly Dictionary<int, int> SlotByFlagId = new Dictionary<int, int>();
        private static IntPtr mappingHandle = IntPtr.Zero;
        private static IntPtr view = IntPtr.Zero;
        private static string lastStatus = "Not connected";

        public static string LastStatus => lastStatus;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenFileMapping(int desiredAccess, bool inheritHandle, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr fileMappingObject, int desiredAccess, uint fileOffsetHigh, uint fileOffsetLow, UIntPtr bytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr baseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        public static bool TryReadFlag(int eventFlagId, out bool active)
        {
            active = false;
            if (eventFlagId <= 0)
            {
                lastStatus = $"Invalid EventFlagId {eventFlagId}";
                return false;
            }

            if (!EnsureMapped())
                return false;

            if (!ValidateHeader())
                return false;

            int slot = GetOrCreateSlot(eventFlagId);
            if (slot < 0)
            {
                lastStatus = "No free flag bridge slots";
                return false;
            }

            int state = ReadInt32(SlotOffset(slot) + 4);
            switch (state)
            {
                case 2:
                    active = true;
                    lastStatus = $"EventFlagId {eventFlagId} is ON";
                    return true;
                case 1:
                    active = false;
                    lastStatus = $"EventFlagId {eventFlagId} is off";
                    return true;
                case -1:
                    lastStatus = BridgeStatusText(ReadUInt32(8), eventFlagId);
                    return false;
                default:
                    lastStatus = $"EventFlagId {eventFlagId} pending";
                    return false;
            }
        }

        public static void Close()
        {
            if (view != IntPtr.Zero)
            {
                UnmapViewOfFile(view);
                view = IntPtr.Zero;
            }

            if (mappingHandle != IntPtr.Zero)
            {
                CloseHandle(mappingHandle);
                mappingHandle = IntPtr.Zero;
            }

            SlotByFlagId.Clear();
        }

        private static bool EnsureMapped()
        {
            if (view != IntPtr.Zero)
                return true;

            mappingHandle = OpenFileMapping(FileMapAllAccess, false, MappingName);
            if (mappingHandle == IntPtr.Zero)
            {
                lastStatus = "Flag bridge is not available yet";
                return false;
            }

            view = MapViewOfFile(mappingHandle, FileMapAllAccess, 0, 0, UIntPtr.Zero);
            if (view != IntPtr.Zero)
                return true;

            lastStatus = "Failed to map flag bridge";
            CloseHandle(mappingHandle);
            mappingHandle = IntPtr.Zero;
            return false;
        }

        private static bool ValidateHeader()
        {
            uint magic = ReadUInt32(0);
            uint version = ReadUInt32(4);
            uint slots = ReadUInt32(12);

            if (magic == BridgeMagic && version == BridgeVersion && slots >= 1)
                return true;

            lastStatus = "Flag bridge header is invalid";
            Close();
            return false;
        }

        private static int GetOrCreateSlot(int eventFlagId)
        {
            if (SlotByFlagId.TryGetValue(eventFlagId, out int existing))
                return existing;

            for (int i = 0; i < MaxSlots; i++)
            {
                int offset = SlotOffset(i);
                int currentFlagId = ReadInt32(offset);
                if (currentFlagId == eventFlagId)
                {
                    SlotByFlagId[eventFlagId] = i;
                    return i;
                }

                if (currentFlagId != 0)
                    continue;

                WriteInt32(offset, eventFlagId);
                WriteInt32(offset + 4, 0);
                SlotByFlagId[eventFlagId] = i;
                return i;
            }

            return -1;
        }

        private static string BridgeStatusText(uint bridgeStatus, int eventFlagId)
        {
            switch (bridgeStatus)
            {
                case 1:
                    return $"CSEventFlagMan exists but is not initialized yet for EventFlagId {eventFlagId}";
                case 2:
                    return $"EventFlagId {eventFlagId} could not be read";
                case 3:
                    return "CSEventFlagMan singleton was not found by the native bridge";
                default:
                    return "Flag bridge is not ready";
            }
        }

        private static int SlotOffset(int slot)
        {
            return HeaderSize + slot * SlotSize;
        }

        private static int ReadInt32(int offset)
        {
            return Marshal.ReadInt32(view, offset);
        }

        private static uint ReadUInt32(int offset)
        {
            return unchecked((uint)Marshal.ReadInt32(view, offset));
        }

        private static void WriteInt32(int offset, int value)
        {
            Marshal.WriteInt32(view, offset, value);
        }
    }
}
