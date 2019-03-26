using DWMDotnet;
using MouseCaptureTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Types;

namespace TileManTest
{
    class TagManager
    {
        public static int DefaultMasterWidth = 1056;
        public TagManager( List<Client> clientlist , int id )
        {
            ClientList = clientlist;
            Id = id;
            ClientsAreVisible = true;
            IconList = new List<Icon>( );
            MasterWidth = DefaultMasterWidth;
        }

        public List<Client> ClientList
        {
            get;
            private set;
        }

        public List<Icon> IconList
        {
            get;
            private set;
        }

        public int Id
        {
            get;
            private set;
        }

        public int _MasterWidth;

        public int MasterWidth
        {
            get
            {
                return _MasterWidth;
            }
            set
            {
                _MasterWidth = value;
            }
        }



        public bool ClientsAreVisible
        {
            get;
            private set;
        }

        public void Visible(bool visible)
        {
            foreach ( var item in ClientList )
            {
                DWM.setClientVisibility( item , visible);
            }
            ClientsAreVisible = visible;
        }

        public Client TryGetClient( IntPtr hwnd )
        {
            var foundItem = ClientList.FirstOrDefault( c => c.Hwnd == hwnd );
            if ( foundItem != null )
            {
                return foundItem;
            }
            return null;
        }

        public void AddClient( Client c )
        {
            ClientList.Add( c );
            Icon item = Win32dll.GetAppIcon( c.Hwnd );
            IconList.Add( item );
        }

        public bool HasClient( IntPtr hwnd )
        {
            return ClientList.Any( c => c.Hwnd == hwnd );
        }

        public bool RemoveClient( Client client )
        {
            var mayInd = ClientList.FindIndex( c => c.Hwnd == client.Hwnd );
            if ( mayInd != -1 )
            {
                // アイコンが生成されないとインデックスが不一致になる
                ClientList.RemoveAt( mayInd );
                var icon = IconList[ mayInd ];
                RemoveIcon( mayInd );
                icon?.Dispose( );
                return true;
            }
            return false;
        }

        public void RemoveIcon( int ind )
        {
            IconList.RemoveAt( ind );
        }

        public IEnumerable<string> ClientTitles
        {
            get
            {
                foreach ( var item in ClientList )
                {
                    yield return item.Title;
                }
            }
        }

    } // class


} // namespace
