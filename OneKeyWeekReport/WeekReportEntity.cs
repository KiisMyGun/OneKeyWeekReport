namespace OneKeyWeekReport
{
	public class WeekReportEntity
	{
		public string Id { get; set; }
		public string Project { get; set; }
		public string TaskName { get; set; }
		public string ParentUserStory { get; set; }
	}

	public class CreatedTask : WeekReportEntity
	{
		public double OriginalEstimate { get; set; }
	}

	public class ClosedTask : WeekReportEntity
	{
		public double CompletedWork { get; set; }
	}

	public class ClosedBug : WeekReportEntity
	{
		public double CompletedWork { get; set; }
	}

	public class NewOrActiveTask : WeekReportEntity
	{
		public double OriginalEstimate { get; set; }
	}
}