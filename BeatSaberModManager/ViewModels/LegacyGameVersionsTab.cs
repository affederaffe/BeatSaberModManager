using System;
using System.Collections.Generic;
using System.Linq;


namespace BeatSaberModManager.ViewModels
{
    /// <inheritdoc />
    public class LegacyGameVersionsTab : ViewModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        public LegacyGameVersionsTab(IGrouping<int, GameVersionViewModel> group, LegacyGameVersionsViewModel legacyGameVersionsViewModel)
        {
            ArgumentNullException.ThrowIfNull(group);
            Year = group.Key;
            Versions = group.ToArray();
            LegacyGameVersionsViewModel = legacyGameVersionsViewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IList<GameVersionViewModel> Versions { get; }

        /// <summary>
        /// 
        /// </summary>
        public LegacyGameVersionsViewModel LegacyGameVersionsViewModel { get; }

    }
}
