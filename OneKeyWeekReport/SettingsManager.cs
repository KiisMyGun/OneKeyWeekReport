using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneKeyWeekReport
{
	public class SettingsManager
	{
		private const string SettingsFileName = "appsettings.json";

		public List<FormBoxView> LoadSettings()
		{
			try
			{
				if (File.Exists(SettingsFileName))
				{
					string json = File.ReadAllText(SettingsFileName);
					return JsonSerializer.Deserialize<List<FormBoxView>>(json);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading settings: {ex.Message}");
			}

			return new List<FormBoxView>();
		}

		public void SaveSettings(List<FormBoxView> settings)
		{
			try
			{
				string json = JsonSerializer.Serialize(settings);
				File.WriteAllText(SettingsFileName, json);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving settings: {ex.Message}");
			}
		}
	}
}