﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AccessProcessMemory
{
    static class AccessProcessMemoryApi
    {
        public const UInt32 PROCESS_ALL_ACCESS = 0x001F0FFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            UInt32 dwDesiredAccess,
            bool bInheritHandle,
            Int32 dwProcessId
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [In, Out] byte[] lpBuffer,
            IntPtr dwSize,
            out IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            IntPtr dwSize,
            out IntPtr lpNumberOfBytesWritten
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 CloseHandle(
            IntPtr hObject
            );

    }

    /// <summary>
    /// Class containing methods to read and write to the memory of the targetProcess.
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// Constructor to initiate Memory class without a target process.
        /// </summary>
        public Memory()
        {

        }

        /// <summary>
        /// Constructor to initiate Memory class with a target process.
        /// </summary>
        public Memory(Process process)
        {
            targetProcess = process;
        }

        /// <summary>
        /// Process that the functions in this library will target.
        /// </summary>
        public Process targetProcess
        {
            get
            {
                return m_Process;
            }
            set
            {
                m_Process = value;
            }
        }

        // Private vars containing the target process and its handle.
        private Process m_Process = null;
        private IntPtr m_ProcessHandle = IntPtr.Zero;

        /// <summary>
        /// Gets the process handle for m_Process. Needs to be called before accessing the memory.
        /// </summary>
        public void OpenProcess()
        {
            m_ProcessHandle = AccessProcessMemoryApi.OpenProcess(AccessProcessMemoryApi.PROCESS_ALL_ACCESS, true, m_Process.Id);
            if (m_ProcessHandle == IntPtr.Zero)
                throw new Exception("OpenProcess failed");
        }

        /// <summary>
        /// Close the handle to the process.
        /// </summary>
        public void CloseProcess()
        {
            int iRetValue = AccessProcessMemoryApi.CloseHandle(m_ProcessHandle);
            if (iRetValue == 0)
                throw new Exception("CloseProcess failed");
            Debug.WriteLine("Game handle closed");
        }

        /// <summary>
        /// Read [length] bytes at [address] in the current targetProcess. The byte array is then 
        /// converted to type [T] before being returned. length defaults to 4.
        /// </summary>
        public T Read<T>(long address, int length = 4)
        {
            OpenProcess();
            byte[] buffer = new byte[length];
            IntPtr ptrBytesReaded;
            AccessProcessMemoryApi.ReadProcessMemory(m_ProcessHandle, (IntPtr)address, buffer, (IntPtr)length, out ptrBytesReaded);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error()); // Throw exception if error occurred
            if (buffer == null)
                throw new Exception("Error while reading memory, no memory read");
            T result = ConvertOutput<T>(buffer);
            return result;
        }

        /// <summary>
        /// Convert the input of type T to a byte array of [length]. If the input is shorter, the rest of the 
        /// array will be filled with zeros. Length defaults to the length of the input after conversion
        /// to byte array. Write the fullInput byte array to [address] in the current targetProcess.
        /// </summary>
        public void Write<T>(long address, T input, int length = int.MinValue)
        {
            OpenProcess();
            if (input == null)
                throw new Exception("Error while writing memory, no input provided");
            string dataType = typeof(T).Name;
            byte[] byteInput = ConvertInput<T>(input);
            if (length == int.MinValue)
                length = byteInput.Length;
            byte[] fullInput = new byte[length];
            Array.Copy(byteInput, fullInput, byteInput.Length);

            IntPtr ptrBytesWritten;
            AccessProcessMemoryApi.WriteProcessMemory(m_ProcessHandle, (IntPtr)address, fullInput, (IntPtr)length, out ptrBytesWritten);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error()); // Throw exception if error occurred
        }

        /// <summary>
        /// Convert byte array to the type given by the targetDataType parameter. If the parameter
        /// contains an unimplemented type an exception is thrown. The byte array is automatically 
        /// converted to little endian if necessary.
        /// </summary>
        private T ConvertOutput<T>(byte[] output)
        {
            if (output == null)
                throw new Exception("Error while converting output from memory, no output");
            string targetDataType = typeof(T).Name;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(output); // Convert big endian to little endian.

            dynamic result;
            targetDataType = targetDataType.ToLowerInvariant();
            switch (targetDataType)
            {
                case "bool":
                    result = BitConverter.ToBoolean(output, 0);
                    break;
                case "byte":
                    result = output[0];
                    break;
                case "short":
                    result = BitConverter.ToInt16(output, 0);
                    break;
                case "int":
                    result = BitConverter.ToInt32(output, 0);
                    break;
                case "long":
                    result = BitConverter.ToInt64(output, 0);
                    break;
                case "float":
                    result = BitConverter.ToSingle(output, 0);
                    break;
                case "double":
                    result = BitConverter.ToDouble(output, 0);
                    break;
                case "ascii":
                    result = Encoding.ASCII.GetString(output);
                    break;
                case "unicode":
                    result = Encoding.Unicode.GetString(output);
                    break;
                default:
                    throw new Exception(String.Format("Tried to convert memory reading to unknown dataType {0}", targetDataType));
            }
            return result;
        }

        /// <summary>
        /// Convert the type given by the targetDataType parameter to a byte array. If the parameter
        /// contains an unimplemented type an exception is thrown. The byte array is automatically 
        /// converted to little endian if necessary.
        /// </summary>
        byte[] ConvertInput<T>(dynamic input)
        {
            if (input == null)
                throw new Exception("Error while converting input for memory, no input");
            string originalDataType = typeof(T).Name;
            byte[] result;

            originalDataType = originalDataType.ToLowerInvariant();
            switch (originalDataType)
            {
                case "bool":
                    result = BitConverter.GetBytes(input);
                    break;
                case "byte":
                    result = new byte[]{input};
                    break;
                case "short":
                    result = BitConverter.GetBytes(input);
                    break;
                case "int":
                    result = BitConverter.GetBytes(input);
                    break;
                case "long":
                    result = BitConverter.GetBytes(input);
                    break;
                case "float":
                    result = BitConverter.GetBytes(input);
                    break;
                case "double":
                    result = BitConverter.GetBytes(input);
                    break;
                case "ascii":
                    result = Encoding.ASCII.GetBytes(input);
                    break;
                case "unicode":
                    result = Encoding.Unicode.GetBytes(input);
                    break;
                default:
                    throw new Exception(String.Format("Tried to convert memory input to unknown dataType {0}", originalDataType));
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result); // Convert big endian to little endian.

            return result;
        }
    }
}