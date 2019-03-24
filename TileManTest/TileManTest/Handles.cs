// こちらは消す
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using static Handles.ThreadWindowHandles;

namespace Handles
{

    /// <summary>
    /// スレッドに所属するトップレベルウィンドウのウィンドウハンドルを列挙する機能を提供します。
    /// </summary>
    [Flags]
    public enum PenStyle : int
    {
        PS_SOLID = 0, //The pen is solid.
        PS_DASH = 1, //The pen is dashed.
        PS_DOT = 2, //The pen is dotted.
        PS_DASHDOT = 3, //The pen has alternating dashes and dots.
        PS_DASHDOTDOT = 4, //The pen has alternating dashes and double dots.
        PS_NULL = 5, //The pen is invisible.
        PS_INSIDEFRAME = 6,// Normally when the edge is drawn, it’s centred on the outer edge meaning that half the width of the pen is drawn
                           // outside the shape’s edge, half is inside the shape’s edge. When PS_INSIDEFRAME is specified the edge is drawn
                           //completely inside the outer edge of the shape.
        PS_USERSTYLE = 7,
        PS_ALTERNATE = 8,
        PS_STYLE_MASK = 0x0000000F,

        PS_ENDCAP_ROUND = 0x00000000,
        PS_ENDCAP_SQUARE = 0x00000100,
        PS_ENDCAP_FLAT = 0x00000200,
        PS_ENDCAP_MASK = 0x00000F00,

        PS_JOIN_ROUND = 0x00000000,
        PS_JOIN_BEVEL = 0x00001000,
        PS_JOIN_MITER = 0x00002000,
        PS_JOIN_MASK = 0x0000F000,

        PS_COSMETIC = 0x00000000,
        PS_GEOMETRIC = 0x00010000,
        PS_TYPE_MASK = 0x000F0000
    };
    [Flags]
    public enum DT : uint
    {
        DT_TOP = 0x00000000,
        DT_LEFT = 0x00000000,
        DT_CENTER = 0x00000001,
        DT_RIGHT = 0x00000002,
        DT_VCENTER = 0x00000004,
        DT_BOTTOM = 0x00000008,
        DT_WORDBREAK = 0x00000010,
        DT_SINGLELINE = 0x00000020,
        DT_EXPANDTABS = 0x00000040,
        DT_TABSTOP = 0x00000080,
        DT_NOCLIP = 0x00000100,
        DT_EXTERNALLEADING = 0x00000200,
        DT_CALCRECT = 0x00000400,
        DT_NOPREFIX = 0x00000800,
        DT_INTERNAL = 0x00001000,
        DT_EDITCONTROL = 0x00002000,
        DT_PATH_ELLIPSIS = 0x00004000,
        DT_END_ELLIPSIS = 0x00008000,
        DT_MODIFYSTRING = 0x00010000,
        DT_RTLREADING = 0x00020000,
        DT_WORD_ELLIPSIS = 0x00040000,
        DT_NOFULLWIDTHCHARBREAK = 0x00080000,
        DT_HIDEPREFIX = 0x00100000,
        DT_PREFIXONLY = 0x00200000,
    }

