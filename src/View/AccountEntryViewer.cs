using DevZH.UI;
using AuthSharp.Model;

namespace AuthSharp.View
{
    public class AccountEntryViewer : VerticalBox
    {
        private AccountEntry _entry;
        public AccountEntryViewer(AccountEntry entry)
        {
            _entry = entry;
            InitializeComponent();
        }   

        private void InitializeComponent()
        {
            var group = new Group(_entry.Name);
            Children.Add(group);
            
            var vbox = new VerticalBox();
            group.Child = vbox;

            var code = new Label("123456");
            
            vbox.Children.Add(code);

            var progress = new ProgressBar();
            progress.Value = 30;
            vbox.Children.Add(progress);

            vbox.Text = "qq";
        }
    }
}