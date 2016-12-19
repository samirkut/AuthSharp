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
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AllowMargins = true;

            _mainBox = new VerticalBox();
            this.Child = _mainBox;

            _mainBox.Children.Add(new Entry());

            _mainBox.Children.Add(new Entry());

            _mainBox.Children.Add(new Entry());
        }
    }
}