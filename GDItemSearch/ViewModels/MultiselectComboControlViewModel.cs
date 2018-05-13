using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.ViewModels
{
    public class MultiselectComboControlViewModel : ViewModelBase
    {

        private string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; RaisePropertyChangedEvent("Header"); }
        }


        private ObservableCollection<MultiselectComboItem> _items;
        public ObservableCollection<MultiselectComboItem> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChangedEvent("Items"); }
        }
    }
}
