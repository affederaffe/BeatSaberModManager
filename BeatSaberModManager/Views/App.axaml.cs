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
        private readonly IResolver _resolver = null!;
        private readonly ILogger _logger = null!;
        private readonly LocalizationManager _localizationManager = null!;
        private readonly ThemeManager _themeManager = null!;

        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public App() { }

        /// <inheritdoc />
        public App(IResolver resolver, ILogger logger, LocalizationManager localizationManager, ThemeManager themeManager)
        {
            _resolver = resolver;
            _logger = logger;
            _localizationManager = localizationManager;
            _themeManager = themeManager;
            DataTemplates.Add(new ViewLocator(resolver));
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
            lifetime.MainWindow = _resolver.Resolve<Window>();
        }

        [SuppressMessage("ReSharper", "AsyncVoidMethod")]
        private async void HandleException(Exception e)
        {
            _logger.Fatal(e, "Application crashed");
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow.Show();
            await new ExceptionWindow(e).ShowDialog(lifetime.MainWindow);
            lifetime.Shutdown(-1);
        }
    }
}