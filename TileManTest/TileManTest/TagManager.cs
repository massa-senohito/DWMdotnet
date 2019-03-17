using DWMDotnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Types;

namespace TileManTest
{
    class TagManager
    {
        public TagManager(
          List<Client> clientlist

        )
        {
            ClientList = clientlist;

        }

        public void Visible(bool visible)
        {
            foreach ( var item in ClientList )
            {
                DWM.setVisibility( item.Hwnd , visible);
            }
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
        }

        public bool HasClient( IntPtr hwnd )
        {
            return ClientList.Any( c => c.Hwnd == hwnd );
        }

        public bool Remove( Client client )
        {
            var contains = ClientList.FirstOrDefault( c => c.Hwnd == client.Hwnd );
            if ( contains != null )
            {
                ClientList.Remove( contains );
                return true;
            }
            return false;
        }

        public List<Client> ClientList
        {
            get;
            private set;
        }

    }


}
