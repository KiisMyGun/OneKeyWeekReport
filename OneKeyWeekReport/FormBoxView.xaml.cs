namespace OneKeyWeekReport;

public partial class FormBoxView : ContentView
{
	public FormBoxView()
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