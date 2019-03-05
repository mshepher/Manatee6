using System;
using Serilog;
using Serilog.Sinks.Xamarin;
using Serilog.Sinks;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Rg.Plugins.Popup;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Manatee7.Droid
{
  [Activity(Label = "Manatee7", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
    public static MainActivity MostRecentActivity { private set; get; }
    protected override void OnCreate(Bundle savedInstanceState) {
      Log.Logger = new LoggerConfiguration().
          WriteTo.AndroidLog(outputTemplate:
              "[{Level:u3} {ThreadId}]{Caller}: {Message:j}").
          Enrich.WithProperty(Constants.SourceContextPropertyName, "Manatee").
          Enrich.WithProperty(Constants.SourceContextPropertyName, "Pterodactyl").
          Enrich.WithThreadId().Enrich.WithCaller().
          CreateLogger();
      MostRecentActivity = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
      Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

    protected override void OnResume() {
      base.OnResume();
      try {
        //fixme App.PostOffice.Subscribe();
      } catch (NullReferenceException) {
        Log.Error("No post office.  You're running in debug mode, right?");
      }
    }

    protected override void OnDestroy() {
      Log.CloseAndFlush();
      try {
        PostOffice.Instance.Hibernate();
      } catch (NullReferenceException) {
        Log.Error("No post office.  You're running in debug mode, right?");
      }
      base.OnDestroy();
    }
  }

  internal class CallerEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
      var skip = 3;
      while (true) {
        var stack = new StackFrame(skip);
        if (!stack.HasMethod()) {
          logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
          return;
        }

        var method = stack.GetMethod();
        if (method.DeclaringType != null && method.DeclaringType.Assembly != typeof(Log).Assembly) {
          var caller = $"{method.DeclaringType.Name}.{method.Name}";
          caller = $"{caller,-40}";
          logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
        }

        skip++;
      }
    }
  }

  static class LoggerCallerEnrichmentConfiguration {
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration) {
      return enrichmentConfiguration.With<CallerEnricher>();
    }
    }
}
