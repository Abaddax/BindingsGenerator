using System.Collections.Generic;

namespace BindingsGenerator.Core.Helper
{
    internal static class CollectionHelper
    {
        internal static void Append<T>(this ICollection<T> collection, IEnumerable<T> append)
        {
            if (append == null)
                return;
            foreach (var item in append)
            {
                collection.Add(item);
            }
        }
        internal static void UpdateFrom<T>(this ICollection<T> collection, IEnumerable<T> update)
        {
            collection.Clear();
            collection.Append(update);
        }
    }
}
