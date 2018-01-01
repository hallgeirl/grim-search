using GDItemSearch.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
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

namespace GDItemSearch.Controls
{
    /// <summary>
    /// Interaction logic for MultiselectComboControl.xaml
    /// </summary>
    public partial class MultiselectComboControl : UserControl
    {
        private bool _fireCheckboxEvents = true;

        public MultiselectComboControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
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

        public IEnumerable ItemsSource
        {
            get
            {
                
                return (IEnumerable)GetValue(ItemsSourceProperty);

            }
            set
            {
                SetValue(ItemsSourceProperty, value);
                SelectorListView.ItemsSource = value;
            }
        }

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

        public event EventHandler SelectionChanged;

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiselectComboControl));

        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(MultiselectComboControl));

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_fireCheckboxEvents)
                SelectionChanged?.Invoke(this, new EventArgs());
        }
    }
}
