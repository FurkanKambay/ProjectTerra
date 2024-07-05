using UnityEngine;
using UnityEngine.UIElements;

namespace Tulip.UI
{
    public static class GlobalConverters
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RegisterGlobalConverters()
        {
            ConverterGroups.RegisterGlobalConverter(
                (ref StyleEnum<Visibility> visibility) => visibility == Visibility.Visible
            );

            ConverterGroups.RegisterGlobalConverter<bool, StyleEnum<Visibility>>(
                (ref bool value) => value ? Visibility.Visible : Visibility.Hidden
            );

            ConverterGroups.RegisterGlobalConverter(
                (ref StyleEnum<DisplayStyle> visibility) => visibility == DisplayStyle.Flex
            );

            ConverterGroups.RegisterGlobalConverter<bool, StyleEnum<DisplayStyle>>(
                (ref bool value) => value ? DisplayStyle.Flex : DisplayStyle.None
            );
        }
    }
}
