using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1(List<CreatedTask> weekReportList1,
            List<ClosedTask> weekReportList2,
            List<ClosedBug> weekReportList3,
            List<NewOrActiveTask> weekReportList4)
        {
            InitializeComponent();

            listView1.ItemsSource = weekReportList1;
            listView2.ItemsSource = weekReportList2;
            listView3.ItemsSource = weekReportList3;
            listView4.ItemsSource = weekReportList4;

            // 计算总小时数并设置 Label 的 Text
            label1.Content = $"Created Task Total Original Estimate = {weekReportList1?.Sum(f => f.OriginalEstimate)} 小时";
            label2.Content = $"Closed Task Total Completed Work = {weekReportList2?.Sum(f => f.CompletedWork)} 小时";
            label3.Content = $"Closed Bug Total Completed Work = {weekReportList3?.Sum(f => f.CompletedWork)} 小时";
            label4.Content = $"New or Active Task Total Original Estimate = {weekReportList4?.Sum(f => f.OriginalEstimate)} 小时";
        }
    }
}