namespace OneKeyWeekReport;

public partial class NewPage1 : ContentPage
{
	public NewPage1(List<CreatedTask> weekReportList1,
			List<ClosedTask> weekReportList2,
			List<ClosedBug> weekReportList3,
			List<NewOrActiveTask> weekReportList4)
	{
		InitializeComponent();

		listView1.ItemsSource = weekReportList1;
		listView2.ItemsSource = weekReportList2;
		listView3.ItemsSource = weekReportList3;
		listView4.ItemsSource = weekReportList4;

		// ������Сʱ�������� Label �� Text
		label1.Text = $"Created Task Total Original Estimate = {weekReportList1?.Sum(f => f.OriginalEstimate)} Сʱ";
		label2.Text = $"Closed Task Total Completed Work = {weekReportList2?.Sum(f => f.CompletedWork)} Сʱ";
		label3.Text = $"Closed Bug Total Completed Work = {weekReportList3?.Sum(f => f.CompletedWork)} Сʱ";
		label4.Text = $"New or Active Task Total Original Estimate = {weekReportList4?.Sum(f => f.OriginalEstimate)} Сʱ";
	}
}