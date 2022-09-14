using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Collections;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// A reflection-free <see cref="DataGridGroupDescription"/>.
    /// </summary>
    public class DataGridFuncGroupDescription<TItem, TKey> : DataGridGroupDescription
    {
        private readonly Func<TItem, TKey> _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFuncGroupDescription{TItem,TKey}"/> class.
        /// </summary>
        /// <param name="selector">A <see cref="Func{TResult}"/> that returns the group key for an item.</param>
        public DataGridFuncGroupDescription(Func<TItem, TKey> selector)
        {
            _selector = selector;
        }

        /// <inheritdoc />
        public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
        {
            object? result = null;
            if (item is TItem tItem)
                result = _selector.Invoke(tItem);
            return result ?? item;
        }

        /// <inheritdoc />
        public override bool KeysMatch(object groupKey, object itemKey) =>
            groupKey is TKey tGroupKey && itemKey is TKey tItemKey
                ? EqualityComparer<TKey>.Default.Equals(tGroupKey, tItemKey)
                : base.KeysMatch(groupKey, itemKey);
    }
}
