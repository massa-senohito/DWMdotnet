using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Types;

namespace TileManTest
{
    class TileWindowManager
    {

        public Client ActiveClient
        {
            get;
            set;
        }
        public List<IScreenWorld> ScreenList = new List<IScreenWorld>( );
        public string SelectedTag = "1";
        public bool IsDirty = false;
        public TagManager CurrentTag
        {
            get
            {
                return ScreenList[ 0 ].Tag( SelectedTag );
            }
        }
        public int UIHeight ;
        public RECT ScreenGeom;

        public TileWindowManager(int uiHeight)
        {
            var scrList = Screen.AllScreens;
            foreach ( var screen in scrList )
            {
                Trace.WriteLine( screen.DeviceName + " " + screen.Bounds );
                ScreenList.Add( ScreenWorld.CreateScreenWorld( screen ) );
                // https://stackoverflow.com/questions/53012896/using-setwindowpos-with-multiple-monitors
            }
            UIHeight = uiHeight;
        }

        public List<Client> SelectedClientList()
        {
            List<Client> tmp = new List<Client>( );
            foreach ( var screen in ScreenList )
            {

                IEnumerable<Client> collection = screen.ClientList( SelectedTag );
                tmp.AddRange( collection );
            }
            return tmp;
        }

        public List<Client> SelectedTiledClient()
        {
            return SelectedClientList( ).Where( c => c.TileMode == TileMode.Tile ).ToList();
        }

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

        public void TagSignal( HotKey item )
        {
            bool send = 9 < item.ID;
            TagManager currentTag = CurrentTag;
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
                    currentTag.RemoveClient( ActiveClient );
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

        public void ChangeTag( TagManager currentTag , string nextTag )
        {
            if ( nextTag == SelectedTag )
            {
                return;
            }
            ActiveClient = null;
            // 移動前にマスターの長さを保存
            CurrentTag.MasterWidth = TryGetMaster( )?.W ?? TagManager.DefaultMasterWidth;
            foreach ( var screen in ScreenList )
            {
                screen.ChangeTag( currentTag , nextTag );
            }
            // master1つなら大きさはスクリーンサイズになる、するとスレイブの大きさが0になってしまうので
            if ( CurrentTag.MasterWidth == ScreenGeom.Width )
            {
                CurrentTag.MasterWidth = TagManager.DefaultMasterWidth;
            }

            foreach ( var screen in ScreenList )
            {
                screen.SetAllWindowFore( );
            }
            SelectedTag = nextTag;
            Tile( );
        }

        public void Tile()
        {
            foreach ( var screen in ScreenList )
            {
                Trace.WriteLine( $"Tile() {screen}" );
                Trace.Indent( );
                screen.Tile( SelectedTag , UIHeight );
                Trace.Unindent( );
            }
        }

        public Client TryGetMaster( )
        {
            var master = SelectedTiledClient( ).FirstOrDefault( );
            //Trace.WriteLine( master?.Title );
            return master;
        }
        public void Attach( Client client , string dest )
        {
#if MultiScreen
            foreach ( var screen in ScreenList )
            {
                if ( screen.IsSameScreen( client.Screen ) )
                {
                    screen.Attach( client , dest );
                }
            }
#else
            ScreenList[ 0 ].Attach( client , dest );
#endif
            IsDirty = true;
        }
    }
}
