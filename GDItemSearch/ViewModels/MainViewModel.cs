using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private string _statusBarText = "";
        public string StatusBarText
        {
            get
            {
                return _statusBarText;
            }
            set
            {
                _statusBarText = value;
                RaisePropertyChangedEvent("StatusBarText");
            }
        }

       

        private ObservableCollection<string> _searchModes = new ObservableCollection<string> { "Regular", "Duplicate search" };
        public ObservableCollection<string> SearchModes
        {
            get { return _searchModes; }
            set { _searchModes = value; RaisePropertyChangedEvent("SearchModes"); }
        }


        private ObservableCollection<MultiselectComboItem> _itemTypes = new ObservableCollection<MultiselectComboItem>();
        public ObservableCollection<MultiselectComboItem> ItemTypes
        {
            get { return _itemTypes; }
            set { _itemTypes = value; RaisePropertyChangedEvent("ItemTypes"); }
        }


        private ObservableCollection<MultiselectComboItem> _itemQualities;
        public ObservableCollection<MultiselectComboItem> ItemQualities
        {
            get { return _itemQualities; }
            set { _itemQualities = value; RaisePropertyChangedEvent("ItemQualities"); }
        }


        private ObservableCollection<ItemViewModel> _searchResults = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> SearchResults
        {
            get { return _searchResults; }
            set { _searchResults = value; RaisePropertyChangedEvent("SearchResults"); }
        }

        #region Search filters

        private string _searchMode = "Regular";
        public string SearchMode
        {
            get { return _searchMode; }
            set { _searchMode = value; RaisePropertyChangedEvent("SearchMode"); }
        }

        private int _minimumLevel = 0;
        public int MinimumLevel
        {
            get { return _minimumLevel; }
            set { _minimumLevel = value; RaisePropertyChangedEvent("MinimumLevel"); }
        }


        private int _maximumLevel = 100;
        public int MaximumLevel
        {
            get { return _maximumLevel; }
            set { _maximumLevel = value; RaisePropertyChangedEvent("MaximumLevel"); }
        }


        private bool _showEquipped;
        public bool ShowEquipped
        {
            get { return _showEquipped; }
            set { _showEquipped = value; RaisePropertyChangedEvent("ShowEquipped"); }
        }

        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set { _searchString = value; RaisePropertyChangedEvent("SearchString"); }
        }
        
        #endregion

        #region Settings


        private string _grimDawnDirectory;
        public string GrimDawnDirectory
        {
            get { return _grimDawnDirectory; }
            set { _grimDawnDirectory = value; RaisePropertyChangedEvent("GrimDawnDirectory"); }
        }


        private string _grimDawnSavesDirectory;
        public string GrimDawnSavesDirectory
        {
            get { return _grimDawnSavesDirectory; }
            set { _grimDawnSavesDirectory = value; RaisePropertyChangedEvent("GrimDawnSavesDirectory"); }
        }

        #endregion

        #region commands

        #endregion

    }
}