    [Flags]
    public enum SWP
    {
        ASYNCWINDOWPOS = 0x4000,
        DEFERERASE = 0x2000,
        DRAWFRAME = 0x0020,
        FRAMECHANGED = 0x0020,
        HIDEWINDOW = 0x0080,
        NOACTIVATE = 0x0010,
        NOCOPYBITS = 0x0100,
        NOMOVE = 0x0002,
        NOOWNERZORDER = 0x0200,
        NOREDRAW = 0x0008,
        NOREPOSITION = 0x0200,
        NOSENDCHANGING = 0x0400,
        NOSIZE = 0x0001,
        NOZORDER = 0x0004,
        SHOWWINDOW = 0x0040,
    }
    [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
    public class ThreadWindowHandles : WindowHandles
    {
        public enum BrushStyles
        {
            BS_SOLID = 0,
            BS_NULL = 1,
            BS_HATCHED = 2,
            BS_PATTERN = 3,
        }

        [UnmanagedFunctionPointer( CallingConvention.StdCall )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public delegate bool EnumWindowsProcDelegate( IntPtr windowHandle , IntPtr lParam );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool EnumThreadWindows(
            uint threadId ,
            [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
            IntPtr lParam );

        [DllImport( "user32.dll" , SetLastError = true )]
        public static extern int GetWindowThreadProcessId(
            IntPtr hWnd , out int lpdwProcessId );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , SetLastError = true )]
        public static extern int GetWindowText( IntPtr hWnd ,
          StringBuilder lpString , int nMaxCount );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , SetLastError = true )]
        public static extern int GetWindowTextLength( IntPtr hWnd );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , SetLastError = true )]
        public static extern int GetClassName( IntPtr hWnd ,
          StringBuilder lpClassName , int nMaxCount );

        [DllImport( "user32.dll" , SetLastError = true )]
        static extern IntPtr GetWindow( IntPtr hWnd , int uCmd );

        [DllImport( "User32.Dll" )]
        static extern IntPtr GetDesktopWindow();

        [DllImport( "User32.dll" , CharSet = CharSet.Unicode )]
        public static extern IntPtr FindWindow(
              string lpszClass ,
              string lpszWindow
              );

        [DllImport( "user32.dll" , EntryPoint = "SystemParametersInfo" ,
        SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SystemParametersInfoGet(
            uint action , uint param , ref RECT vparam , uint init );


        [DllImport( "User32.dll" )]
        public static extern int SetForegroundWindow(
              IntPtr hWnd
              );

        [DllImport( "user32.dll" , ExactSpelling = true , CharSet = CharSet.Auto )]
        public static extern IntPtr GetParent( IntPtr hWnd );

        public static IntPtr GetParentSafe( IntPtr handle )
        {
            IntPtr result = GetParent( handle );
            if ( result == IntPtr.Zero )
            {
                // An error occured
                throw new System.ComponentModel.Win32Exception( Marshal.GetLastWin32Error( ) );
            }
            return result;
        }

        [DllImport( "User32.Dll" )]
        public static extern int IsWindowVisible(
              IntPtr hWnd
              );

        [DllImport( "user32.dll" )]
        static extern IntPtr GetWindowDC( IntPtr hwnd );

        [DllImport( "user32.dll" )]
        static extern short GetKeyState( int nVirtKey );

        [DllImport( "user32.dll" )]
        extern static int UnregisterHotKey( IntPtr hWnd , int id );

        [DllImport( "user32.dll" , SetLastError = true , CharSet = CharSet.Auto )]
        public static extern uint RegisterWindowMessage( string lpString );
        [DllImport( "user32.dll" , SetLastError = true )]
        public static extern bool RegisterShellHookWindow( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        static extern bool SetSysColors( int cElements , int[] lpaElements ,
           uint[] lpaRgbValues );

        [DllImport( "user32.dll" )]
        static extern IntPtr ReleaseDC( IntPtr hwnd , IntPtr hdc );

        [DllImportAttribute( "gdi32.dll" , EntryPoint = "CreateSolidBrush" )]
        public static extern IntPtr CreateSolidBrush( BrushStyles enBrushStyle , int crColor );

        [DllImport( "gdi32.dll" , EntryPoint = "SelectObject" )]
        public static extern IntPtr SelectObject( IntPtr hdc , IntPtr bmp );

        [DllImport( "user32.dll" )]
        static extern int FillRect( IntPtr hDC , [In] ref RECT lprc , IntPtr hbr );

        [DllImport( "gdi32.dll" , EntryPoint = "DeleteObject" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool DeleteObject( [In] IntPtr hObject );

        [DllImport( "gdi32.dll" )]
        public static extern IntPtr CreatePen( PenStyle fnPenStyle , int nWidth , uint crColor );

        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(IntPtr hdc, int crColor);

        [DllImport( "user32.dll" , CharSet = CharSet.Auto )]
        public static extern int DrawText( IntPtr hDC , string lpString , int nCount , [In, Out] ref RECT lpRect , DT uFormat );

#if true

        [DllImport( "USER32.DLL" , CharSet = CharSet.Auto )]
        public static extern bool SetWindowPos(
            IntPtr hWnd , // ウィンドウのハンドル
            IntPtr hWndInsertAfter , // 配置順序のハンドル
            int X , // 横方向の位置
            int Y , // 縦方向の位置
            int cx , // 幅
            int cy , // 高さ
            SWP uFlags // ウィンドウ位置のオプション
        );
        /// <summary>
///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
///     to the upper-left corner of the parent window's client area.
///     <para>
///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
///         information
///     </para>
/// </summary>
/// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
/// <param name="X">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
/// <param name="Y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
/// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
/// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
/// <param name="bRepaint">
///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
///     parent window uncovered as a result of moving a child window.
/// </param>
/// <returns>
///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
///     <br />To get extended error information, call GetLastError.
/// </returns>
[DllImport("user32.dll", SetLastError = true)]
public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        static extern bool GetWindowPlacement( IntPtr hWnd , ref WINDOWPLACEMENT lpwndpl );

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public RECT rcNormalPosition;
        }

        //        BOOL GetWindowRect(
        //            HWND hWnd,      // ウィンドウのハンドル
        //            LPRECT lpRect   // ウィンドウの座標値
        //            );
        [DllImport( "User32.Dll" )]
        static extern int GetWindowRect(
        IntPtr hWnd ,      // ウィンドウのハンドル
        out RECT rect   // ウィンドウの座標値
        );
        //        BOOL MoveWindow(
        //            HWND hWnd,      // ウィンドウのハンドル
        //            int X,          // 横方向の位置
        //            int Y,          // 縦方向の位置
        //            int nWidth,     // 幅
        //            int nHeight,    // 高さ
        //            BOOL bRepaint   // 再描画オプション
        //            );
        [DllImport( "User32.dll" )]
        static extern int MoveWindow(
              IntPtr hWnd ,
              int x ,
              int y ,
              int nWidth ,
              int nHeight ,
              int bRepaint
              );
        [DllImport( "User32.Dll" )]
        static extern int IsChild(
              IntPtr hWnd
              );
        //        BOOL ShowWindow(
        //            HWND hWnd,     // ウィンドウのハンドル
        //            int nCmdShow   // 表示状態
        //            );
        [Flags]
        public enum SW
        {

            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11,
        }
        [DllImport( "User32.Dll" )]
        public static extern int ShowWindow(
            IntPtr hWnd ,
            SW nCmdShow
            );

        uint threadId;

        [DllImport( "gdi32.dll" )]
        static extern bool GetTextExtentPoint32( IntPtr hdc , string lpString ,
           int cbString , out Size lpSize );

        public static int GetTextExtentWidth( IntPtr hdc , string lpString )
        {
            Size size = new Size( );
            int cbString = lpString.Length;
            GetTextExtentPoint32( hdc , lpString , cbString , out size );
            return size.Width;
        }
        /// <summary>
        /// スレッドに所属するトップレベルウィンドウのウィンドウハンドルを取得します。
        /// </summary>
        /// <param name="threadId">スレッドIDです。</param>
        public ThreadWindowHandles( uint threadId )
            : base( )
        {
            this.threadId = threadId;
            EnumThreadWindows( threadId , EnumWindowProc , default( IntPtr ) );
        }

        public static int GetWindowThreadProcessId( IntPtr hWnd )
        {
            int ret = 0;
            return GetWindowThreadProcessId( hWnd , out ret );
        }

        public static String GetWindowText( IntPtr hWnd )
        {
            //ウィンドウのタイトルの長さを取得する
            int textLen = GetWindowTextLength( hWnd );
            if ( 0 < textLen )
            {
                //ウィンドウのタイトルを取得する
                StringBuilder tsb = new StringBuilder( textLen + 1 );
                GetWindowText( hWnd , tsb , tsb.Capacity );

                return tsb.ToString();
            }
            return "";
        }
        public static String GetClassText( IntPtr hWnd )
        {
            //ウィンドウのクラス名を取得する
            StringBuilder csb = new StringBuilder( 256 );
            GetClassName( hWnd , csb , csb.Capacity );
            return csb.ToString();
        }
        /// <summary>
        /// スレッドIDです。
        /// </summary>
        public uint ThreadID
        {
            get
            {
                return threadId;
            }
        }

#endif
    }
    /// <summary>
    /// ウィンドウハンドルを列挙するクラスの共通機能をまとめたクラスです。
    /// </summary>
    public abstract class WindowHandles : IEnumerable<IntPtr>
    {


        public List<IntPtr> handles;

        public WindowHandles()
        {
            handles = new List<IntPtr>( );
        }

        public IEnumerator<IntPtr> GetEnumerator()
        {
            return handles.GetEnumerator( );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return handles.GetEnumerator( );
        }

        internal bool EnumWindowProc( IntPtr handle , IntPtr lParam )
        {
            handles.Add( handle );
            return true;
        }
    }

    /// <summary>
    /// トップレベルウィンドウのウィンドウハンドルを列挙する機能を提供します。
    /// </summary>
    [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
    public sealed class TopLevelWindowHandles : WindowHandles
    {
        [SuppressUnmanagedCodeSecurity]
        public static class NativeMethods
        {
            [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
            [return: MarshalAs( UnmanagedType.Bool )]
            public static extern bool EnumWindows(
                [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
                IntPtr lParam );
        }

        /// <summary>
        /// トップレベルウィンドウのウィンドウハンドルを列挙します。
        /// </summary>
        public TopLevelWindowHandles()
            : base( )
        {
            handles = new List<IntPtr>( );
            NativeMethods.EnumWindows( EnumWindowProc , default( IntPtr ) );
        }
    }
    [StructLayout( LayoutKind.Sequential , Pack = 4 )]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width
        {
            get
            {
                return Right - Left;
            }
            set
            {
                Right = Left + value;
            }
        }

        public int Height
        {
            get
            {
                return Bottom - Top;
            }
            set
            {
                Bottom = Top + value;
            }
        }

        public RECT( int left , int top , int width , int height )
        {
            Left = left;
            Top  = top;
            Right = left + width;
            Bottom = top + height;
        }

        public RECT( RECT rect )
        {
            Left = rect.Left;
            Top  = rect.Top;
            Right = rect.Right;
            Bottom = rect.Bottom;
        }

        public Rectangle Rect
        {
            get
            {
                return new Rectangle( Left , Top , Width , Height );
            }
        }
    }
    /// <summary>
    /// 親ウィンドウの子ウィンドウのウィンドウハンドルを列挙する機能を提供します。
    /// </summary>
    [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
    public sealed class ChildWindowHandles : WindowHandles
    {
        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
            [return: MarshalAs( UnmanagedType.Bool )]
            public static extern bool EnumChildWindows(
                IntPtr handle ,
                [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
                IntPtr lParam );
        }

        IntPtr windowHandle;

        /// <summary>
        /// 親ウィンドウの子ウィンドウのウィンドウハンドルを取得します。
        /// </summary>
        /// <param name="windowHandle">親ウィンドウのウィンドウハンドルです。</param>
        public ChildWindowHandles( IntPtr windowHandle )
            : base( )
        {
            this.windowHandle = windowHandle;
            NativeMethods.EnumChildWindows( windowHandle , EnumWindowProc , default( IntPtr ) );
        }

        /// <summary>
        /// 親ウィンドウのウィンドウハンドルです。
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                return windowHandle;
            }
        }
    }



}

