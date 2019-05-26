using Handles;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using TileManTest;
using WinApi.Gdi32;
using WinApi.User32;

namespace MouseCaptureTest
{
    internal class Win32dll
    {
        public const int STRING_BUFFER_LENGTH = 1024;
        static DebugLogger Logger = new DebugLogger( "Win32dll" );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr WindowFromPoint( int x , int y );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetWindowText( IntPtr hWnd , string lpString , int cch );

        public static string GetWindowText( IntPtr hWnd )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetWindowText( hWnd , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetClassName( IntPtr hWnd , string lpString , int cch );

        public static string GetClassName( IntPtr hWnd )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetClassName( hWnd , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        public enum GetAncestorFlags : uint
        {
            GA_PARENT = 1,
            GA_ROOT = 2,
            GA_ROOTOWNER = 3
        }
        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr GetAncestor( IntPtr hWnd , GetAncestorFlags gaFlags );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint GetWindowThreadProcessId( IntPtr hWnd , out uint ProcessId );

        public enum ProcessAccessFlags : uint
        {
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x00000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            SuspendResume = 0x00000800,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        [DllImport( "kernel32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr OpenProcess( ProcessAccessFlags dwDesiredAccess , bool bInheritHandle , uint dwProcessId );

        [DllImport( "psapi.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern bool EnumProcessModules( IntPtr hProcess , [MarshalAs( UnmanagedType.LPArray )] [In][Out] IntPtr[] lphModule , uint cb , out uint lpcbNeeded );

        [DllImport( "psapi.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        public static extern int GetModuleFileNameEx( IntPtr hProcess , IntPtr hModule , string lpBaseName , int nSize );

        public static string GetModuleFileNameEx( IntPtr hProcess , IntPtr hModule )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetModuleFileNameEx( hProcess , hModule , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "kernel32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern bool CloseHandle( IntPtr handle );

        [DllImport( "psapi.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetProcessImageFileName( IntPtr hProcess , string lpString , int cch );

        public static string GetProcessImageFileName( IntPtr hProcess )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetProcessImageFileName( hProcess , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "kernel32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        public static extern bool QueryFullProcessImageName( IntPtr hProcess , uint dwFlags , string lpExeName , ref int lpdwSize );

        public static string QueryFullProcessImageName( IntPtr hProcess , bool native )
        {
            uint dwFlags = ( native ? ( uint )0x00000001 : ( uint )0 );

            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = text.Length;
            if ( QueryFullProcessImageName( hProcess , dwFlags , text , ref len ) )
                return text.Substring( 0 , len );
            else
                return null;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
       static extern bool QueryServiceStatusEx(IntPtr serviceHandle, int infoLevel, IntPtr buffer, int bufferSize, out int bytesNeeded);


        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = -20;
        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint GetWindowLong( IntPtr hWnd , int index );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint SetWindowLong( IntPtr hWnd , int index , uint unValue );

        // https://codeutopia.net/blog/2007/12/18/find-an-applications-icon-with-winapi/
        public static Icon GetAppIcon( IntPtr hwnd )
        {
            IntPtr iconHandle = SendMessage( hwnd , WM_GETICON , ICON_SMALL2 , 0 );
            if ( iconHandle == IntPtr.Zero )
                iconHandle = SendMessage( hwnd , WM_GETICON , ICON_SMALL , 0 );
            if ( iconHandle == IntPtr.Zero )
                iconHandle = SendMessage( hwnd , WM_GETICON , ICON_BIG , 0 );
            // -14だめなことがある
            if ( iconHandle == IntPtr.Zero )
                iconHandle = GetClassLongPtr( hwnd , GCL_HICON );
            if ( iconHandle == IntPtr.Zero )
                iconHandle = GetClassLongPtr( hwnd , GCL_HICONSM );

            if ( iconHandle == IntPtr.Zero )
                return SystemIcons.Information;

            Icon icn = Icon.FromHandle( iconHandle );
            return icn;
        }
        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x7F;

        public static bool IsWin10
        {
            get
            {
                var ver = Environment.OSVersion.Version.Major;
                return ver > 5;
            }
        }

        public static IntPtr GetClassLongPtr( IntPtr hWnd , int nIndex )
        {
            if ( IntPtr.Size > 4 )
            {
                return GetClassLongPtr64( new HandleRef (hWnd , hWnd) , nIndex );
            }
            else
            {
                try
                {

                    uint value = GetClassLongPtr32( hWnd , nIndex );

                    if ( value < Int32.MaxValue )
                    {
                        return new IntPtr( value );
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error( ex.ToString( ) );
                    return IntPtr.Zero;
                }
            }
        }

        [DllImport( "user32.dll" , EntryPoint = "GetClassLong" )]
        public static extern uint GetClassLongPtr32( IntPtr hWnd , int nIndex );

        [DllImport( "user32.dll" , EntryPoint = "GetClassLongPtr" )]
        public static extern IntPtr GetClassLongPtr64( HandleRef hWnd , int nIndex );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , SetLastError = false )]
        static extern IntPtr SendMessage( IntPtr hWnd , int Msg , int wParam , int lParam );

    } // class Win32dll
} // namespace MouseCaptureTest