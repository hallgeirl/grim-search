using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace GrimSearch.ViewModels
{
    public class MultiselectComboControlViewModel : ViewModelBase
    {

        private string _header;
        public string Header
        {
            get { return _header; }
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }


        private ObservableCollection<MultiselectComboItem> _items;
        public ObservableCollection<MultiselectComboItem> Items
        {
            get { return _items; }
            set { _items = value; this.RaisePropertyChanged("Items"); }
        }
    }
}
