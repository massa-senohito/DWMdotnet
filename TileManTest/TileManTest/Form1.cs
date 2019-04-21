//#define USESCREENWORLD
//#define RECOVER
//#define MultiScreen
using DWMDotnet;
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
using static Types;

namespace TileManTest
{

    public partial class Form1 : Form
    {
        Timer UpdateTimer;
        uint ShellHookID;
        private List<HotKey> HotkeyList = new List<HotKey>( );
        RECT ScreenGeom;
        int BarHeight;
        int BarY;
        bool TopBar = true;
        int UIHeight = 118;
        int ToggleHotID = 100;
        int SortMaster = 101;

        List<ListBox> ClientTitleList;

        const string SettingPath = "TileSetting.txt";
        TileSetting _TileSetting ;
        DebugLogger Logger;
        TileWindowManager WindowManager;

        public Form1()
        {
            Logger = new DebugLogger( "Form1" );
            _TileSetting = TileSetting.Load( SettingPath );

            WindowManager = new TileWindowManager( UIHeight );

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
                    var hotkeyNotify = new HotKey( this.Handle, i      , d0 , changeTag);
                    var hotkeySend   = new HotKey( this.Handle, i + 10 , d0 , sendTag);
                    HotkeyList.Add( hotkeyNotify );
                    HotkeyList.Add( hotkeySend );

                    foreach ( var screen in WindowManager.ScreenList )
                    {
                        screen.AddTag( i );
                    }
                    ClientTitleList[ i - 1 ].Click += Form1_SelectedIndexChanged;
                }
                HotkeyList.Add( new HotKey( Handle , ToggleHotID , Keys.H , Keys.Control ) );
                HotkeyList.Add( new HotKey( Handle , SortMaster  , Keys.M , Keys.Control ) );

                SetUp( );
                WindowManager.ActiveClient = WindowManager.TryGetMaster( );
                UpdateGeom( );
                ThreadWindowHandles.RegisterShellHookWindow( Handle );
                ShellHookID = ThreadWindowHandles.RegisterWindowMessage( "SHELLHOOK" );
                FormClosing += Form1_FormClosing;
                Bounds = ScreenGeom.Rect;
                Height = UIHeight;

