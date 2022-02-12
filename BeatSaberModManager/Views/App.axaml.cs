using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;
using BeatSaberModManager.Views.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.Views
{
    /// <inheritdoc />
    public class App : Application
    {
        private readonly IServiceProvider _services = null!;
        private readonly ILogger<App> _logger = null!;
        private readonly IOptions<AppSettings> _appSettings = null!;
        private readonly LocalizationManager _localizationManager = null!;
        private readonly ThemeManager _themeManager = null!;
        private readonly Action<ILogger<App>, Exception> _logCrash = null!;

        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public App() { }

        /// <inheritdoc />
        [ActivatorUtilitiesConstructor]
        public App(IServiceProvider services, ILogger<App> logger, IOptions<AppSettings> appSettings, LocalizationManager localizationManager, ThemeManager themeManager)
        {
            _services = services;
            _logger = logger;
            _appSettings = appSettings;
            _localizationManager = localizationManager;
            _themeManager = themeManager;
            _logCrash = LoggerMessage.Define(LogLevel.Critical, new EventId(-1), string.Empty);
            DataTemplates.Add(new ViewLocator(services));
#if !DEBUG
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(HandleException);
#endif
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localizationManager.Initialize(l => Resources.MergedDictionaries[0] = l.ResourceProvider);
            _themeManager.Initialize(t => Styles[0] = t.Style);
        }

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow = _services.GetRequiredService<Window>();
            if (Environment.CurrentDirectory == _appSettings.Value.InstallDir)
                RxApp.DefaultExceptionHandler.OnNext(new InvalidOperationException("Application cannot be executed in the game's installation directory"));
        }

        [SuppressMessage("ReSharper", "AsyncVoidMethod")]
        private async void HandleException(Exception e)
        {
            _logCrash(_logger, e);
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow.Show();
            await new ExceptionWindow(e).ShowDialog(lifetime.MainWindow);
            lifetime.Shutdown(-1);
        }
    }
}