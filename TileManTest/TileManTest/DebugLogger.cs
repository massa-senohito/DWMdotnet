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
        private static Logger Logger =
            LogManager.GetLogger("NLog.UnitTests.Targets.RichTextBoxTargetTests");

        int Frame;
        Form LoggerForm; 

        string Ondate( LogEventInfo info )
        {
            // 来たメッセージやスタックトレースも出せる
            var stack = info.StackTrace?.ToString( );
            return Frame.ToString();
        }

        public void Fatal<T>(T value)
        {
            Logger.Fatal( value );
        }
        public void Frror<T>( T value )
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

        public DebugLogger()
        {
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

            var config = new LoggingConfiguration();
            config.AddRule( LogLevel.Info , LogLevel.Fatal , debugStr );
            config.AddRule( LogLevel.Info , LogLevel.Fatal , target );

            LogManager.Configuration = config;

            var form = target.TargetForm;
            form.TopMost = true;
            LoggerForm = form;
        }

        static readonly DebugLogger _GlobalLogger = new DebugLogger();
        public static DebugLogger GlobalLogger
        {
            get
            {
                return _GlobalLogger;
            }
        }

        public void Update()
        {
            Frame++;
        }
    }
}
