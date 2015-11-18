﻿using AccessProcessMemory;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GTAVC_Chaos
{
    static class ProcessHandlerApi
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] // GetClassName
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int MaxCount);
    }

    class Game
    {
        private long versionAddress;
        private string baseVersion;

        private GameVersion memoryAddressesVersion;

        public string name;
        public string abbreviation;
        public string windowName;
        public string windowClass;

        public Memory memory;
        public GameVersion currentVersion;

        private GameVersion[] gameVersions;
        public MemoryAddress[] memoryAddresses;
        private Limitation[] limitations;

        public Modules modules;

        public bool IsRunning
        {
            get { return memory != null && memory.HasValidProcess(); }
        }

        public Game(string name, string abbreviation, string windowName, string windowClass, long versionAddress, string baseVersion, GameVersion[] gameVersions)
        {
            this.name = name;
            this.abbreviation = abbreviation;
            this.windowName = windowName;
            this.windowClass = windowClass;
            this.versionAddress = versionAddress;
            this.baseVersion = baseVersion;
            this.gameVersions = gameVersions;
        }

        public void SetMemoryAddresses(MemoryAddress[] memoryAddresses, string gameVersionName)
        {
            this.memoryAddresses = memoryAddresses;
            this.memoryAddressesVersion = FindGameVersionByName(gameVersionName);

            // NOTE(Ligh): Get a pointer to the base address for all dynamic addresses.
            foreach (MemoryAddress address in Array.FindAll(memoryAddresses, m => m.address == 0))
            {
                address.ResolveBaseAddress();
            }
        }

        public void InitializeMemoryAddressesForVersion()
        {
            for (int i = 0; i < this.memoryAddresses.Length; i++)
            {
                this.memoryAddresses[i].SetMemoryHandle(memory);

                if (this.memoryAddresses[i].address != 0)
                {
                    // NOTE(Ligh): Static address
                    this.memoryAddresses[i].UpdateForVersion(currentVersion, memoryAddressesVersion);
                }
            }
        }

        public void SetLimitations(Limitation[] limitations)
        {
            this.limitations = limitations;

            foreach (Limitation limitation in limitations)
            {
                for (int i = 0; i < limitation.checks.Length; i++)
                {
                    if (limitation.checks[i] is LimitationCheck)
                    {
                        (limitation.checks[i] as LimitationCheck).ResolveLimitation();
                    }
                }
            }

        }

        public void GetHandle()
        {
            Debug.WriteLine("Starting attempts to get game handle");
            OpenProcess();
            if (memory != null)
            {
                Debug.WriteLine("Game handle found");
                InitVersion();
                InitializeMemoryAddressesForVersion();
            }
            else
            {
                Debug.WriteLine("Search for game handle aborted");
                Thread.CurrentThread.Abort();
            }
        }

        /// <summary>
        /// This function gets all processes. It will then check if any of the processes has the right
        /// window name and window class. If this is the case, it will instantiate the Memory class for
        /// the (first) matching process to be used for reading and writing the memory of the process.
        /// If no matching process is found, the method will sleep and keep trying until a matching process is found.
        /// </summary>
        private void OpenProcess()
        {
            do
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.MainWindowTitle == windowName)
                    {
                        StringBuilder foundClassName = new StringBuilder();
                        ProcessHandlerApi.GetClassName(process.MainWindowHandle, foundClassName, windowClass.Length + 1);
                        if (foundClassName.ToString() == windowClass)
                        {
                            memory = new Memory(process);
                            break;
                        }
                    }
                }
                Thread.Sleep(Settings.DEFAULT_WAIT_TIME);
            } while (memory == null && Program.shouldStop == false);
        }

        public void CloseProcess()
        {
            // TODO(Ligh): Deactivate active effects here.

            memory.CloseProcess();
            memory = null;
            currentVersion = null;
        }

        public void InitVersion()
        {
            if (memory == null)
            {
                throw new Exception("Tried to determine the version without a handle to the game.");
            }

            byte value = memory.Read(versionAddress, "byte", 1);

            foreach (GameVersion gameVersion in gameVersions)
            {
                if (gameVersion.addressValue == value)
                {
                    Debug.WriteLine("Detected game version: " + gameVersion.name);
                    currentVersion = gameVersion;
                    break;
                }
            }

            if (currentVersion == null)
            {
                throw new Exception("Failed to determine the game version: Unknown version. Version address value was " + value);
            }
        }

        public void InitModules()
        {
            GetHandle();

            modules = new Modules();

            // TODO(Ligh): The name of this method does not match its behaviour.
            while (this.IsRunning && !Program.shouldStop)
            {
                modules.Update();
            }

            if (this.IsRunning)
            {
                // The modules affect the game state, so if the game is still running,
                // we shut them down to restore the game to its unaltered state.
                modules.Shutdown();
            }

            CloseProcess();
        }

        public MemoryAddress FindMemoryAddressByName(string name)
        {
            // TODO(Ligh): Handle not found case.
            return Array.Find(memoryAddresses, p => p.name == name);
        }

        public Limitation FindLimitationByName(string name)
        {
            // TODO(Ligh): Handle not found case.
            return Array.Find(limitations, p => p.name == name);
        }

        public GameVersion FindGameVersionByName(string name)
        {
            // TODO(Ligh): Handle not found case.
            return Array.Find(gameVersions, p => p.name == name);
        }
    }
}
