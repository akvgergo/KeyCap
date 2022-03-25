using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyCap.Keyboard {

    /// <summary>
    /// Interface for all classes firing events based on keyboard input.
    /// </summary>
    public interface IKeyEventSource {

        /// <summary>
        /// Whether the current instance is firing events.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Raised for a keyboard event.
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyEvent;

        /// <summary>
        /// Pauses the firing of key events.
        /// </summary>
        void Pause();

        /// <summary>
        /// Starts the firing of key events
        /// </summary>
        void Start();
    }
}
