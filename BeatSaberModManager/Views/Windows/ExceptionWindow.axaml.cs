using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using JetBrains.Annotations;


namespace BeatSaberModManager.Views.Windows
{
    /// <summary>
    /// Dialog that displays an <see cref="Exception"/>.
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public ExceptionWindow() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionWindow"/> class.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> to display.</param>
        public ExceptionWindow(Exception e)
        {
            InitializeComponent();
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            TransparencyLevelHint = OperatingSystem.IsWindowsVersionAtLeast(11) ? WindowTransparencyLevel.Mica : WindowTransparencyLevel.Blur;
            Margin = ExtendClientAreaToDecorationsHint ? WindowDecorationMargin : new Thickness();
            ExceptionTextBlock.Text = e.ToString();
        }

        [UsedImplicitly]
        private void OkButtonClicked(object? sender, RoutedEventArgs e) => Close();
    }
}
