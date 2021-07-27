using Avalonia.Controls;


namespace BeatSaberModManager.Localisation
{
    public class Language
    {
        public Language(string name, IResourceProvider resourceProvider)
        {
            Name = name;
            ResourceProvider = resourceProvider;
        }

        public string Name { get; }
        public IResourceProvider ResourceProvider { get; }
    }
}