using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions.Windows
{
    public enum Keys
    {
        //
        // Сводка:
        //     The bitmask to extract modifiers from a key value.
        Modifiers = -65536,
        //
        // Сводка:
        //     No key pressed.
        None = 0,
        //
        // Сводка:
        //     The left mouse button.
        LButton = 1,
        //
        // Сводка:
        //     The right mouse button.
        RButton = 2,
        //
        // Сводка:
        //     The CANCEL key.
        Cancel = 3,
        //
        // Сводка:
        //     The middle mouse button (three-button mouse).
        MButton = 4,
        //
        // Сводка:
        //     The first x mouse button (five-button mouse).
        XButton1 = 5,
        //
        // Сводка:
        //     The second x mouse button (five-button mouse).
        XButton2 = 6,
        //
        // Сводка:
        //     The BACKSPACE key.
        Back = 8,
        //
        // Сводка:
        //     The TAB key.
        Tab = 9,
        //
        // Сводка:
        //     The LINEFEED key.
        LineFeed = 10,
        //
        // Сводка:
        //     The CLEAR key.
        Clear = 12,
        //
        // Сводка:
        //     The RETURN key.
        Return = 13,
        //
        // Сводка:
        //     The ENTER key.
        Enter = 13,
        //
        // Сводка:
        //     The SHIFT key.
        ShiftKey = 16,
        //
        // Сводка:
        //     The CTRL key.
        ControlKey = 17,
        //
        // Сводка:
        //     The ALT key.
        Menu = 18,
        //
        // Сводка:
        //     The PAUSE key.
        Pause = 19,
        //
        // Сводка:
        //     The CAPS LOCK key.
        Capital = 20,
        //
        // Сводка:
        //     The CAPS LOCK key.
        CapsLock = 20,
        //
        // Сводка:
        //     The IME Kana mode key.
        KanaMode = 21,
        //
        // Сводка:
        //     The IME Hanguel mode key. (maintained for compatibility; use HangulMode)
        HanguelMode = 21,
        //
        // Сводка:
        //     The IME Hangul mode key.
        HangulMode = 21,
        //
        // Сводка:
        //     The IME Junja mode key.
        JunjaMode = 23,
        //
        // Сводка:
        //     The IME final mode key.
        FinalMode = 24,
        //
        // Сводка:
        //     The IME Hanja mode key.
        HanjaMode = 25,
        //
        // Сводка:
        //     The IME Kanji mode key.
        KanjiMode = 25,
        //
        // Сводка:
        //     The ESC key.
        Escape = 27,
        //
        // Сводка:
        //     The IME convert key.
        IMEConvert = 28,
        //
        // Сводка:
        //     The IME nonconvert key.
        IMENonconvert = 29,
        //
        // Сводка:
        //     The IME accept key, replaces System.Windows.Forms.Keys.IMEAceept.
        IMEAccept = 30,
        //
        // Сводка:
        //     The IME accept key. Obsolete, use System.Windows.Forms.Keys.IMEAccept instead.
        IMEAceept = 30,
        //
        // Сводка:
        //     The IME mode change key.
        IMEModeChange = 31,
        //
        // Сводка:
        //     The SPACEBAR key.
        Space = 32,
        //
        // Сводка:
        //     The PAGE UP key.
        Prior = 33,
        //
        // Сводка:
        //     The PAGE UP key.
        PageUp = 33,
        //
        // Сводка:
        //     The PAGE DOWN key.
        Next = 34,
        //
        // Сводка:
        //     The PAGE DOWN key.
        PageDown = 34,
        //
        // Сводка:
        //     The END key.
        End = 35,
        //
        // Сводка:
        //     The HOME key.
        Home = 36,
        //
        // Сводка:
        //     The LEFT ARROW key.
        Left = 37,
        //
        // Сводка:
        //     The UP ARROW key.
        Up = 38,
        //
        // Сводка:
        //     The RIGHT ARROW key.
        Right = 39,
        //
        // Сводка:
        //     The DOWN ARROW key.
        Down = 40,
        //
        // Сводка:
        //     The SELECT key.
        Select = 41,
        //
        // Сводка:
        //     The PRINT key.
        Print = 42,
        //
        // Сводка:
        //     The EXECUTE key.
        Execute = 43,
        //
        // Сводка:
        //     The PRINT SCREEN key.
        Snapshot = 44,
        //
        // Сводка:
        //     The PRINT SCREEN key.
        PrintScreen = 44,
        //
        // Сводка:
        //     The INS key.
        Insert = 45,
        //
        // Сводка:
        //     The DEL key.
        Delete = 46,
        //
        // Сводка:
        //     The HELP key.
        Help = 47,
        //
        // Сводка:
        //     The 0 key.
        D0 = 48,
        //
        // Сводка:
        //     The 1 key.
        D1 = 49,
        //
        // Сводка:
        //     The 2 key.
        D2 = 50,
        //
        // Сводка:
        //     The 3 key.
        D3 = 51,
        //
        // Сводка:
        //     The 4 key.
        D4 = 52,
        //
        // Сводка:
        //     The 5 key.
        D5 = 53,
        //
        // Сводка:
        //     The 6 key.
        D6 = 54,
        //
        // Сводка:
        //     The 7 key.
        D7 = 55,
        //
        // Сводка:
        //     The 8 key.
        D8 = 56,
        //
        // Сводка:
        //     The 9 key.
        D9 = 57,
        //
        // Сводка:
        //     The A key.
        A = 65,
        //
        // Сводка:
        //     The B key.
        B = 66,
        //
        // Сводка:
        //     The C key.
        C = 67,
        //
        // Сводка:
        //     The D key.
        D = 68,
        //
        // Сводка:
        //     The E key.
        E = 69,
        //
        // Сводка:
        //     The F key.
        F = 70,
        //
        // Сводка:
        //     The G key.
        G = 71,
        //
        // Сводка:
        //     The H key.
        H = 72,
        //
        // Сводка:
        //     The I key.
        I = 73,
        //
        // Сводка:
        //     The J key.
        J = 74,
        //
        // Сводка:
        //     The K key.
        K = 75,
        //
        // Сводка:
        //     The L key.
        L = 76,
        //
        // Сводка:
        //     The M key.
        M = 77,
        //
        // Сводка:
        //     The N key.
        N = 78,
        //
        // Сводка:
        //     The O key.
        O = 79,
        //
        // Сводка:
        //     The P key.
        P = 80,
        //
        // Сводка:
        //     The Q key.
        Q = 81,
        //
        // Сводка:
        //     The R key.
        R = 82,
        //
        // Сводка:
        //     The S key.
        S = 83,
        //
        // Сводка:
        //     The T key.
        T = 84,
        //
        // Сводка:
        //     The U key.
        U = 85,
        //
        // Сводка:
        //     The V key.
        V = 86,
        //
        // Сводка:
        //     The W key.
        W = 87,
        //
        // Сводка:
        //     The X key.
        X = 88,
        //
        // Сводка:
        //     The Y key.
        Y = 89,
        //
        // Сводка:
        //     The Z key.
        Z = 90,
        //
        // Сводка:
        //     The left Windows logo key (Microsoft Natural Keyboard).
        LWin = 91,
        //
        // Сводка:
        //     The right Windows logo key (Microsoft Natural Keyboard).
        RWin = 92,
        //
        // Сводка:
        //     The application key (Microsoft Natural Keyboard).
        Apps = 93,
        //
        // Сводка:
        //     The computer sleep key.
        Sleep = 95,
        //
        // Сводка:
        //     The 0 key on the numeric keypad.
        NumPad0 = 96,
        //
        // Сводка:
        //     The 1 key on the numeric keypad.
        NumPad1 = 97,
        //
        // Сводка:
        //     The 2 key on the numeric keypad.
        NumPad2 = 98,
        //
        // Сводка:
        //     The 3 key on the numeric keypad.
        NumPad3 = 99,
        //
        // Сводка:
        //     The 4 key on the numeric keypad.
        NumPad4 = 100,
        //
        // Сводка:
        //     The 5 key on the numeric keypad.
        NumPad5 = 101,
        //
        // Сводка:
        //     The 6 key on the numeric keypad.
        NumPad6 = 102,
        //
        // Сводка:
        //     The 7 key on the numeric keypad.
        NumPad7 = 103,
        //
        // Сводка:
        //     The 8 key on the numeric keypad.
        NumPad8 = 104,
        //
        // Сводка:
        //     The 9 key on the numeric keypad.
        NumPad9 = 105,
        //
        // Сводка:
        //     The multiply key.
        Multiply = 106,
        //
        // Сводка:
        //     The add key.
        Add = 107,
        //
        // Сводка:
        //     The separator key.
        Separator = 108,
        //
        // Сводка:
        //     The subtract key.
        Subtract = 109,
        //
        // Сводка:
        //     The decimal key.
        Decimal = 110,
        //
        // Сводка:
        //     The divide key.
        Divide = 111,
        //
        // Сводка:
        //     The F1 key.
        F1 = 112,
        //
        // Сводка:
        //     The F2 key.
        F2 = 113,
        //
        // Сводка:
        //     The F3 key.
        F3 = 114,
        //
        // Сводка:
        //     The F4 key.
        F4 = 115,
        //
        // Сводка:
        //     The F5 key.
        F5 = 116,
        //
        // Сводка:
        //     The F6 key.
        F6 = 117,
        //
        // Сводка:
        //     The F7 key.
        F7 = 118,
        //
        // Сводка:
        //     The F8 key.
        F8 = 119,
        //
        // Сводка:
        //     The F9 key.
        F9 = 120,
        //
        // Сводка:
        //     The F10 key.
        F10 = 121,
        //
        // Сводка:
        //     The F11 key.
        F11 = 122,
        //
        // Сводка:
        //     The F12 key.
        F12 = 123,
        //
        // Сводка:
        //     The F13 key.
        F13 = 124,
        //
        // Сводка:
        //     The F14 key.
        F14 = 125,
        //
        // Сводка:
        //     The F15 key.
        F15 = 126,
        //
        // Сводка:
        //     The F16 key.
        F16 = 127,
        //
        // Сводка:
        //     The F17 key.
        F17 = 128,
        //
        // Сводка:
        //     The F18 key.
        F18 = 129,
        //
        // Сводка:
        //     The F19 key.
        F19 = 130,
        //
        // Сводка:
        //     The F20 key.
        F20 = 131,
        //
        // Сводка:
        //     The F21 key.
        F21 = 132,
        //
        // Сводка:
        //     The F22 key.
        F22 = 133,
        //
        // Сводка:
        //     The F23 key.
        F23 = 134,
        //
        // Сводка:
        //     The F24 key.
        F24 = 135,
        //
        // Сводка:
        //     The NUM LOCK key.
        NumLock = 144,
        //
        // Сводка:
        //     The SCROLL LOCK key.
        Scroll = 145,
        //
        // Сводка:
        //     The left SHIFT key.
        LShiftKey = 160,
        //
        // Сводка:
        //     The right SHIFT key.
        RShiftKey = 161,
        //
        // Сводка:
        //     The left CTRL key.
        LControlKey = 162,
        //
        // Сводка:
        //     The right CTRL key.
        RControlKey = 163,
        //
        // Сводка:
        //     The left ALT key.
        LMenu = 164,
        //
        // Сводка:
        //     The right ALT key.
        RMenu = 165,
        //
        // Сводка:
        //     The browser back key.
        BrowserBack = 166,
        //
        // Сводка:
        //     The browser forward key.
        BrowserForward = 167,
        //
        // Сводка:
        //     The browser refresh key.
        BrowserRefresh = 168,
        //
        // Сводка:
        //     The browser stop key.
        BrowserStop = 169,
        //
        // Сводка:
        //     The browser search key.
        BrowserSearch = 170,
        //
        // Сводка:
        //     The browser favorites key.
        BrowserFavorites = 171,
        //
        // Сводка:
        //     The browser home key.
        BrowserHome = 172,
        //
        // Сводка:
        //     The volume mute key.
        VolumeMute = 173,
        //
        // Сводка:
        //     The volume down key.
        VolumeDown = 174,
        //
        // Сводка:
        //     The volume up key.
        VolumeUp = 175,
        //
        // Сводка:
        //     The media next track key.
        MediaNextTrack = 176,
        //
        // Сводка:
        //     The media previous track key.
        MediaPreviousTrack = 177,
        //
        // Сводка:
        //     The media Stop key.
        MediaStop = 178,
        //
        // Сводка:
        //     The media play pause key.
        MediaPlayPause = 179,
        //
        // Сводка:
        //     The launch mail key.
        LaunchMail = 180,
        //
        // Сводка:
        //     The select media key.
        SelectMedia = 181,
        //
        // Сводка:
        //     The start application one key.
        LaunchApplication1 = 182,
        //
        // Сводка:
        //     The start application two key.
        LaunchApplication2 = 183,
        //
        // Сводка:
        //     The OEM Semicolon key on a US standard keyboard.
        OemSemicolon = 186,
        //
        // Сводка:
        //     The OEM 1 key.
        Oem1 = 186,
        //
        // Сводка:
        //     The OEM plus key on any country/region keyboard.
        Oemplus = 187,
        //
        // Сводка:
        //     The OEM comma key on any country/region keyboard.
        Oemcomma = 188,
        //
        // Сводка:
        //     The OEM minus key on any country/region keyboard.
        OemMinus = 189,
        //
        // Сводка:
        //     The OEM period key on any country/region keyboard.
        OemPeriod = 190,
        //
        // Сводка:
        //     The OEM question mark key on a US standard keyboard.
        OemQuestion = 191,
        //
        // Сводка:
        //     The OEM 2 key.
        Oem2 = 191,
        //
        // Сводка:
        //     The OEM tilde key on a US standard keyboard.
        Oemtilde = 192,
        //
        // Сводка:
        //     The OEM 3 key.
        Oem3 = 192,
        //
        // Сводка:
        //     The OEM open bracket key on a US standard keyboard.
        OemOpenBrackets = 219,
        //
        // Сводка:
        //     The OEM 4 key.
        Oem4 = 219,
        //
        // Сводка:
        //     The OEM pipe key on a US standard keyboard.
        OemPipe = 220,
        //
        // Сводка:
        //     The OEM 5 key.
        Oem5 = 220,
        //
        // Сводка:
        //     The OEM close bracket key on a US standard keyboard.
        OemCloseBrackets = 221,
        //
        // Сводка:
        //     The OEM 6 key.
        Oem6 = 221,
        //
        // Сводка:
        //     The OEM singled/double quote key on a US standard keyboard.
        OemQuotes = 222,
        //
        // Сводка:
        //     The OEM 7 key.
        Oem7 = 222,
        //
        // Сводка:
        //     The OEM 8 key.
        Oem8 = 223,
        //
        // Сводка:
        //     The OEM angle bracket or backslash key on the RT 102 key keyboard.
        OemBackslash = 226,
        //
        // Сводка:
        //     The OEM 102 key.
        Oem102 = 226,
        //
        // Сводка:
        //     The PROCESS KEY key.
        ProcessKey = 229,
        //
        // Сводка:
        //     Used to pass Unicode characters as if they were keystrokes. The Packet key value
        //     is the low word of a 32-bit virtual-key value used for non-keyboard input methods.
        Packet = 231,
        //
        // Сводка:
        //     The ATTN key.
        Attn = 246,
        //
        // Сводка:
        //     The CRSEL key.
        Crsel = 247,
        //
        // Сводка:
        //     The EXSEL key.
        Exsel = 248,
        //
        // Сводка:
        //     The ERASE EOF key.
        EraseEof = 249,
        //
        // Сводка:
        //     The PLAY key.
        Play = 250,
        //
        // Сводка:
        //     The ZOOM key.
        Zoom = 251,
        //
        // Сводка:
        //     A constant reserved for future use.
        NoName = 252,
        //
        // Сводка:
        //     The PA1 key.
        Pa1 = 253,
        //
        // Сводка:
        //     The CLEAR key.
        OemClear = 254,
        //
        // Сводка:
        //     The bitmask to extract a key code from a key value.
        KeyCode = 65535,
        //
        // Сводка:
        //     The SHIFT modifier key.
        Shift = 65536,
        //
        // Сводка:
        //     The CTRL modifier key.
        Control = 131072,
        //
        // Сводка:
        //     The ALT modifier key.
        Alt = 262144
    }
}
