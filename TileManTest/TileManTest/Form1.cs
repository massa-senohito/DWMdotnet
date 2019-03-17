﻿using DWMDotnet;
using Handles;
using MouseCaptureTest;
using NetCoreEx.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinApi.Gdi32;
using WinApi.User32;
using static MouseCaptureTest.Win32dll;
using static Types;
using TagType = System.String;
namespace TileManTest
{
    public partial class Form1 : Form
    {
        Timer t;
        uint ShellHookID;
        private List<HotKey> HotkeyList = new List<HotKey>( );
        RECT ScreenGeom;
        RECT WindowGeom;
        int BarHeight;
        int BarY;
        bool ShowBar = true;
        bool TopBar = true;
        bool IsDirty = false;
        int Blw = 33;
        int UIHeight = 118;

        float MasterFact = 0.55f;
        string SelectedTag = "1";

        Dictionary<TagType , TagManager> TagClientDic = new Dictionary<TagType, TagManager>( );

        List<Client> SelectedClientList()
        {
            return TagClientDic[  SelectedTag ].ClientList;
        }

        List<ListBox> ClientTitleList;

        void ChangeMaster( Client client )
        {
            List<Client> list = SelectedClientList( );
            var nextMaster = list.Find( c => c == client );
            List<Client> clientList = list;
            clientList.Remove( client );
            clientList.Reverse( );
            clientList.Add( client );
            clientList.Reverse( );
        }

        public Form1()
        {
            try
            {
                InitializeComponent( );

                ClientTitleList = new List<ListBox>( )
                {
                    listBox1,
                    listBox2,
                    listBox3,
                    listBox4,
                    listBox5,
                    listBox6,
                    listBox7,
                    listBox8,
                    listBox9,
                };

                for ( int i = 1 ; i < 10 ; i++ )
                {
                    Keys d0 = ( Keys )( ( int )Keys.D0 + i );
                    const Keys changeTag = Keys.Control ;
                    const Keys sendTag   = Keys.Control | Keys.Alt;
                    var hotkeyNotify = new HotKey( this.Handle, i   , d0 , changeTag);
                    var hotkeySend   = new HotKey( this.Handle, i + 10, d0 , sendTag);
                    HotkeyList.Add( hotkeyNotify );
                    HotkeyList.Add( hotkeySend );
                    var temp = new TagManager( new List<Client>() , i );
                    TagClientDic.Add( i.ToString() , temp );
                    ClientTitleList[ i - 1 ].Click += Form1_SelectedIndexChanged;
                }
                SetUp( );
                ActiveClient = TryGetMaster( );
                UpdateGeom( );
                ThreadWindowHandles.RegisterShellHookWindow( Handle );
                ShellHookID = ThreadWindowHandles.RegisterWindowMessage( "SHELLHOOK" );
                FormClosing += Form1_FormClosing;
                Tile( );
                Bounds = WindowGeom.Rect;
                Height = UIHeight;
                t = new Timer( );
                t.Interval = 10;
                t.Tick += T_Tick;
                t.Start( );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.ToString( ) );
                CleanUp( );
            }
        }

        private void Form1_SelectedIndexChanged( object sender , EventArgs e )
        {
            var name = sender as ListBox;
            var newTag = name.Name.Last( ).ToString();
            ChangeTag( TagClientDic[ SelectedTag.ToString( ) ] , newTag );
            SelectedTag = newTag;
        }

        private void CleanUp()
        {
            foreach ( var tagMan in TagClientDic.Values )
            {
                foreach ( var item in tagMan.ClientList )
                {
                    DWM.setClientVisibility( item , true );
                    User32Methods.MoveWindow( item.Hwnd , 0 , 0 , WindowGeom.Width , 480 , true );
                }
            }
            foreach ( var item in HotkeyList )
            {
                item.Unregister( );
            }
        }

        private void Form1_FormClosing( object sender , FormClosingEventArgs e )
        {
            CleanUp( );

        }

        void DrawSquare( bool filled , bool empty , bool invert , RECT rect )
        {
            int size = 5;
            Rectangle extended;// = rect;
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
            Rectangle extended;
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
        }

