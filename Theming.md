# Theming
### Built In

These come with the program, and you can't change them, however you can overwrite them by creating one of the other two theme types with the same name.\
If you have a particularly popular theme, you can submit a Pull Request to add your theme as a built-in theme.

### Custom Themes
Custom themes are loaded from the directory selected in the settings.

### Create Themes
To create your own theme, create a new file named `{ThemeName}.axaml`.\
See the [AvaloniaUI documentation](https://docs.avaloniaui.net/docs/styling) for the basic syntax.
Currently, you cannot load external pictures due to a [limitation of Avalonia](https://github.com/AvaloniaUI/Avalonia/issues/2183), so you're stuck with overriding colors for now.
To see which resource names are used, browse the `.axaml` files in the [source code](https://github.com/affederaffe/BeatSaberModManager/tree/main/BeatSaberModManager).\
It is important to include a base theme, for example `BaseDark.xaml` and `FluentControlResourcesDark.xaml`.

#### Example
    <Styles xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

        <StyleInclude Source="avares://Avalonia.Themes.Fluent/Accents/BaseDark.xaml" />
        <StyleInclude Source="avares://Avalonia.Themes.Fluent/Accents/FluentControlResourcesDark.xaml" />

        <Styles.Resources>
            <SolidColorBrush x:Key="SystemErrorBrush" Color="#7289DA" />
            <SolidColorBrush x:Key="ButtonBackground" Color="#303237" />
            <SolidColorBrush x:Key="ButtonBorderBrush" Color="#202225" />
            <SolidColorBrush x:Key="SystemChromeLowColor" Color="#202225" />
            <SolidColorBrush x:Key="BackgroundMediumLowBrush" Color="#36393F" />
            <SolidColorBrush x:Key="BackgroundMediumBrush" Color="#2F3136" />
            <SolidColorBrush x:Key="BackgroundHighBrush" Color="#36393F" />
            <SolidColorBrush x:Key="InfoIconBrush" Color="#7289DA" />
            <SolidColorBrush x:Key="DashboardIconBrush" Color="#7289DA" />
            <SolidColorBrush x:Key="LibraryIconBrush" Color="#7289DA" />
            <SolidColorBrush x:Key="SettingsIconBrush" Color="#7289DA" />
        </Styles.Resources>

    </Styles>
