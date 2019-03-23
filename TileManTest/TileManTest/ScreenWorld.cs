using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TileManTest
{
    class ScreenWorld
    {
        Screen _Screen;
        int AdjacentScrenOffset;
        TagManager UnhandledTag;

        public ScreenWorld( Screen screen )
        {
            _Screen = screen;
            AdjacentScrenOffset = screen.Bounds.Left;
            UnhandledTag = new TagManager( new List<Types.Client>( ) , 20 );
        }

        public bool IsContainScreen( IntPtr hwnd )
        {
            return _Screen == Screen.FromHandle( hwnd );
        }

        public void AddUnhandledClient( Types.Client client )
        {
            UnhandledTag.AddClient( client );
        }
    }
}
