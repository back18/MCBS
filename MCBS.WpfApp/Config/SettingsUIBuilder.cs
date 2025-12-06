using iNKORE.UI.WPF.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Config.Items;
using MCBS.WpfApp.Converters;
using MCBS.WpfApp.UserControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace MCBS.WpfApp.Config
{
    public static class SettingsUIBuilder
    {
        private static readonly Type BOOL_TYPE;
        private static readonly Type STRING_TYPE;
        private static readonly HashSet<Type> INTEGER_TYPES;
        private static readonly HashSet<Type> FLOAT_TYPES;

        static SettingsUIBuilder()
        {
            BOOL_TYPE = typeof(bool);
            STRING_TYPE = typeof(string);

            INTEGER_TYPES =
            [
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong)
            ];

            FLOAT_TYPES =
            [
                typeof(float),
                typeof(double),
                typeof(decimal)
            ];
        }

        public static async Task<ScrollViewer> BuildSettingsUIAsync(object model)
        {
            ConfigItem[] configItems = await Task.Run(() => ConfigItem.LoadAllConfigItem(model));
            return BuildSettingsUI(configItems);
        }

        public static ScrollViewer BuildSettingsUI(object model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            ConfigItem[] configItems = ConfigItem.LoadAllConfigItem(model);
            return BuildSettingsUI(configItems);
        }

        public static ScrollViewer BuildSettingsUI(ConfigItem[] configItems)
        {
            ArgumentNullException.ThrowIfNull(configItems, nameof(configItems));

            ScrollViewer scrollViewer = new() { Margin = new(10) };
            SimpleStackPanel simpleStackPanel = new() { Spacing = 10 };
            var settingsControls = configItems.Select(BuildSettingsControl);

            scrollViewer.Content = simpleStackPanel;
            foreach (var settingsCard in settingsControls)
                simpleStackPanel.Children.Add(settingsCard);

            return scrollViewer;
        }

        public static Control BuildSettingsControl(ConfigItem configItem)
        {
            ArgumentNullException.ThrowIfNull(configItem, nameof(configItem));

            if (configItem is GroupConfigItem)
                return BuildSettingsGroup(configItem);
            else if (configItem is ListConfigItem)
                return BuildSettingsExpander(configItem);
            else
                return BuildSettingsCard(configItem);
        }

        private static SettingsCard BuildSettingsCard(ConfigItem configItem)
        {
            SettingsCard card = new()
            {
                Header = configItem.Name,
                Description = configItem.Description
            };

            if (BuildValueControl(configItem) is Control control)
                card.Content = control;

            return card;
        }

        private static SettingsExpander BuildSettingsExpander(ConfigItem configItem)
        {
            SettingsExpander expander = new()
            {
                Header = configItem.Name,
                Description = configItem.Description
            };

            if (BuildValueControl(configItem) is Control control)
            {
                SettingsCard card = new()
                {
                    ContentAlignment = ContentAlignment.Vertical,
                    Content = control
                };
                expander.Items.Add(card);
            }

            return expander;
        }

        private static SettingsCard BuildSettingsGroup(ConfigItem configItem)
        {
            SettingsCard group = new()
            {
                Header = configItem.Name,
                Description = configItem.Description,
                IsClickEnabled = true,
                CommandParameter = configItem.Identifier
            };

            group.SetBinding(SettingsCard.CommandProperty, "NavigateToSubconfigCommand");
            return group;
        }

        private static Control? BuildValueControl(ConfigItem configItem)
        {
            if (configItem is SelectorConfigItem selectorConfigItem)
            {
                ComboBox comboBox = new()
                {
                    Width = 240,
                    ItemsSource = selectorConfigItem.AllowedValues,
                    IsEditable = true
                };
                comboBox.SetBinding(ComboBox.TextProperty, configItem.Identifier);

                return comboBox;
            }
            else if (configItem is ListConfigItem listConfigItem)
            {
                if (!STRING_TYPE.Equals(listConfigItem.ItemType))
                    return null;

                TextListEditor textListEditor = new();
                textListEditor.SetBinding(TextListEditor.ItemsSourceProperty, listConfigItem.Identifier);
                return textListEditor;
            }

            Type type = configItem.Type;
            if (BOOL_TYPE.Equals(type))
            {
                ToggleSwitch toggleSwitch = new()
                {
                    OnContent = string.Empty,
                    OffContent = string.Empty
                };

                toggleSwitch.SetBinding(ToggleSwitch.IsOnProperty, configItem.Identifier);
                return toggleSwitch;
            }
            else if (STRING_TYPE.Equals(type))
            {
                TextBox textBox = new()
                {
                    Width = 240
                };

                textBox.SetBinding(TextBox.TextProperty, configItem.Identifier);
                return textBox;
            }
            else if (FLOAT_TYPES.Contains(type))
            {
                NumberBox numberBox = new()
                {
                    MinWidth = 120,
                    Width = 240,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
                };
                numberBox.SetBinding(NumberBox.ValueProperty, configItem.Identifier);

                if (GetRange(configItem) is Range range)
                {
                    numberBox.Minimum = range.Minimum;
                    numberBox.Maximum = range.Maximum;
                    numberBox.Description = $"Range: {range.Minimum} ~ {range.Maximum}";
                }

                return numberBox;
            }
            else if (INTEGER_TYPES.Contains(type))
            {
                NumberBox numberBox = new()
                {
                    MinWidth = 120,
                    Width = 240,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                    NumberFormatter = new NumberFormatter(0)
                };
                Binding binding = new(configItem.Identifier)
                {
                    Converter = new IntToDoubleConverter()
                };
                numberBox.SetBinding(NumberBox.ValueProperty, binding);

                if (GetRange(configItem) is Range range)
                {
                    numberBox.Minimum = range.Minimum;
                    numberBox.Maximum = range.Maximum;
                    numberBox.Description = $"Range: {range.Minimum} ~ {range.Maximum}";
                }

                return numberBox;
            }
            else
            {
                return null;
            }
        }

        private static Range? GetRange(ConfigItem configItem)
        {
            if (configItem is RangeConfigItem rangeConfigItem)
            {
                double? minimum = (rangeConfigItem.Minimum as IConvertible)?.ToDouble(null);
                double? maximum = (rangeConfigItem.Maximum as IConvertible)?.ToDouble(null);

                if (minimum.HasValue && maximum.HasValue)
                {
                    return new(minimum.Value, maximum.Value);
                }
            }

            return null;
        }

        private record Range(double Minimum, double Maximum);
    }
}