                UpdateTimer = new Timer( );
                UpdateTimer.Interval = 10;
                UpdateTimer.Tick += T_Tick;
                UpdateTimer.Start( );
                Paint += Form1_Paint;
                // 狭いスクリーンだとquitがなくなることがある
                var widestScr = Screen.AllScreens.OrderBy( s => s.WorkingArea.Width ).Last( );
                Location = widestScr.Bounds.Location;
                WindowManager.BelongScreen = Screen.FromHandle( Handle );
                WindowManager.Tile( );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.ToString( ) );
                CleanUp( );
            }
        }

        private void Form1_Paint( object sender , PaintEventArgs e )
        {
            foreach ( var item in WindowManager.ScreenList )
            {
                item.PaintIcon( ClientTitleList , e );
            }
        }

        private void Form1_SelectedIndexChanged( object sender , EventArgs e )
        {
            var listBox = sender as ListBox;
            var newTag  = listBox.Name.Last( ).ToString();
            //ChangeTag( TagClientDic[ SelectedTag.ToString( ) ] , newTag );
            WindowManager.ChangeTag( WindowManager.CurrentTag , newTag );
            WindowManager.SelectedTag = newTag;
            int selectedInd = listBox.SelectedIndex;
            //label3.Text = listBox.Name + " " + selectedInd.ToString();
            if ( selectedInd != ListBox.NoMatches )
            {
                User32Methods.SetForegroundWindow(
                WindowManager.SelectedClientList( )[ selectedInd ].Hwnd );
            }
        }

        private void CleanUp()
        {
            foreach ( var screen in WindowManager.ScreenList )
            {
                screen.CleanUp( );
            }
            foreach ( var hotkey in HotkeyList )
            {
                hotkey.Unregister( );
            }
        }

        private void Form1_FormClosing( object sender , FormClosingEventArgs e )
        {
            CleanUp( );

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
                OnHotKey( m );
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

        private void OnHotKey( System.Windows.Forms.Message m )
        {
            foreach ( var item in HotkeyList )
            {
                if ( m.LParam == item.LParam )
                {
                    Trace.WriteLine( $"hotkey {item.ID}" );
                    if ( item.ID == ToggleHotID )
                    {
                        Visible = !Visible;
                        return;
                    }
                    if ( item.ID == SortMaster )
                    {
                        WindowManager.SortMaster( );
                        return;
                    }
                    WindowManager.TagSignal( item );
                }
            }
        }

        private void OtherWindow( System.Windows.Forms.Message m , WM param , Client client )
        {
            //Trace.WriteLine( client?.Title + " " + param );
            switch ( param )
            {
                case WM.HSHELL_WINDOWCREATED:
                    TileMode tileMode = IsManageable( m.LParam );
                    if ( tileMode == TileMode.Tile )
                    {
                        var newClient = Manage( m.LParam , tileMode );
                        // とりあえず子ウィンドウはスルー
                        WindowManager.Tile( );
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
                            WindowManager.Tile( );
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
                        WindowManager.ActiveClient = client;
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
            // ここ全体 TileWindowManagerに持っていきたい
            WindowManager.ScreenGeom = ScreenGeom;
        }

        private void UpdateClient()
        {
            var bui = new StringBuilder( );
            //foreach ( var tagMan in TagClientDic.Values )
            foreach ( var tagMan in WindowManager.ScreenList.SelectMany(s=>s.TagList) )
            {
                foreach ( var client in tagMan.ClientList )
                {
                    bui.Append( $"{client.Title} y = {client.Rect.Y} h = {client.Rect.Height}\n" );
                    if ( client.HasTitleUpdate( ) )
                    {
                        WindowManager.IsDirty = true;
                        return;
                    }
                }
            }
        }

        void UpdateTitle( TagManager tagMan )
        {
            ClientTitleList[ tagMan.Id - 1 ].Items.Clear( );
            string[] items = tagMan.ClientTitles.ToArray( );
            ClientTitleList[ tagMan.Id - 1 ].Items.AddRange( items );
        }

        private void T_Tick( object sender , EventArgs e )
        {
            DebugLogger.Update( );
            var wnd = Win32dll.WindowFromPoint( MousePosition.X , MousePosition.Y );
            uint procID = 0;
            label2.Text = wnd.ToString( );
            //label1.Text = SelectedClientList( ).Aggregate( "" , ( acc , c ) => acc + "\n" + c.Title );
#if USEWINAPI
            //IntPtr procPtr = IntPtr.Zero;
            //uint procID2 = User32Methods.GetWindowThreadProcessId( wnd , procPtr );
#endif
            Win32dll.GetWindowThreadProcessId( wnd , out procID );
            //wnd = Win32dll.OpenProcess( Win32dll.ProcessAccessFlags.QueryInformation | Win32dll.ProcessAccessFlags.VMRead | Win32dll.ProcessAccessFlags.Terminate , false , procID );
            //label3.Text = SelectedTag;
            //var size = User32Methods.GetWindowTextLength( wnd );
            //if ( size > 0 )
            //{
            //    var len = size + 1;
            //    var sb = new StringBuilder( len );

            //    label1.Text = Win32dll.QueryFullProcessImageName( wnd , false );
            //}
            UpdateClient( );

            if ( WindowManager.IsDirty )
            {
#if MultiScreen
                foreach ( var screen in ScreenList )
                {
                    foreach ( var tagMan in screen.TagList )
#else
                    foreach ( var tagMan in WindowManager.ScreenList[0].TagList )
#endif
                    {
                        UpdateTitle( tagMan );
                    }
#if MultiScreen
                }
#endif
                Invalidate( );
                WindowManager.IsDirty = false;
            }

            CalcSlaveSizeFromMaster( );

            UpdateSlaveSize( );
#if true
            var movedClientList = WindowManager.ScreenList.SelectMany(screen=> screen.MovedClients( WindowManager.SelectedTag ));
            UpdateClientScreen( movedClientList );

            foreach ( var screen in WindowManager.ScreenList )
            {
                screen.UpdateScreen( WindowManager.SelectedTag );
            }
#endif
        }

        private void UpdateClientScreen( IEnumerable<Client> movedClientList )
        {
            var mov = movedClientList.ToList( );
            foreach ( var movedClient in mov )
            {
                IScreenWorld fromWorld = null;
                IScreenWorld toWorld = null;
                foreach ( var inscreen in WindowManager.ScreenList )
                {
                    if ( inscreen.IsSameScreen( movedClient.Screen ) )
                    {
                        toWorld = inscreen;
                    }
                    if ( inscreen.IsSameScreen( movedClient.PrevScreen ) )
                    {
                        fromWorld = inscreen;
                    }
                }

                // 一部のウィンドウはスクリーン名が空のときがあった
                TagManager fromWorldTagManager = fromWorld?.Tag( WindowManager.SelectedTag );
                if ( fromWorldTagManager == null )
                {
                    continue;
                }
                // 生成されたばかりのウィンドウの場合もmoveされ、マネージとここで2回分追加される
                if ( fromWorldTagManager.HasClient( movedClient.Hwnd ) )
                {

                    fromWorldTagManager.RemoveClient( movedClient );
                    TagManager toWorldTagManager = toWorld.Tag( WindowManager.SelectedTag );
                    toWorldTagManager.AddClient( movedClient );
                }
;
            }
        }

        private void UpdateSlaveSize()
        {
            foreach ( var item in SlaveList( ) )
            {
                if ( item.HasSizeUpdate( ) )
                {

                }
            }
        }

        private void CalcSlaveSizeFromMaster()
        {
            Client masterClient = WindowManager.TryGetMaster( );
            if ( masterClient == null )
            {
                return;
            }
            if ( masterClient.HasSizeUpdate( ) )
            {
                foreach ( var slave in SlaveList( ) )
                {
                    int x = masterClient.Rect.Width;
                    slave.SetXW( x , ScreenGeom.Width - x );
                }
            }
        }

        IScreenWorld FindScreen( Screen screen )
        {
            foreach ( var item in WindowManager.ScreenList )
            {
                if ( item.IsSameScreen(screen) )
                {
                    return item;
                }
            }
            Debug.Assert( false );
            return null;
        }

        private void ArrangeIfScreenChanged()
        {
            foreach ( var client in WindowManager.SelectedClientList() )
            {
                if ( client.ScreenChanged )
                {
                    DebugLogger.GlobalLogger.Warn( $"scr changed {client.Title}" );
                }
            }
        }

        Client GetClient( IntPtr hwnd )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var tagMan in WindowManager.ScreenList[0].TagList )
            {
                var foundItem = tagMan.TryGetClient( hwnd );
                if ( foundItem != null )
                {
                    return foundItem;
                }
            }
            return null;
        }

        Client Manage( IntPtr hwnd , TileMode tileMode )
        {

            if ( ContainClient( hwnd ) )
            {
                return GetClient( hwnd );
            }
            WindowInfo windowInfo = new WindowInfo( );
            User32Methods.GetWindowInfo( hwnd , ref windowInfo );
            Client client = CreateClient( hwnd );
            if ( tileMode == TileMode.Tile )
            {

                WindowPlacement windowPlacement = new WindowPlacement( );
                if ( ThreadWindowHandles.IsWindowVisible( client.Hwnd ) > 0 )
                {
                    User32Methods.SetWindowPlacement( hwnd , ref windowPlacement );
                }

                if ( ThreadWindowHandles.IsWindowVisible( hwnd ) > 0 )
                {
                    Rectangle windowRect = windowInfo.WindowRect;
                    DWM.resize( client , windowRect.Left , windowRect.Top , windowRect.Width , windowRect.Height , ScreenGeom.Rect );
                }
            }
            client.TileMode = tileMode;

            WindowManager.Attach( client , WindowManager.SelectedTag );
            return client;
        }

        private static Client CreateClient( IntPtr hwnd )
        {
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            if ( title == string.Empty )
            {
                title = ThreadWindowHandles.GetClassText( hwnd );
            }
            Client client = Types.createClient(
                hwnd , ThreadWindowHandles.GetParent( hwnd ) ,
                ( int )User32Methods.GetWindowThreadProcessId( hwnd , IntPtr.Zero ) ,
                 WindowStyle( hwnd ) , title );
            return client;
        }

        bool Scan( IntPtr hwnd , IntPtr lParam )
        {
            TileMode tileMode = IsManageable( hwnd );
            if ( tileMode != TileMode.NoHandle )
            {
                Manage( hwnd , tileMode);
                // chrome、最小化してるウィンドウがマスターだとサイズが0のように処理される
                ThreadWindowHandles.ShowWindow( hwnd , ThreadWindowHandles.SW.SW_RESTORE );
            }
            return true;
        }

        void SetUp()
        {
            User32Methods.EnumWindows( Scan , IntPtr.Zero );
            WindowManager.IsDirty = true;
        }


        TileMode IsManageable( IntPtr hwnd )
        {
            if ( ContainClient( hwnd ) )
            {
                return TileMode.NoHandle;
            }
            if ( hwnd == Handle )
            {
                return TileMode.NoHandle;
            }
            String windowText = ThreadWindowHandles.GetWindowText( hwnd );
            String classText = ThreadWindowHandles.GetClassText( hwnd );
            bool isFloatListName = false;
            if ( !_TileSetting.IsTilingTarget( hwnd ) )
            {
                isFloatListName = true;
            }
            if ( _TileSetting.IsBlackTarget( hwnd ) )
            {
                return TileMode.NoHandle;
            }
            Trace.WriteLine( windowText + " : className = " + classText );
#if RECOVER
            if ( 
                windowText.Contains( "Chrome" ) ||
                windowText.Contains( "Avast Secure" ) ||
                windowText.Contains( "Visual" ) ||
                classText.Contains( "CabinetWClass" )
                )
            {
                DWM.setVisibility( hwnd , true );
                return TileMode.Tile;
            }
#endif
            if ( User32Methods.GetWindowPlacement( hwnd , out WindowPlacement windowPlacement ) )
            {
                if ( (windowPlacement.ShowCmd & ShowWindowCommands.SW_HIDE) != 0 )
                {
                    return TileMode.NoHandle;
                }
            }


            var parent = ThreadWindowHandles.GetParent( hwnd );
            var owner = User32Helpers.GetWindow( hwnd , GetWindowFlag.GW_OWNER );
            WindowStyles style = WindowStyle( hwnd );
            //tosafeがほしいけど別ライブラリ内
            var exStyle = ( WindowExStyles )( User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_EXSTYLE ).ToInt32( ) );
            bool isParentOK = parent != IntPtr.Zero && IsManageable( parent ) == TileMode.Tile;
            bool isTool = ( exStyle & WindowExStyles.WS_EX_TOOLWINDOW ) != 0;
            bool isApp = ( exStyle & WindowExStyles.WS_EX_APPWINDOW ) != 0;
            if ( isParentOK && !ContainClient( parent ) )
            {
                // なんか見えないウィンドウをmanageするので
                //Manage( hwnd );
            }

            if ( ( style & WindowStyles.WS_DISABLED ) != 0 )
            {
                return TileMode.NoHandle;
            }
            // WS_POPUPWINDOW も含んでいる
            if ( ( style & WindowStyles.WS_POPUP ) != 0 )
            {
                return TileMode.NoHandle;
            }
            bool hasParent = parent != IntPtr.Zero;
            var winIsVisibleAndNoParent = !hasParent && User32Methods.IsWindowVisible( hwnd );
            if ( winIsVisibleAndNoParent || isParentOK )
            {
                if ( ( !isTool && !hasParent ) || ( isTool && isParentOK ) || ( isApp && hasParent ) )
                {
                    if ( windowText != null )
                    {

                        Trace.Write( windowText );
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style );
                    }
                    if ( isFloatListName )
                    {
                        return TileMode.Float;
                    }
                    return TileMode.Tile;
                }
            }
            return TileMode.NoHandle;
        }

        private static WindowStyles WindowStyle( IntPtr hwnd )
        {
            return ( WindowStyles )User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_STYLE ).ToInt32( );
        }

        private bool ContainClient( IntPtr hwnd )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var tagMan in WindowManager.ScreenList[0].TagList )
            {
                bool contains = tagMan.HasClient( hwnd );
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

        private List<Client> SlaveList( )
        {
            return WindowManager.SelectedTiledClient( ).Skip( 1 ).ToList( );
        }

        void Unmanage( Client client )
        {
            DWM.setClientVisibility( client , true );
            Detach( client );
        }

        private void Detach( Client client )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var screen in WindowManager.ScreenList )
            {
                foreach ( var tagMan in screen.TagList )
                {
                    var contains = tagMan.RemoveClient( client );
                    if ( contains )
                    {
                        WindowManager.IsDirty = true;
                        return;
                    }
                }
            }
        }

        void UpdateBar()
        {
            User32Helpers.SetWindowPos( Handle , HwndZOrder.HWND_TOPMOST 
                , 0 , BarY , ScreenGeom.Width , BarHeight ,
                WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOSENDCHANGING );
        }

        private void QuitButton_Click( object sender , EventArgs e )
        {
            foreach ( var screen in WindowManager.ScreenList )
            {

                foreach ( var tagMan in screen.TagList )
                {
                    //tagMan.DumpIcon( );
                }
            }
            CleanUp( );
            Close( );
            //File.WriteAllText( SettingPath , _TileSetting.ToJson( ) );
        }
    }
}
