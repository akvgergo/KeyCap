using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using KeyCap.LowLevel.Win32;

namespace KeyCap.Keyboard {

    /// <summary>
    /// Globally listens to keyboard events through a primitive message loop.
    /// </summary>
    /// <remarks>
    /// This class implements a message loop and a hook, allowing you to be the source of all events for
    /// an entire application, or for a single thread.
    /// 
    /// If your application is using a GUI framework such as WinForms or WPF,
    /// consider using <see cref="KeyListener"/> instead.
    /// </remarks>
    public sealed class KeyListenerApp : IKeyEventSource, IDisposable {

        bool _inMsgLoop;
        bool _active;
        WndProc _proc;
        HashSet<Keys> _keyboardState = new HashSet<Keys>();
        Hook _hook;
        bool _disposedValue;

        /// <summary>
        /// Raised for any keyboard input.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyEvent;

        /// <summary>
        /// Whether the current instance is firing events.
        /// </summary>
        public bool Active { get => _hook != null && _hook.Active && _active; }

        /// <summary>
        /// Creates a new instance with the specified window procedure,
        /// and installs a hook on the owner thread.
        /// </summary>
        public KeyListenerApp(WndProc proc) {
            _proc = proc;
            _hook = Hook.Create(HookType.WH_KEYBOARD_LL, HookCallback);
        }

        /// <summary>
        /// Starts the message loop and the firing of key events.
        /// </summary>
        public void Start() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = true;
            _inMsgLoop = true;
            Win32Interop.RunMessageLoop(_proc);
            _inMsgLoop = false;
        }

        /// <summary>
        /// Pauses the firing of key events.
        /// </summary>
        public void Pause() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = false;
        }

        /// <summary>
        /// Posts a WM_QUIT message to the message loop, causing it to exit.
        /// </summary>
        public void Exit() {
            if (_disposedValue) throw new InvalidOperationException();
            if (_inMsgLoop) WinApi.PostQuitMessage(0);
        }

        /// <summary>
        /// GC
        /// </summary>
        ~KeyListenerApp() {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Finalizes this instance, removing all listeners, and destroying the installed hook.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    KeyEvent = null;
                }

                _hook?.Dispose();
                if (_inMsgLoop) {
                    Win32Interop.DoEvents(_proc);
                    Exit();
                }
                
                _disposedValue = true;
            }
        }

        /// <summary>
        /// The callback method installed by the hook.
        /// </summary>
        /// <remarks>
        /// This method is called whenever the thread that owns the hook calls GetMessage().
        /// When that happens, if there are key events to be processed, this method is called on our thread,
        /// before retrieving a message.
        /// </remarks>
        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0 || !_active)
                return WinApi.CallNextHookEx(_hook.Handle, nCode, wParam, lParam);
            
            var wke = (Win32KeyEvent)wParam.ToInt32();
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            
            KeyEventType ket;
            
            //this indicates that another app with a hook cancelled
            //the key press, so we should skip all processing
            //the hook doesn't actually have any data indicating whether it's a
            //new key press, or a single key being held down, so we need to keep track
            //ourselves
            if (wke == Win32KeyEvent.WM_KEYDOWN || wke == Win32KeyEvent.WM_SYSKEYDOWN) {
                if (!_keyboardState.Contains((Keys)hookStruct.vkCode)) {
                    _keyboardState.Add((Keys)hookStruct.vkCode);
                    ket = KeyEventType.KeyDown;
                } else {
                    ket = KeyEventType.KeyHold;
                }
            } else {
                _keyboardState.Remove((Keys)hookStruct.vkCode);
                ket = KeyEventType.KeyUp;
            }

            //Character input handling is currently done in the event class, but it's
            //a good candidate to be moved here
            var eventArgs = new KeyboardEventArgs(
                ket,
                (Keys)hookStruct.vkCode,
                hookStruct.scanCode,
                _keyboardState.ToArray()
            );

            KeyEvent?.Invoke(this, eventArgs);

            //If the event was cancelled, not calling CallNextHook() will prevent
            //the event from reaching other hooks and the target window.
            if (eventArgs.Handled) return new IntPtr(1);

            return WinApi.CallNextHookEx(_hook.Handle, eventArgs.Handled ? -1 : nCode, wParam, lParam);
        }

    }
}