        void SetSelected( Client client )
        {
            // drawborder何もしてない
        }

        protected override void WndProc( ref System.Windows.Forms.Message m )
        {
            // DrawBar
            base.WndProc( ref m );
            //return;
            var code = ( WM )m.Msg;
            var wParam = m.WParam;
            #region barhandler
            switch ( code )
            {
                case WM.CREATE:

                    UpdateBar( );

                    break;
                case WM.PAINT:

                    //PaintStruct paintStruct;
                    //User32Methods.BeginPaint( m.HWnd , out paintStruct );
                    //DrawBar( );
                    //User32Methods.EndPaint( m.HWnd , ref paintStruct );

                    break;
                case WM.LBUTTONDOWN:
                    break;

                default:
                    break;
            }
            #endregion
            if ( code == WM.CLOSE )
            {
            }
            else if ( code == WM.DESTROY )
            {
            }
            else if ( code == WM.HOTKEY )
            {
                foreach ( var item in HotkeyList )
                {
                    if ( m.LParam == item.LParam )
                    {
                        Trace.WriteLine( $"hotkey{item.ID}" );
                        TagSignal( item );
                    }
                }
            }
            else
            {
                if ( m.Msg == ShellHookID )
                {

                    var param = ( WM )( wParam );
                    var client = GetClient( m.LParam );
                    OtherWindow( m , param , client );

                }

            }

                //if(code != 0 && wParam != IntPtr.Zero)
                //Trace.WriteLine( $"code : {code} wParam : {wParam}" );
        }

        Client ActiveClient;

        private void TagSignal( HotKey item )
        {
            bool send = 9 < item.ID;
            TagManager currentTag = TagClientDic[ SelectedTag.ToString() ];
            string itemID = item.ID.ToString( );
            int sentDest = item.ID - 10;
            // 同じタグなら再度入らないように
            if ( sentDest.ToString() == SelectedTag )
            {
                return;
            }
            if ( send )
            {
                if ( ActiveClient != null )
                {
                    IsDirty = true;
                    DWM.setClientVisibility( ActiveClient , false );
                    currentTag.Remove( ActiveClient );
                    Attach( ActiveClient , sentDest.ToString( ) );
                    Tile( );
                    ActiveClient = TryGetMaster( );
                }
            }
            else
            {
                ChangeTag( currentTag , itemID );
            }
        }

        private void ChangeTag( TagManager currentTag , string itemID )
        {
            if ( itemID == SelectedTag )
            {
                return;
            }
            ActiveClient = null;
            currentTag.Visible( false );
            TagClientDic[ itemID ].Visible( true );
            SelectedTag = itemID;
            Tile( );
        }

