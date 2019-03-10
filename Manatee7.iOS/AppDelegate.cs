using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Serilog;
using Serilog.Sinks.Xamarin;
using NearbyMessages;
using Xamarin.Forms;
using Manatee7.iOS;
using System.Runtime.CompilerServices;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Xamarin;
using Syncfusion.ListView.XForms.iOS;
using Syncfusion.SfNumericUpDown.XForms.iOS;
using Syncfusion.XForms.iOS.Buttons;

//using Message = NearbyMessages.GNSMessage;

using Foundation;
using UIKit;

namespace Manatee7.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.NSLog(outputTemplate: 
            //  "[{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}").CreateLogger();
            Log.Logger = new Serilog.LoggerConfiguration().Enrich.WithThreadId().Enrich.WithCaller().
              WriteTo.NSLog(outputTemplate:
              "[{Level:u3} {ThreadId}]{Caller}: {Message:j}").CreateLogger();
            Log.Information("In AppDelegate; Logger created");
            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();
            KeyboardOverlap.Forms.Plugin.iOSUnified.KeyboardOverlapRenderer.Init();
            SfListViewRenderer.Init();
            SfNumericUpDownRenderer.Init();
            SfCheckBoxRenderer.Init();

            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        //https://forums.xamarin.com/discussion/141808/system-dialogs-triggering-onsleep-ios
        public override void DidEnterBackground(UIApplication uiApplication)
        {
            MessagingCenter.Send("iOSSleep", "iOSSleep");
            base.DidEnterBackground(uiApplication);
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            base.WillEnterForeground(uiApplication);
            MessagingCenter.Send("iOSWake", "iOSWake");
        }
    }

    internal class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;
            while (true)
            {
                var stack = new StackFrame(skip);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method.DeclaringType.Assembly != typeof(Log).Assembly)
                {
                    var caller = $"{method.DeclaringType.Name}.{method.Name}";
                    caller = $"{caller,-40}";
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                }

                skip++;
            }
        }
    }

    static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }
}
