using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;
using BeatSaberModManager.Views.Windows;

using ReactiveUI;

using Serilog;


namespace BeatSaberModManager.Views
{
    /// <inheritdoc />
    public class App : Application
    {
        private readonly ILogger _logger = null!;
        private readonly LocalizationManager _localizationManager = null!;
        private readonly ThemeManager _themeManager = null!;
        private readonly Lazy<Window> _mainWindow = null!;

        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public App() { }

        /// <inheritdoc />
        public App(IServiceProvider services, ILogger logger, LocalizationManager localizationManager, ThemeManager themeManager, Lazy<Window> mainWindow)
        {
            _logger = logger;
            _localizationManager = localizationManager;
            _themeManager = themeManager;
            _mainWindow = mainWindow;
            DataTemplates.Add(new ViewLocator(services));
#if !DEBUG
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ShowException);
#endif
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localizationManager.Initialize(this);
            _themeManager.Initialize(this);
        }

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow = _mainWindow.Value;
        }

        [SuppressMessage("ReSharper", "AsyncVoidMethod")]
        private async void ShowException(Exception e)
        {
            _logger.Fatal(e, "Application crashed");
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow.Show();
            await new ExceptionWindow(e).ShowDialog(lifetime.MainWindow);
            lifetime.Shutdown(-1);
        }
    }
}