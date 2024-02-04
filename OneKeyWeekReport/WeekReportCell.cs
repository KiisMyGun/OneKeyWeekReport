using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace OneKeyWeekReport
{
	public class WeekReportCell : ViewCell
	{
		public WeekReportCell()
		{
			// 在这里定义周报单元格的布局
			var taskIdLabel = new Label
			{
				FontSize = 18,
			};
			taskIdLabel.SetBinding(Label.TextProperty, new Binding("Id", stringFormat: "Task ID: {0}"));

			var projectNameLabel = new Label
			{
				FontSize = 16,
			};
			projectNameLabel.SetBinding(Label.TextProperty, new Binding("Project", stringFormat: "Project: {0}"));

			var taskNameLabel = new Label
			{
				FontSize = 16,
			};
			taskNameLabel.SetBinding(Label.TextProperty, new Binding("TaskName", stringFormat: "Task Name: {0}"));

			var originalEstimateLabel = new Label
			{
				FontSize = 16,
			};
			originalEstimateLabel.SetBinding(Label.TextProperty, new Binding("OriginalEstimate", stringFormat: "Original Estimate: {0} hours"));

			// ... 添加其他属性

			var stackLayout = new StackLayout
			{
				Children = { taskIdLabel, projectNameLabel, taskNameLabel, originalEstimateLabel },
				Padding = new Thickness(10)
			};

			View = stackLayout;
		}
	}
}