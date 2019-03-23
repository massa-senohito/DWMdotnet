using Handles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileManTest
{
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
