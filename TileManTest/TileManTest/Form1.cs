﻿using DWMDotnet;
using Handles;
using MouseCaptureTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinApi.Gdi32;
using WinApi.User32;
using static MouseCaptureTest.Win32dll;
using static Types;

namespace TileManTest
{
    public partial class Form1 : Form
    {
        Timer t;
        uint ShellHookID;
        RECT ScreenGeom;
        RECT WindowGeom;
        int BarHeight;
        int BarY;
        bool ShowBar = true;
        bool TopBar =true;
        int Blw = 33;

        float MasterFact = 0.55f;
        int SelectedTag;
        int SelectedClient;

        List<string> TagList = new List<string>( )
        {
            "1"
        };
        MultiDictionary<string , Client> TagClientDic = new MultiDictionary<string , Client>( );

        List<Client> SelectedClientList()
        {
            return TagClientDic[ TagList[ SelectedTag ] ];
        }

        void ChangeMaster( Client client )
        {
            var nextMaster = SelectedClientList( ).Find( c => c == client );
            List<Client> clientList = TagClientDic[ TagList[ SelectedTag ] ];
            clientList.Remove( client );
            clientList.Reverse( );
            clientList.Add( client );
            clientList.Reverse( );
        }
        bool Initialized;
        public Form1()
        {
            MessageBox.Show( "hoge" );
            try
            {
                InitializeComponent( );
                t = new Timer( );
                t.Interval = 10;
                t.Tick += T_Tick;
                t.Start( );
                SetUp( );
                UpdateGeom( );
                ThreadWindowHandles.RegisterShellHookWindow( Handle );
                ShellHookID = ThreadWindowHandles.RegisterWindowMessage( "SHELLHOOK" );
                Tile( );
                Initialized = true;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.ToString() );
                foreach ( var wnd in DWM.allClient( ) )
                {
                    User32Methods.MoveWindow( wnd.Hwnd , 0 , 0 , 640 , 480 , true );
                }
            }
        }

        void DrawBar()
        {
            RECT barRect = new RECT( 0 , 0 , 0 , BarHeight );
            for ( int i = 0 ; i < TagList.Count ; i++ )
            {
                var item = TagList[ i ];
                int w = DWM.textnw( Handle , item );
                DrawText( item , SelectedTag == i , ScreenGeom );
                DrawSquare( SelectedTag == i , false , false , barRect );
                barRect.Right += w;
            }
        }

        void DrawSquare( bool filled , bool empty , bool invert , RECT rect )
        {
            int size = 5;
            NetCoreEx.Geometry.Rectangle extended;// = rect;
            extended.Left = rect.Left + 1;
            extended.Top = rect.Top + 1;
            extended.Right = rect.Right + size;
            extended.Bottom = rect.Bottom + size;

            var ForeColor = SystemColor.COLOR_SCROLLBAR;
            var BackColor = SystemColor.COLOR_WINDOW;

            using ( var brushColor = new PenBrush( false , invert ? BackColor : ForeColor ) )
            {
                var brush = brushColor.BrushPtr;
                ThreadWindowHandles.SelectObject( Handle , brush );
                if ( filled )
                {
                    User32Methods.FillRect( Handle , ref extended , brush );
                }
                else if ( empty )
                {
                    User32Methods.FillRect( Handle , ref extended , brush );
                }
                ThreadWindowHandles.DeleteObject( brush );
            }
        }

        int borderPx = 2;
        int Transparent = 1;

        void DrawText( string text , bool invert , RECT rect)
        {
            NetCoreEx.Geometry.Rectangle extended;
            extended.Left = rect.Left;
            extended.Top = rect.Top;
            extended.Right = rect.Right + rect.Width;
            extended.Bottom = rect.Bottom + rect.Height;

            var ForeColor = SystemColor.COLOR_SCROLLBAR;
            var BackColor = SystemColor.COLOR_WINDOW;

            var brush = invert ? BackColor : ForeColor;
            using ( var brushColor = new PenBrush( true , invert ? BackColor : ForeColor ) )
            {
                using ( var pen = new PenBrush( false , brush ) )
                {
                    ThreadWindowHandles.SelectObject( Handle , pen.BrushPtr );
                    ThreadWindowHandles.SelectObject( Handle , brushColor.BrushPtr );
                    User32Methods.FillRect( Handle , ref extended , brushColor.BrushPtr );
                }
            }

            Gdi32Methods.SetBkMode( Handle , Transparent );
            ThreadWindowHandles.SetTextColor( Handle , PenBrush.Color2Int( brush ) );
            var font = Gdi32Helpers.GetStockObject( StockObject.SYSTEM_FONT );
            ThreadWindowHandles.SelectObject( Handle , font );
            User32Helpers.DrawText( Handle , text , -1 , ref extended ,
                DrawTextFormatFlags.DT_CENTER | DrawTextFormatFlags.DT_VCENTER | DrawTextFormatFlags.DT_SINGLELINE );
        }

