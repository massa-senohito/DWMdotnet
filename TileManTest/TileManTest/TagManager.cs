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
        public TagManager(
          List<Client> clientlist , int id
        )
        {
            ClientList = clientlist;
            Id = id;
            ClientsAreVisible = true;
            IconList = new List<Icon>( );
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

        public Client GetClient( IntPtr hwnd )
        {
            var foundItem = ClientList.FirstOrDefault( c => c.Hwnd == hwnd );
            if ( foundItem != null )
            {
                return foundItem;
            }
            return null;
        }

        public void Add( Client c )
        {
            ClientList.Add( c );
            Icon item = Win32dll.GetAppIcon( c.Hwnd );
            IconList.Add( item );
        }

        public bool HasClient( IntPtr hwnd )
        {
            return ClientList.Any( c => c.Hwnd == hwnd );
        }

        public bool Remove( Client client )
        {
            var mayInd = ClientList.FindIndex( c => c.Hwnd == client.Hwnd );
            if ( mayInd != -1 )
            {
                ClientList.RemoveAt( mayInd );
                var icon = IconList[ mayInd ];
                IconList.RemoveAt( mayInd );
                icon?.Dispose( );
                return true;
            }
            return false;
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
