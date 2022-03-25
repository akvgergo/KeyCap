using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyCap.LowLevel.Win32 {

    //These are only left public to save some boilerplate on the consumer side if there is some
    //need to handle a few special messages. Chances are, if you are using these extensively you should
    //be using a window framework. Disabling doc warnings because there isn't much sense in copy-pasting
    //msdn, you either know how to use these, or should be reading the docs anyway.
#pragma warning disable 1591

    /// <summary>
    /// Represents a windows procedure, aka. an action that is called when there is a new 
    /// message retrieved from the message queue.
    /// </summary>
    /// <param name="msg"></param>
    public delegate void WndProc(MSG msg);

    /// <summary>
    /// Represents a win32 message structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public int time;
        public Point pt;
        public int lPrivate;
    }

    /// <summary>
    /// Represents a win32 point structure.
    /// </summary>
    /// <remarks>
    /// For the sake of simplicity, this is used for both interop and public facing methods.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point {
        public int X;
        public int Y;

        public Point(int x, int y) {
            X = x;
            Y = y;
        }

        public override string ToString() {
            return $"{{{X}, {Y}}}";
        }
    }

    /// <summary>
    /// Collection of publicly visible win32 methods and structures.
    /// </summary>
    public static class Win32Interop {

        /// <summary>
        /// Enters a message loop on the calling thread, returning messages as they are received.
        /// </summary>
        /// <remarks>
        /// Doing proper message handling is out of scope for this Api, but this checks all the basics.
        /// </remarks>
        public static void RunMessageLoop(WndProc procedure = null) {
            while (WinApi.GetMessage(out MSG message, IntPtr.Zero, 0, 0) != 0) {
                WinApi.TranslateMessage(ref message);
                WinApi.DispatchMessage(ref message);

                procedure?.Invoke(message);
            }
        }

        /// <summary>
        /// Processes all the messages in our queue before returning.
        /// </summary>
        /// <remarks>
        /// This method explicitly doesn't process WM_QUIT messages, so that the main loop
        /// can process it.
        /// </remarks>
        public static void DoEvents(WndProc procedure = null) {
            while (WinApi.PeekMessage(out MSG message, IntPtr.Zero, 0, 0, 0) && message.message != 0x0012) {
                WinApi.GetMessage(out message, IntPtr.Zero, 0, 0);
                WinApi.TranslateMessage(ref message);
                WinApi.DispatchMessage(ref message);

                procedure?.Invoke(message);
            }
        }

    }
}
