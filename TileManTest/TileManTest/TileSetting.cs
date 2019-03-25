using Handles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Types;

namespace TileManTest
{
    class ClientSetting
    {
        public ClientSetting(
          string windowname ,
          string windowclass ,
          string tagid ,
          TileMode _tilemode
        )
        {
            WindowName = windowname;
            WindowClass = windowclass;
            TagId = tagid;
            _TileMode = _tilemode;

        }

        public string WindowName
        {
            get;
            private set;
        }
        public string WindowClass
        {
            get;
            private set;
        }
        public string TagId
        {
            get;
            private set;
        }
        public TileMode _TileMode
        {
            get;
            private set;
        }

        public bool MatchRule( IntPtr hwnd )
        {
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            var className = ThreadWindowHandles.GetClassText( hwnd );
            if ( WindowName.Contains( title ) )
            {
                return true;
            }
            return false;
        }
    }
class TileSetting
    {
        public List<string> NoTilingList = new List<string>( );

        // タイル化対象
        public bool IsTilingTarget( IntPtr hwnd )
        {
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            String classText = ThreadWindowHandles.GetClassText( hwnd );
            // 一部分で
            foreach ( var item in NoTilingList )
            {
                bool titleCont = title.Contains( item );
                bool classCont = classText.Contains( item );
                if ( titleCont || classCont )
                {
                    return false;
                }
            }
            return true;
        }

        public string ToJson()
        {
            var jsonString = JsonConvert.SerializeObject( this , Formatting.Indented ,
                new JsonConverter[] { new StringEnumConverter( ) } );
            return jsonString;
        }

        public static TileSetting Load(string path)
        {
            if ( File.Exists( path ) )
            {
                var lineList = File.ReadAllText( path );
                return JsonConvert.DeserializeObject<TileSetting>( lineList );
            }
            return new TileSetting( );
        }

        public TileSetting()
        {

            //NoTilingList = new List<string>( )
            //{
            //    "Visual",
            //    "Everything",
            //    "Orchis",
            //};
        }

    } // class
}
