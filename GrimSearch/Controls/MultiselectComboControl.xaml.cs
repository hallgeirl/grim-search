using GrimSearch.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            InitializeComponent();

            var vm = LayoutRoot.DataContext;
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
        
        public ObservableCollection<MultiselectComboItem> ItemsSource
        {
            get
            {
                
                return (ObservableCollection<MultiselectComboItem>)GetValue(ItemsSourceProperty);

            }
            set
            {
                SetValue(ItemsSourceProperty, value);
                SelectorListView.ItemsSource = value;
            }
        }

        public event EventHandler SelectionChanged;

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<MultiselectComboItem>), typeof(MultiselectComboControl),
            new FrameworkPropertyMetadata
            {
                DefaultValue = new ObservableCollection<MultiselectComboItem>(),
                DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });



        public string Header
        {
            get
            {
                return (string)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }
        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(MultiselectComboControl));


        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(MultiselectComboControl));

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
