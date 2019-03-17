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
          List<Client> clientlist , int id
        )
        {
            ClientList = clientlist;
            Id = id;
            ClientsAreVisible = true;
        }

        public List<Client> ClientList
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

    }


}
