using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Types;

namespace TileManTest
{
    class WindowManager
    {
        Client _Client;
        RECT ScreenGeom;

        public WindowManager( Client client , RECT scr)
        {
            _Client = client;
            ScreenGeom = scr;
        }
        void ResizeClient( int x , int y , int w , int h )
        {
            var c = _Client;
            Trace.WriteLine( $"{c.Title} rect : {c.Rect}" );
            Rectangle rect = new Rectangle( x , y , x + w , y + h );
            DWM.resize( c , rect.Left , rect.Top , rect.Width , rect.Height , ScreenGeom.Rect );
            Trace.WriteLine( $"after {c.Title} rect : {c.Rect}" );
        }
    }
}
