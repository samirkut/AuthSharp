using System;
using System.Composition;
using DevZH.UI;

namespace AuthSharp.View
{
    public class MainWindow : Window
    {
        private VerticalBox _mainBox;

        [Import]
        public IDataAccess DataAccess { get; set; }

        public MainWindow()
                : base("2FA", 250, 500, true)
        {
            AllowMargins = true;
        }

        public void Initialize()
        {
            _mainBox = new VerticalBox();
            this.Child = _mainBox;

            Refresh();
        }

        private void Refresh()
        {
            //do the children need to be disposed of?
            _mainBox.Children.Clear();

            var entries = DataAccess.GetEntries();
            foreach (var item in entries)
            {
                _mainBox.Children.Add(new AccountEntryViewer(item));

                _mainBox.Children.Add(new Label(" "));
            }
        }
    }
}