        private void OtherWindow( System.Windows.Forms.Message m , WM param , Client client )
        {
            switch ( param )
            {
                case WM.HSHELL_WINDOWCREATED:
                    if ( IsManageable( m.LParam ) )
                    {
                        var newClient = Manage( m.LParam );
                        // とりあえず子ウィンドウはスルー
                        Tile( );
                    }
                    Trace.WriteLine( "created :" + ThreadWindowHandles.GetWindowText( m.LParam ) );

                    break;
                case WM.HSHELL_WINDOWDESTROYED:
                    if ( client != null )
                    {

                        Trace.WriteLine( "destroyed : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                        // タグ移動で隠された時にも来るので、その場合無視
                        if ( !client.Ignore )
                        {

                            Unmanage( client );
                            Tile( );
                        }
                        else
                        {
                            client.Ignore = false;
                        }
                    }
                    break;
                case WM.HSHELL_WINDOWACTIVATED:
                case WM.HSHELL_RUDEAPPACTIVATED:
                    if ( client != null )
                    {
                        ActiveClient = client;
                        Trace.WriteLine( "active : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                    }
                    break;

                default:
                    break;
            }

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
            //label1.Text = SelectedClientList( ).Aggregate( "" , ( acc , c ) => acc + "\n" + c.Title );
            Win32dll.GetWindowThreadProcessId( wnd ,out procID);
            //wnd = Win32dll.OpenProcess( Win32dll.ProcessAccessFlags.QueryInformation | Win32dll.ProcessAccessFlags.VMRead | Win32dll.ProcessAccessFlags.Terminate , false , procID );
            //label3.Text = wnd.ToString();
            label3.Text = SelectedTag;
            //var size = User32Methods.GetWindowTextLength( wnd );
            //if ( size > 0 )
            //{
            //    var len = size + 1;
            //    var sb = new StringBuilder( len );

            //    label1.Text = Win32dll.QueryFullProcessImageName( wnd , false );
            //}
            if ( IsDirty )
            {
                foreach ( var tagMan in TagClientDic.Values )
                {
                    ClientTitleList[ tagMan.Id - 1 ].Items.Clear( );
                    ClientTitleList[ tagMan.Id - 1 ].Items.AddRange( tagMan.ClientTitles.ToArray( ) );
                }
                IsDirty = false;
            }
            //label3.Text = nameList.ToString( );
            Client client = TryGetMaster( );
            if ( client == null )
            {
                return;
            }
            if ( client.HasUpdate( ) )
            {
                foreach ( var item in SlaveList() )
                {
                    int x = client.Rect.Width;
                    item.SetXW( x , ScreenGeom.Width - x );
                }
                return;
            }
            //foreach ( var item in SlaveList() )
            //{
            //    if ( item.HasUpdate( ) )
            //    {

            //    }
            //}
        }

        Client GetClient( IntPtr hwnd )
        {
            foreach ( var item in TagClientDic.Values )
            {
                var foundItem = item.GetClient( hwnd );
                if ( foundItem != null )
                {
                    return foundItem;
                }
            }
            return null;
        }

        Client Manage( IntPtr hwnd )
        {
            //StringBuilder value = ThreadWindowHandles.GetWindowText( hwnd );
            //if ( value != null )
            //{
            //    //Trace.WriteLine( value );
            //}

            if ( ContainClient( hwnd ) )
            {
                return GetClient( hwnd );
            }
            WindowInfo windowInfo = new WindowInfo();
            User32Methods.GetWindowInfo( hwnd , ref windowInfo );
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            Client client = Types.createClient(
                hwnd , ThreadWindowHandles.GetParent( hwnd ) ,
                ( int )User32Methods.GetWindowThreadProcessId( hwnd , IntPtr.Zero ) ,
                 WindowStyle( hwnd ) , title );

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

            Attach( client , SelectedTag);
            return client;
        }

        private void Attach( Client client , string dest )
        {
            TagClientDic[ dest ].Add( client );
            IsDirty = true;
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
            IsDirty = true;
        }

        void SetupBar()
        {
            //var font = Gdi32Helpers.GetStockObject( StockObject.SYSTEM_FONT );
            //ThreadWindowHandles.SelectObject( Handle , font );
            //blw直書きなので

        }

        bool IsManageable( IntPtr hwnd )
        {
            if ( ContainClient( hwnd ) )
            {
                return false;
            }
            if ( hwnd == Handle )
            {
                return false;
            }
            String value = ThreadWindowHandles.GetWindowText( hwnd );
            String classText = ThreadWindowHandles.GetClassText( hwnd );
            if ( //value.Contains( "Visual" ) || 
                value.Contains("Everything"))
            {
                return false;
            }
            if ( value.Contains( "Orchis" ) )
            {
                Trace.WriteLine( "" );
            }
            //Trace.WriteLine( value + " : className = " + classText );
            //if ( value.Contains( "Chrome" ) || classText.Contains( "CabinetWClass" ) )
            //{
            //    DWM.setVisibility( hwnd , true );
            //    return true;
            //}
            var parent = ThreadWindowHandles.GetParent( hwnd );
            var owner = User32Helpers.GetWindow( hwnd , GetWindowFlag.GW_OWNER );
            WindowStyles style = WindowStyle( hwnd );
            //tosafeがほしいけど別ライブラリ内
            var exStyle = ( WindowExStyles )( User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_EXSTYLE ).ToInt32( ) );
            bool isParentOK = parent != IntPtr.Zero && IsManageable( parent );
            bool isTool = ( exStyle & WindowExStyles.WS_EX_TOOLWINDOW ) != 0;
            bool isApp = ( exStyle & WindowExStyles.WS_EX_APPWINDOW ) != 0;
            if ( isParentOK && !ContainClient( parent ) )
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
            if ( ( style & WindowStyles.WS_DISABLED ) != 0 )
            {
                return false;
            }
            // WS_POPUPWINDOW も含んでいる
            if ( ( style & WindowStyles.WS_POPUP ) != 0 )
            {
                return false;
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
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style );
                    }
                    return true;
                }
                if ( isApp && hasParent )
                {
                    if ( value != null )
                    {

                        Trace.Write( value );
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style );
                    }
                    return true;
                }
            }
            return false;
        }

        private static WindowStyles WindowStyle( IntPtr hwnd )
        {
            return ( WindowStyles )User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_STYLE ).ToInt32( );
        }

        private bool ContainClient( IntPtr hwnd )
        {
            foreach ( var item in TagClientDic.Values )
            {
                bool contains = item.HasClient( hwnd );
                if ( contains )
                {
                    return true;
                }
            }
            return false;
        }

        void ResizeClient( Client c , int x , int y , int w , int h )
        {
            Trace.WriteLine( $"{c.Title} rect : {c.Rect}" );
            Rectangle rect = new Rectangle( x , y , x + w , y + h );
            DWM.resize( c , rect.Left , rect.Top , rect.Width , rect.Height , ScreenGeom.Rect );
            Trace.WriteLine( $"after {c.Title} rect : {c.Rect}" );
        }

        void ShowHide( Client client )
        {
            // 見えるべきでないものを見えなくし、逆を見せる
        }

        void Tile()
        {
            List<Client> clientList = SelectedClientList( );
            if ( clientList.Count == 0 )
            {
                return;
            }
            var master = clientList.First( );
            RECT winGeom = WindowGeom;
            var masterWidth = MasterFact * winGeom.Width;

            bool onlyOne = clientList.Count == 1;
            float v = ( onlyOne ? winGeom.Width : masterWidth ) - ( 2 * master.Bw );
            ResizeClient( master , winGeom.Left , winGeom.Top + UIHeight ,
                ( int )v , winGeom.Height - 2 * master.Bw );
            if ( onlyOne )
            {
                return;
            }
            bool winIsLargerThanMaster = winGeom.Left + masterWidth > master.Rect.X + master.Rect.Width;
            var x = winIsLargerThanMaster ? master.Rect.X + master.Rect.Width + master.Bw * 2 : winGeom.Left + masterWidth;
            int y = winGeom.Top + UIHeight;
            var w = winIsLargerThanMaster ? winGeom.Left + winGeom.Width - x : winGeom.Width - masterWidth;
            var h = (WindowGeom.Height - UIHeight) / clientList.Count;
            if ( h < BarHeight )
            {
                h = winGeom.Height;
            }
            var slaveList = SlaveList( );
            int slaveCount = slaveList.Count;
            for ( int i = 0 ; i < slaveCount ; i++ )
            {

                var item = slaveList[ i ];
                bool isLastOne = ( i + 1 == slaveCount );
                var height = isLastOne ? winGeom.Top + winGeom.Height - y - 2 * item.Bw : h - 2 * item.Bw;
                User32Methods.GetWindowRect( item.Hwnd , out Rectangle winRect );
                User32Methods.GetClientRect( item.Hwnd , out Rectangle cliRect );
                ResizeClient( item , ( int )x , y , ( int )w - 2 * item.Bw , height );
                if ( !isLastOne )
                {
                    y = item.Rect.Top + item.Rect.Height;
                    var title = winRect.Height - cliRect.Height;
                    y += title;
                }
            }
        }

        private Client TryGetMaster( )
        {
            return SelectedClientList( ).FirstOrDefault( );
        }

        private List<Client> SlaveList( )
        {
            return SelectedClientList( ).Skip( 1 ).ToList( );
        }

        void Unmanage( Client client )
        {
            DWM.setClientVisibility( client , true );
            Detach( client );
        }

        private void Detach( Client client )
        {
            foreach ( var item in TagClientDic.Values )
            {
                var contains = item.Remove(client);
                if ( contains  )
                {
                    IsDirty = true;
                    return ;
                }
            }
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
