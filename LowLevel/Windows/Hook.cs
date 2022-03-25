using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyCap.LowLevel.Windows {
    
    /// <summary>
    /// Stores information about an installed windows hook.
    /// </summary>
    internal sealed class Hook : IDisposable {

        /// <summary>
        /// The type of hook this instance handles.
        /// </summary>
        public HookType Type { get; private set; }
        /// <summary>
        /// The native Thread ID that this hook is installed on.
        /// </summary>
        public uint NativeThreadID { get; private set; }
        /// <summary>
        /// The handle of the installed hook.
        /// </summary>
        public IntPtr Handle { get; private set; }
        /// <summary>
        /// The callback method associated with this hook.
        /// </summary>
        public WinApi.LLCallback Callback { get; private set; }
        /// <summary>
        /// Whether this hook is currently installed and active.
        /// </summary>
        public bool Active { get => !disposedValue; }

        private bool disposedValue;

        /// <summary>
        /// Creates a new hook with the defined type and callback method.
        /// </summary>
        public static Hook Create(HookType hType, WinApi.LLCallback hMethod) {
            return new Hook(hType, hMethod);
        }

        Hook(HookType hType, WinApi.LLCallback hMethod) {
            Handle = WinApi.SetWindowsHookEx(
                (int)hType,
                hMethod,
                WinApi.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                0
            );

            Callback = hMethod;
            Type = hType;
            NativeThreadID = WinApi.GetCurrentThreadId();
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                WinApi.UnhookWindowsHookEx(Handle);
                disposedValue = true;
            }
        }

        ~Hook() {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Uninstalls the hook held by this instance, releasing all unmanaged resources.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
