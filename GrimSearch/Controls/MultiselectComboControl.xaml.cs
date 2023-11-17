using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GrimSearch.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GrimSearch.Controls
{
    /// <summary>
    /// Interaction logic for MultiselectComboControl.xaml
    /// </summary>
    public partial class MultiselectComboControl : UserControl
    {
        //MultiselectComboControlViewModel _viewModel;

        private bool _fireCheckboxEvents = true;

        public MultiselectComboControl()
        {
            AvaloniaXamlLoader.Load(this);

            //TODO: Fix - avalonia rewrite
            //var vm = Parent.DataContext;
        }

        private void DeselectAllItemTypes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _fireCheckboxEvents = false;
                foreach (var i in ItemsSource)
                {
                    var item = i as MultiselectComboItem;

                    if (item != null)
                        item.Selected = false;
                }
            }
            finally
            {
                _fireCheckboxEvents = true;
            }

            SelectionChanged?.Invoke(this, new EventArgs());
        }

        private void SelectAllItemTypes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _fireCheckboxEvents = false;
                foreach (var i in ItemsSource)
                {
                    var item = i as MultiselectComboItem;

                    if (item != null)
                        item.Selected = true;
                }
            }
            finally
            {
                _fireCheckboxEvents = true;
            }

            SelectionChanged?.Invoke(this, new EventArgs());
        }

        ObservableCollection<MultiselectComboItem> _itemsSource;
        public ObservableCollection<MultiselectComboItem> ItemsSource
        {
            get
            {

                return _itemsSource;

            }
            set
            {
                SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
                //TODO: Fix - avalonia rewrite
                //SelectorListView.ItemsSource = value;
            }
        }

        public event EventHandler SelectionChanged;

        public static readonly DirectProperty<MultiselectComboControl, ObservableCollection<MultiselectComboItem>> ItemsSourceProperty
         = AvaloniaProperty.RegisterDirect<MultiselectComboControl, ObservableCollection<MultiselectComboItem>>(nameof(ItemsSource), o => o.ItemsSource, (o, v) => o.ItemsSource = v);
        /*public static StyledProperty ItemsSourceProperty = StyledProperty.Register("ItemsSource", typeof(ObservableCollection<MultiselectComboItem>), typeof(MultiselectComboControl),
        //TODO: Fix - avalonia rewrite
            new FrameworkPropertyMetadata
            {
                DefaultValue = new ObservableCollection<MultiselectComboItem>(),
                DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });*/



        string _header;
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                SetAndRaise(HeaderProperty, ref _header, value);
            }
        }
        public static readonly DirectProperty<MultiselectComboControl, string> HeaderProperty = AvaloniaProperty.RegisterDirect<MultiselectComboControl, string>(nameof(Header), o => o.Header, (o, s) => o.Header = s);


        ICommand _command;
        public ICommand Command
        {
            get
            {
                return _command;
            }
            set
            {
                SetAndRaise(CommandProperty, ref _command, value);
            }
        }
        public static readonly DirectProperty<MultiselectComboControl, ICommand> CommandProperty = AvaloniaProperty.RegisterDirect<MultiselectComboControl, ICommand>(nameof(Command), o => o.Command, (o, v) => o.Command = v);

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_fireCheckboxEvents)
            {
                SelectionChanged?.Invoke(this, new EventArgs());
                if (Command != null)
                    Command.Execute(null);
            }
        }
    }
}
