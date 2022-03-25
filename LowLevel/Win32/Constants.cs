using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyCap.LowLevel.Win32 {

    [Flags]
    internal enum KBDLLHOOKSTRUCTFlags : uint {
        LLKHF_EXTENDED = 0x01,
        LLKHF_INJECTED = 0x10,
        LLKHF_ALTDOWN = 0x20,
        LLKHF_UP = 0x80,
    }

    internal enum HookType : int {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_SYSMSGFILTER = 6,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    /// <summary>
    /// Windows messages possible to receive through a <see cref="HookType.WH_MOUSE_LL"/> hook.
    /// </summary>
    /// <remarks>
    /// Note that the double click messages are missing. It's up to
    /// the target window whether they want to receive those messages,
    /// and since the hook is called before any window receives them, it seems
    /// impossible to receive them through the hook.
    /// </remarks>
    internal enum Win32MouseEvent {
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MOUSEWHEEL = 0x020A,
        WM_XBUTTONDOWN = 0x020B,
        WM_XBUTTONUP = 0x020C,
        WM_MOUSEHWHEEL = 0x020E,
    }

    /// <summary>
    /// Windows messages possible to receive through a <see cref="HookType.WH_KEYBOARD_LL"/> hook.
    /// </summary>
    internal enum Win32KeyEvent : int {
        WM_KEYDOWN = 0X100,
        WM_KEYUP = 0X101,
        WM_SYSKEYDOWN = 0x104,
        WM_SYSKEYUP = 0x105
    }

    internal enum MapVkMapType : uint {
        /// <summary>
        /// VKeyCode to ScanCode, without left or right distinction
        /// </summary>
        MAPVK_VK_TO_VSC = 0x00,
        /// <summary>
        /// ScanCode to VKeyCode, without left or right distinction
        /// </summary>
        MAPVK_VSC_TO_VK = 0x01,
        /// <summary>
        /// VKeyCode to Char
        /// </summary>
        MAPVK_VK_TO_CHAR = 0x02,
        /// <summary>
        /// ScanCode to VKeyCode, with left or right distinction
        /// </summary>
        MAPVK_VSC_TO_VK_EX = 0x03,
        /// <summary>
        /// VKeyCode to ScanCode, with left or right distinction
        /// </summary>
        MAPVK_VK_TO_VSC_EX = 0x04
    }
}
