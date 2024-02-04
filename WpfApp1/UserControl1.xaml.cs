using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public string OrganizationText
        {
            get { return organizationEntry.Text; }
            set { organizationEntry.Text = value; }
        }

        public string ProjectText
        {
            get { return projectEntry.Text; }
            set { projectEntry.Text = value; }
        }

        public string TokenText
        {
            get { return tokenEntry.Text; }
            set { tokenEntry.Text = value; }
        }
    }
}