using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCBS.WpfApp.UserControls
{
    /// <summary>
    /// TextListEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TextListEditor : UserControl
    {
        public TextListEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList<string>),
            typeof(TextListEditor),
            new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsMeasure |
            FrameworkPropertyMetadataOptions.AffectsArrange,
            OnItemsSourceChanged));

        public IList<string> ItemsSource
        {
            get => (IList<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextListEditor control)
                control.OnItemsSourceChanged(e.NewValue);
        }

        private void OnItemsSourceChanged(object newValue)
        {
            ListBox.ItemsSource = newValue as IList<string>;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTextBox();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButton();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox.ItemsSource is IList<string> list && !list.IsReadOnly)
            {
                int index = ListBox.SelectedIndex;
                if (index == -1)
                {
                    list.Add(TextBox.Text);
                    index = list.Count - 1;
                }
                else
                {
                    string text = list[index];
                    if (TextBox.Text == text)
                    {
                        index++;
                        list.Insert(index, string.Empty);
                    }
                    else
                    {
                        list[index] = TextBox.Text;
                    }
                }

                ListBox.SelectedIndex = index;
                UpdateButton();
            }
        }

        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = ListBox.SelectedIndex;
            if (index == -1)
                return;

            if (ListBox.ItemsSource is IList<string> list && !list.IsReadOnly)
            {
                list.RemoveAt(index);

                if (index >= list.Count)
                    index = list.Count - 1;

                ListBox.SelectedIndex = index;
                UpdateButton();
            }
        }

        private void UpdateButton()
        {
            if (ListBox.SelectedItem is string text)
            {
                if (TextBox.Text == text)
                    Add_Button.Content = "添加";
                else
                    Add_Button.Content = "修改";
            }
        }

        private void UpdateTextBox()
        {
            if (ListBox.SelectedItem is string text)
                TextBox.Text = text;
            else
                TextBox.Clear();
        }
    }
}