        void ApplyRules( Types.Client client )
        {
            var tag = TagList[ SelectedTag ];
            TagClientDic.Add( tag , client );
        }

        void SetSelected( Client client )
        {
            // drawborder何もしてない
            SelectedClient = SelectedClientList( ).IndexOf( client );
        }

        protected override void WndProc( ref System.Windows.Forms.Message m )
        {
            //base.WndProc( ref m );
            //return;
            var code = (WM)m.Msg;
            var wParam = m.WParam;
            if ( code == WM.CREATE )
            {

            }
            else if ( code == WM.CLOSE )
            {
            }
            else if ( code == WM.DESTROY )
            {

            }
            else if ( code == WM.HOTKEY )
            {

            }
            else
            {
                if ( m.Msg == ShellHookID )
                {

                    var param = ( WM )( wParam );
                    var client = GetClient( m.LParam );
                    switch ( param )
                    {
                        case WM.HSHELL_WINDOWCREATED:
                            if ( IsManageable( m.LParam ) )
                            {
                                var newClient = Manage( m.LParam );
                                // とりあえず子ウィンドウはスルー
                            }
                            Trace.WriteLine( "created :" + ThreadWindowHandles.GetWindowText( m.LParam ) );

                            break;
                        case WM.HSHELL_WINDOWDESTROYED:
                            Trace.WriteLine( "destroyed : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                            break;
                        case WM.HSHELL_WINDOWACTIVATED:
                        case WM.HSHELL_RUDEAPPACTIVATED:
                            Trace.WriteLine( "active : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                            break;
                        default:
                            break;
                    }

                }

            }

                //if(code != 0 && wParam != IntPtr.Zero)
                //Trace.WriteLine( $"code : {code} wParam : {wParam}" );
        }

        void UpdateGeom()
        {
            var hwnd = ThreadWindowHandles.FindWindow( "Shell_TrayWnd" , null );
            RECT rect = new RECT();
            if ( ThreadWindowHandles.IsWindowVisible( hwnd ) > 0 )
            {
                ThreadWindowHandles.SystemParametersInfoGet( ( uint )SystemParametersDesktopInfo.SPI_GETWORKAREA , 0 ,
                    ref rect , ( uint )SystemParamtersInfoFlags.None );
                ScreenGeom = rect;
            }
            else
            {
                int sx = User32Methods.GetSystemMetrics( SystemMetrics.SM_XVIRTUALSCREEN );
                int sy = User32Methods.GetSystemMetrics( SystemMetrics.SM_YVIRTUALSCREEN );
                int sw = User32Methods.GetSystemMetrics( SystemMetrics.SM_CXVIRTUALSCREEN );
                int sh = User32Methods.GetSystemMetrics( SystemMetrics.SM_CYVIRTUALSCREEN );
                ScreenGeom = new RECT( sx , sy , sw , sh );
            }
            // 固定値
            BarHeight = 20;
            int wy = ScreenGeom.Top;
            int wh = ScreenGeom.Height;
            BarY = -BarHeight;
            if ( ShowBar )
            {
                if ( TopBar )
                {
                    wy = ScreenGeom.Top + BarHeight;
                    BarY = wy - BarHeight;
                }
                else
                {
                    BarY = wy + wh;
                }
                wh = ScreenGeom.Height - BarHeight;
            }
            WindowGeom = new RECT( ScreenGeom );
        }

        private void T_Tick( object sender , EventArgs e )
        {
            var wnd = Win32dll.WindowFromPoint( MousePosition.X , MousePosition.Y );
            uint procID = 0;
            label2.Text = wnd.ToString();

            Win32dll.GetWindowThreadProcessId( wnd ,out procID);
            wnd = Win32dll.OpenProcess( Win32dll.ProcessAccessFlags.QueryInformation | Win32dll.ProcessAccessFlags.VMRead | Win32dll.ProcessAccessFlags.Terminate , false , procID );
            label3.Text = wnd.ToString();
            var size = User32Methods.GetWindowTextLength( wnd );
            if ( size > 0 )
            {
                var len = size + 1;
                var sb = new StringBuilder( len );
                //return User32Methods.GetWindowText( this.Handle , sb , len ) > 0 ? sb.ToString( ) : string.Empty;
                //NativeWibndowコンストラクトで
                //var n = new .( );
                //n.Attach()

                //User32Methods.WindowFromPoint(User32Methods.mou)
                label1.Text = Win32dll.QueryFullProcessImageName( wnd , false );
            }
        }

        void Update()
        {
            //User32Methods.FindWindow( className , null );
        }

        Client GetClient( IntPtr hwnd )
        {
            return DWM.allClient( ).First( c => c.Hwnd == hwnd );
        }

        Client Manage( IntPtr hwnd )
        {
            //StringBuilder value = ThreadWindowHandles.GetWindowText( hwnd );
            //if ( value != null )
            //{
            //    //Trace.WriteLine( value );
            //}

            if ( DWM.containClient( hwnd ) )
            {
                return GetClient( hwnd );
            }
            WindowInfo windowInfo = new WindowInfo();
            User32Methods.GetWindowInfo( hwnd , ref windowInfo );
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            Client client = new Client( 
                hwnd , ThreadWindowHandles.GetParent(hwnd) , IntPtr.Zero ,
                (int)User32Methods.GetWindowThreadProcessId(hwnd , IntPtr.Zero ) ,
                new System.Drawing.Rectangle() , 0 , true , title );

            WindowPlacement windowPlacement = new WindowPlacement();
            if ( ThreadWindowHandles.IsWindowVisible( client.Hwnd ) > 0 )
            {
                User32Methods.SetWindowPlacement( hwnd , ref windowPlacement );
            }
            // isfloat
            ApplyRules( client );

            if ( ThreadWindowHandles.IsWindowVisible( hwnd ) > 0 )
            {
                NetCoreEx.Geometry.Rectangle windowRect = windowInfo.WindowRect;
                DWM.resize( client , windowRect.Left , windowRect.Top , windowRect.Width , windowRect.Height , WindowGeom.Rect );
            }

            DWM.attach( client );
            return client;
        }

        bool Scan( IntPtr hwnd , IntPtr lParam )
        {
            if ( IsManageable( hwnd ) )
            {
                Manage( hwnd );
            }
            return true;
        }

        void SetBorder( Client client , bool border )
        {
            if ( border )
            {
                var style = ( WindowStyles )User32Helpers.GetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE );
                var titleAndScaleable = WindowStyles.WS_CAPTION | WindowStyles.WS_SIZEBOX;
                User32Helpers.SetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE , ( IntPtr )( style | titleAndScaleable ) );
            }
            else
            {
                //User32Helpers.SetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE , ( IntPtr )( style | titleAndScaleable ) );

            }
        }

