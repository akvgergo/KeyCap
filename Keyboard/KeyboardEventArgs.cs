using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyCap.LowLevel.Win32;

namespace KeyCap.Keyboard {
    
    /// <summary>
    /// Arguments for low level keyboard events.
    /// </summary>
    public class KeyboardEventArgs : EventArgs {

        /// <summary>
        /// Required for calling ToUnicode().
        /// </summary>
        /// <remarks>
        /// Always resetting every important key is the simplest approach,
        /// and that also means we don't have to create a brand new array every time.
        /// As a downside, this has to be marked using <see cref="ThreadStaticAttribute"/>, in case
        /// there are multiple threads using their own hooks.
        /// </remarks>
        [ThreadStatic]
        static byte[] _kbState;

        /// <summary>
        /// The size of the buffer passed to ToUnicode(). 8 seems overkill, but I've seen some shit 'aight.
        /// </summary>
        const int _capacity = 8;

        /// <summary>
        /// The type of change that occured causing this event.
        /// </summary>
        public KeyEventType EventType { get; }

        /// <summary>
        /// The exact key that changed state, causing this event.
        /// </summary>
        public Keys EventKeyCode { get; }

        /// <summary>
        /// The hardware scan code associated with the key that caused this event.
        /// </summary>
        public uint ScanCode { get; }

        /// <summary>
        /// All the characters that this key event will input.
        /// </summary>
        /// <remarks>
        /// Note that this is a string because a key event may cause 0 or multiple character inputs,
        /// eg. pressing a dead key twice will not produce input for the first press, but the second press
        /// will trigger both inputs. Same for a dead key that is followed by a character that the dead key cannot modify.
        /// </remarks>
        public string Characters { get; } = "";

        /// <summary>
        /// Indicates whether this key event caused a character input in the foregground window.
        /// </summary>
        public bool IsCharInput { get => !string.IsNullOrEmpty(Characters); }

        /// <summary>
        /// All the keys held down during this event.
        /// </summary>
        public KeyboardState KeyboardState { get; }

        /// <summary>
        /// If set to true, this key press will be prevented from reaching the foreground window.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyboardEventArgs"/> class.
        /// </summary>
        public KeyboardEventArgs(KeyEventType eventType, Keys eventKey, uint scanCode, Keys[] keyboardState) {
            EventType = eventType;
            EventKeyCode = eventKey;
            KeyboardState = new KeyboardState(keyboardState);
            ScanCode = scanCode;

            /*  
                This part figures out whether this key event caused character input.

                On a side note, it would be beneficial to do a lazily initialized
                approach for Characters { get; } and IsCharInput { get; }.
                The only concern with that is ToUnicode() relying on the keyboard
                buffer to do the conversion, meaning that calling it even slightly
                late could produce wildly wrong results, so better to
                do all this inside the hook for now.
            */
            if (EventType == KeyEventType.KeyUp) return;

            
            if (_kbState == null) {
                _kbState = new byte[256];
            }

            //It's a lot more efficient to keep track of this ourselves,
            //ToUnicode() wants the entire keyboard, but these are
            //all the keys that actually matter. Also, calling GetKeyboardState()
            //while it may seem easier, relies on a functional message
            //loop to do it's job, so better to avoid that.
            _kbState[(int)Keys.SHIFT] = KeyboardState.Shift ? (byte)128 : (byte)0;
            _kbState[(int)Keys.CONTROL] = KeyboardState.Ctrl ? (byte)128 : (byte)0;
            _kbState[(int)Keys.MENU] = KeyboardState.Alt ? (byte)128 : (byte)0;
            //interestingly, GetAsyncKeyState() refuses to cooperate, so we're going
            //with GetKeyState(). This SHOULD rely on our message loop, but from the
            //limited testing, this doesn't appear to be the case?
            //If anything breaks related to Caps Lock, look here.
            _kbState[(int)Keys.CAPITAL] = (WinApi.GetKeyState(Keys.CAPITAL) & 0x1) == 1 ? (byte)1 : (byte)0;

            var result = new StringBuilder(_capacity);
            
            //ToUnicode used to break the keyboard cache for some reason, but this was
            //fixed in win update 1607. This means that on a system that doesn't have that update,
            //this will break.

            //Whether we have an easy way to detect that and work around it
            //is under consideration, but any workarounds I found so far are messy
            //and rarely 100%, so until then we are sticking with this.

            //This effectively sets a version req, but we can only reliably detect runtime versions,
            //not windows itself, so a TODO is to figure all this out.
            int ret = WinApi.ToUnicode((uint)EventKeyCode, ScanCode, _kbState, result, _capacity, 4u);


            if (ret <= 0) return;

            Characters = result.ToString();
        }
    }

    /// <summary>
    /// The type of the event that raised <see cref="KeyListenerApp.KeyEvent"/>.
    /// </summary>
    public enum KeyEventType {
        /// <summary>
        /// Indicates that the key event was caused by a key being pressed.
        /// </summary>
        KeyDown,
        /// <summary>
        /// Indicates that the key event was caused by a key being held down.
        /// </summary>
        KeyHold,
        /// <summary>
        /// Indicates that the key event was caused by a key being released.
        /// </summary>
        KeyUp
    }
}
