using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TileManTest
{
    class DebugLogger
    {
        // https://nlog-project.org/config/?tab=targets
        private Logger Logger;
        string Name;

        static int Frame;
        Form LoggerForm; 

        string Ondate( LogEventInfo info )
        {
            var frame = Frame.ToString( ).PadLeft( 8 );
            var xml = $"<log4j:event logger=\"{Name}\" level=\"{info.Level}\" timestamp=\"{info.TimeStamp.ToLongTimeString( )}\" thread=\"1\"><log4j:message>{frame} {info.FormattedMessage}</log4j:message><log4j:properties><log4j:data name=\"log4japp\" value=\"LogTest.exe(3124)\" /><log4j:data name=\"log4jmachinename\" value=\"MYCOMPUTER\" /></log4j:properties></log4j:event>";

            var stack = info.StackTrace?.ToString( );
            //return Frame.ToString();
            return xml; 
        }

        public void Fatal<T>(T value)
        {
            Logger.Fatal( value );
        }
        public void Error<T>( T value )
        {
            Logger.Error( value );
        }
        public void Warn<T>( T value )
        {
            Logger.Warn( value );
        }
        public void Info<T>( T value )
        {
            Logger.Info( value );
        }
        public void Debug<T>( T value )
        {
            Logger.Debug( value );
        }
        public void Trace<T>( T value )
        {
            Logger.Trace( value );
        }

        public DebugLogger(string name)
        {
            Logger = LogManager.GetLogger(name);
            Name = name;
            // ログが出されたときにどういう形式で表示するか決められる マクロは自分で定義できる
            var layout = "${level} ${message} ${frame}";

            // マクロの定義方法 定義名とコールバックの組で宣言できる
            LayoutRenderer.Register( "frame" , Ondate );
            RichTextBoxTarget target = new RichTextBoxTarget( )
            {
                ControlName = "Control1" ,
                UseDefaultRowColoringRules = true ,
                Layout = layout ,
                ToolWindow = false ,
                Width = 600 ,
                Height = 600 ,
            };

            var debugStr = new NLog.Targets.DebuggerTarget(  )
            {
                Layout = layout
            };
            var log4ViewTarget = new NLog.Targets.NetworkTarget() //new NLog.Targets.NLogViewerTarget( )
            {
                Address = "udp://127.0.0.1:7071"
            }   ;
            log4ViewTarget.Layout = "${frame} ";

            var config = new LoggingConfiguration();
            config.AddRule( LogLevel.Info , LogLevel.Fatal , debugStr );
            //config.AddRule( LogLevel.Info , LogLevel.Fatal , target );
            config.AddRule( LogLevel.Info , LogLevel.Fatal , log4ViewTarget );

            LogManager.Configuration = config;

            //var form = target.TargetForm;
            //form.TopMost = true;
            //LoggerForm = form;
        }

        static readonly DebugLogger _GlobalLogger = new DebugLogger("global");
        public static DebugLogger GlobalLogger
        {
            get
            {
                return _GlobalLogger;
            }
        }

        public static void Update()
        {
            Frame++;
        }
    }
}
