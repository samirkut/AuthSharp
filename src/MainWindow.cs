using System;
using System.Composition;
using DevZH.UI;

namespace AuthSharp
{
    public class MainWindow : Window
    {

        [Import]
        public IDataAccess DataAccess { get; set; }
        public MainWindow()
                : base("2FA", 250, 500, false)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AllowMargins = true;
        }
    }
}