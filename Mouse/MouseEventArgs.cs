using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyCap.LowLevel.Win32;

namespace KeyCap.Mouse {

    /// <summary>
    /// Arguments for low level mouse events.
    /// </summary>
    public class MouseEventArgs {

        /// <summary>
        /// The location of the mouse pointer.
        /// </summary>
        public Point Location { get; }

        /// <summary>
        /// The type of change that caused this event.
        /// </summary>
        public MouseEventType EventType { get; }

        /// <summary>
        /// What key, if any, changed state, causing this event.
        /// </summary>
        public MouseKey MouseKey { get; }

        /// <summary>
        /// If the cause of the event was a scroll, this specifies the delta value of this event.
        /// </summary>
        public int Delta { get; }
        
        /// <summary>
        /// The ID of the message received with this event. Can be used to add custom actions,
        /// for events not covered by this API.
        /// </summary>
        public int Message { get; }

        /// <summary>
        /// If set to true, this event will be cancelled, preventing it from reaching the target window.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new instance with the specified properties.
        /// </summary>
        public MouseEventArgs(Point loc, MouseEventType meType, MouseKey mKey, int message, int delta) {
            Location = loc;
            EventType = meType;
            MouseKey = mKey;
            Message = message;
            Delta = delta;
        }
    }

    /// <summary>
    /// Describes the type of mouse event, or more exactly what caused it.
    /// </summary>
    public enum MouseEventType {
        /// <summary>
        /// Indicates that this event was caused by an unknown action. See <see cref="MouseEventArgs.Message"/> for
        /// further details.
        /// </summary>
        /// <remarks>
        /// This is a fallback that shouldn't really happen, but with unconventional mice
        /// it is likely possible. This is the default value to be inline with <see cref="MouseKey.None"/>
        /// </remarks>
        Special,
        /// <summary>
        /// Indicates that this event was caused by the mouse pointer being moved.
        /// </summary>
        MouseMove,
        /// <summary>
        /// Indicates that one of the mouse keys was pressed, causing this event.
        /// </summary>
        MouseKeyDown,
        /// <summary>
        /// Indicates that one of the mouse key was released, causing this event.
        /// </summary>
        MouseKeyUp,
        /// <summary>
        /// Indicates that a vertical scroll caused this event.
        /// </summary>
        MouseScroll,
        /// <summary>
        /// Indicates that a horizontal scroll caused this event.
        /// </summary>
        MouseScrollHorizontal
    }

    /// <summary>
    /// Describes which button, if any, caused the action that raised this event.
    /// </summary>
    public enum MouseKey {
        /// <summary>
        /// Indicates that this event had no associated key, or that the key is unknown. Eg. simply moving the mouse.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the left mouse button changed state, causing this event.
        /// </summary>
        LeftButton,
        /// <summary>
        /// Indicates that the right mouse button changed state, causing this event.
        /// </summary>
        RightButton,
        /// <summary>
        /// Indicates that the middle mouse button changed state, causing this event.
        /// </summary>
        MiddleButton,
        /// <summary>
        /// Indicates that the 4th, "Back" mouse button changes state, causing this event.
        /// </summary>
        Mouse4,
        /// <summary>
        /// Indicates that the 5th, "Forward" mouse button changes state, causing this event.
        /// </summary>
        Mouse5
    }
}
