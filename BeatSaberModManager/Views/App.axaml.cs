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

using DryIoc;

using ReactiveUI;

using Serilog;


namespace BeatSaberModManager.Views
{
    /// <inheritdoc />
    public class App : Application
    {
        private readonly ILogger _logger = null!;
        private readonly Lazy<Window> _mainWindow = null!;
        private readonly Lazy<LocalizationManager> _localizationManager = null!;
        private readonly Lazy<ThemeManager> _themeManager = null!;

        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public App() { }

        /// <inheritdoc />
        public App(IResolver resolver, ILogger logger, Lazy<Window> mainWindow, Lazy<LocalizationManager> localizationManager, Lazy<ThemeManager> themeManager)
        {
            _logger = logger;
            _mainWindow = mainWindow;
            _localizationManager = localizationManager;
            _themeManager = themeManager;
            DataTemplates.Add(new ViewLocator(resolver));
#if !DEBUG
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ShowException);
#endif
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _ = _localizationManager.Value;
            _ = _themeManager.Value;
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