        void SetUp()
        {
            User32Methods.EnumWindows( Scan , IntPtr.Zero );
        }

        void SetupBar()
        {
            //var font = Gdi32Helpers.GetStockObject( StockObject.SYSTEM_FONT );
            //ThreadWindowHandles.SelectObject( Handle , font );
            //blw直書きなので

        }

        bool IsManageable( IntPtr hwnd )
        {
            if ( DWM.containClient(hwnd) )
            {
                return true;
            }
            String value = ThreadWindowHandles.GetWindowText( hwnd );
            var parent = ThreadWindowHandles.GetParent( hwnd );
            var owner  = User32Helpers.GetWindow( hwnd , GetWindowFlag.GW_OWNER );
            var style   = (WindowStyles)User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_STYLE ).ToInt32();
            //tosafeがほしいけど別ライブラリ内
            var exStyle = (WindowExStyles)(User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_EXSTYLE ).ToInt32());
            bool isParentOK = parent != IntPtr.Zero && IsManageable( parent );
            bool isTool = ( exStyle & WindowExStyles.WS_EX_TOOLWINDOW ) != 0;
            bool isApp  = ( exStyle & WindowExStyles.WS_EX_APPWINDOW  ) != 0;
            if ( isParentOK && !DWM.containClient( parent ) )
            {
                // なんか見えないウィンドウをmanageするので
                //Manage( hwnd );
            }
	/* XXX: should we do this? */
            //if ( GetWindowTextLength( hwnd ) == 0 )
            //{
            //    debug( "   title: NULL\n" );
            //    debug( "  manage: false\n" );
            //    return false;
            //}
            if ( (style & WindowStyles.WS_DISABLED) != 0 )
            {
                return false;
            }
            if ( ( style & WindowStyles.WS_POPUP ) != 0 )
            {
                return false;
            }
            if ( ( style & WindowStyles.WS_POPUPWINDOW ) != 0 )
            {
                //return false;
            }
            bool hasParent = parent != IntPtr.Zero;
            var winIsVisibleAndNoParent = !hasParent && User32Methods.IsWindowVisible( hwnd );
            if ( winIsVisibleAndNoParent || isParentOK )
            {
                if ( ( !isTool && !hasParent ) || ( isTool && isParentOK ) )
                {
                    if ( value != null )
                    {

                        Trace.Write( value );
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style);
                    }
                    return true;
                }
                if ( isApp && hasParent )
                {
                    if ( value != null )
                    {

                        Trace.Write( value );
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style);
                    }
                    return true;
                }
            }
            return false;
        }

        void ResizeClient( Client c , int x , int y , int w , int h )
        {
            DWM.resize( c , x , y , w , h , ScreenGeom.Rect );
        }

        void Tile()
        {
            List<Client> clientList = SelectedClientList( );
            var master = clientList.First( );
            RECT winGeom = WindowGeom;
            var masterWidth = MasterFact * winGeom.Width;

            bool onlyOne = clientList.Count == 1;
            float v = ( onlyOne ? winGeom.Width : masterWidth ) - ( 2 * master.Bw );
            ResizeClient( master , winGeom.Left , winGeom.Top ,
                (int)v , winGeom.Height - 2 * master.Bw );
            if ( onlyOne )
            {
                return;
            }
            bool winIsLargerThanMaster = winGeom.Right + masterWidth > master.Rect.X + master.Rect.Width;
            var x = winIsLargerThanMaster ? master.Rect.X + master.Rect.Width + master.Bw * 2 : winGeom.Right + masterWidth;
            int y = winGeom.Top;
            var w = winIsLargerThanMaster ? winGeom.Right + winGeom.Width - x : winGeom.Width - masterWidth;
            var h = WindowGeom.Height / clientList.Count;
            if ( h < BarHeight )
            {
                h = winGeom.Height;
            }
            var slaveList = clientList.Skip( 1 ).ToList( );
            int slaveCount = slaveList.Count;
            for ( int i = 0 ; i < slaveCount ; i++ )
            {

                var item = slaveList[ i ];
                bool isLastOne = ( i + 1 == slaveCount );
                var height = isLastOne ? winGeom.Top + winGeom.Height - y -2 * item.Bw : h - 2 * item.Bw;
                ResizeClient( item , (int)x , y , (int)w - 2 * item.Bw , height );
                if ( isLastOne )
                {
                    y = item.Rect.Top + item.Rect.Height;
                }
            }
        }

        void Unmanage( Client client )
        {
            DWM.setVisibility( client.Hwnd , true );
            DWM.detach( client );

        }

        void UpdateBar()
        {
            // SetWindowPos(barhwnd, showbar ? HWND_TOPMOST : HWND_NOTOPMOST, 0, by, ww, bh, (showbar ? SWP_SHOWWINDOW : SWP_HIDEWINDOW) | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
            //ThreadWindowHandles.SetWindowPos(Handle, HWND_TOPMOST , 0, 0, 1024, 20, SWP_SHOWWINDOW  | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
            User32Helpers.SetWindowPos( Handle , HwndZOrder.HWND_TOPMOST 
                , 0 , BarY , WindowGeom.Width , BarHeight ,
                WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOSENDCHANGING );
        }

    }
}