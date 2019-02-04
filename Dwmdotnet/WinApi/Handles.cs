using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using static Handles.NativeMethods.ThreadWindowHandles;

namespace Handles
{

    [SuppressUnmanagedCodeSecurity]
    public static class NativeMethods
    {
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
            static extern IntPtr FindWindow(
                  string lpszClass ,
                  string lpszWindow
                  );
            [DllImport( "User32.dll" )]
            static extern int SetForegroundWindow(
                  IntPtr hWnd
                  );
            [DllImport( "User32.Dll" )]
            static extern int GetParent(
                  IntPtr hWnd
                  );
            [DllImport( "User32.Dll" )]
            static extern int IsWindowVisible(
                  IntPtr hWnd
                  );
            [DllImport( "user32.dll" )]
            static extern IntPtr GetWindowDC( IntPtr hwnd );
            [DllImport( "user32.dll" )]
            static extern short GetKeyState( int nVirtKey );
            [DllImport( "user32.dll" )]
            extern static int UnregisterHotKey( IntPtr hWnd , int id );
            [DllImport( "user32.dll" , SetLastError = true )]
            static extern bool RegisterShellHookWindow( IntPtr hWnd );
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
            static extern IntPtr CreatePen( PenStyle fnPenStyle , int nWidth , uint crColor );
            [DllImport( "user32.dll" , CharSet = CharSet.Auto )]
            public static extern int DrawText( IntPtr hDC , string lpString , int nCount , [In, Out] ref RECT lpRect , DT uFormat );
#if true
            [Flags]
            public enum SWP
            {
                SWP_ASYNCWINDOWPOS = 0x4000,
                SWP_DEFERERASE = 0x2000,
                SWP_DRAWFRAME = 0x0020,
                SWP_FRAMECHANGED = 0x0020,
                SWP_HIDEWINDOW = 0x0080,
                SWP_NOACTIVATE = 0x0010,
                SWP_NOCOPYBITS = 0x0100,
                SWP_NOMOVE = 0x0002,
                SWP_NOOWNERZORDER = 0x0200,
                SWP_NOREDRAW = 0x0008,
                SWP_NOREPOSITION = 0x0200,
                SWP_NOSENDCHANGING = 0x0400,
                SWP_NOSIZE = 0x0001,
                SWP_NOZORDER = 0x0004,
                SWP_SHOWWINDOW = 0x0040,
            }

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
                public System.Drawing.Rectangle rcNormalPosition;
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
            enum SW
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
            static extern int ShowWindow(
                IntPtr hWnd ,
                int nCmdShow
                );

            uint threadId;

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

            public static StringBuilder GetWindowText( IntPtr hWnd )
            {
                //ウィンドウのタイトルの長さを取得する
                int textLen = GetWindowTextLength( hWnd );
                if ( 0 < textLen )
                {
                    //ウィンドウのタイトルを取得する
                    StringBuilder tsb = new StringBuilder( textLen + 1 );
                    GetWindowText( hWnd , tsb , tsb.Capacity );

                    return tsb;
                }
                return null;
            }
            public static StringBuilder GetClassText( IntPtr hWnd )
            {
                //ウィンドウのクラス名を取得する
                StringBuilder csb = new StringBuilder( 256 );
                GetClassName( hWnd , csb , csb.Capacity );
                return csb;
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

    public WindowHandles( )
    {
      handles = new List<IntPtr>( );
    }

    public IEnumerator<IntPtr> GetEnumerator( )
    {
      return handles.GetEnumerator( );
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
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
    public TopLevelWindowHandles( )
        : base( )
    {
      handles = new List<IntPtr>( );
      NativeMethods.EnumWindows( EnumWindowProc , default( IntPtr ) );
    }
  }
	  [StructLayout(LayoutKind.Sequential, Pack = 4)]
      public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
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
    public IntPtr WindowHandle { get { return windowHandle; } }
  }

  /// <summary>
  /// スレッドに所属するトップレベルウィンドウのウィンドウハンドルを列挙する機能を提供します。
  /// </summary>

    private enum PenStyle : int
{
    PS_SOLID    = 0, //The pen is solid.
    PS_DASH     = 1, //The pen is dashed.
    PS_DOT      = 2, //The pen is dotted.
    PS_DASHDOT      = 3, //The pen has alternating dashes and dots.
    PS_DASHDOTDOT       = 4, //The pen has alternating dashes and double dots.
    PS_NULL     = 5, //The pen is invisible.
    PS_INSIDEFRAME      = 6,// Normally when the edge is drawn, it’s centred on the outer edge meaning that half the width of the pen is drawn
        // outside the shape’s edge, half is inside the shape’s edge. When PS_INSIDEFRAME is specified the edge is drawn
        //completely inside the outer edge of the shape.
    PS_USERSTYLE    = 7,
    PS_ALTERNATE    = 8,
    PS_STYLE_MASK       = 0x0000000F,

    PS_ENDCAP_ROUND     = 0x00000000,
    PS_ENDCAP_SQUARE    = 0x00000100,
    PS_ENDCAP_FLAT      = 0x00000200,
    PS_ENDCAP_MASK      = 0x00000F00,

    PS_JOIN_ROUND       = 0x00000000,
    PS_JOIN_BEVEL       = 0x00001000,
    PS_JOIN_MITER       = 0x00002000,
    PS_JOIN_MASK    = 0x0000F000,

    PS_COSMETIC     = 0x00000000,
    PS_GEOMETRIC    = 0x00010000,
    PS_TYPE_MASK    = 0x000F0000
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

  }
}
