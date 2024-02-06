using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 属性定义

        public List<ClosedBug> ClosedBugs = new List<ClosedBug>();
        public List<NewOrActiveTask> NewOrActiveTasks = new List<NewOrActiveTask>();
        public string CreatedTaskInfo => "Created Task（本周创建任务） Total Original Estimate = 44.5 小时";

        public List<CreatedTask> CreatedTasks { get; set; } = new List<CreatedTask>();

        public string ClosedTaskInfo => "Closed Task（本周完成任务， Total Completed Work = 25.5小时";

        public List<ClosedTask> ClosedTasks { get; set; } = new List<ClosedTask>();

        public string ClosedBugInfo => "Closed Bug (本周关闭Bug)，Total Completed Work = 13 小时";
        public string NewOrActiveTaskInfo => "New or Active Task (还需要完成的任务，在New 或Active 状态的）Total Original Estimate = 20 小时";

        #endregion 属性定义

        public MainWindow()
        {
            InitializeComponent();
            LoadSetting();
        }

        public void LoadSetting()
        {
            string jsonFilePath = "settings.json";
            if (!File.Exists(jsonFilePath))
            {
                // 如果文件不存在，创建并写入初始内容
                string initialJsonContent = "{\r\n  \"Setting\": [\r\n    {\r\n      \"OrganizationText\": \"\",\r\n      \"ProjectText\": \"\",\r\n      \"TokenText\": \"\"\r\n    }\r\n  ]\r\n}";
                File.WriteAllText(jsonFilePath, initialJsonContent);
            }

            string jsonContent = File.ReadAllText(jsonFilePath);

            // 解析JSON内容
            JObject jsonObject = JObject.Parse(jsonContent);

            // 获取Setting数组
            JArray settingsArray = (JArray)jsonObject["Setting"];

            // 假设只有一个Setting对象
            if (settingsArray != null && settingsArray.Count > 0)
            {
                foreach (var item in settingsArray)
                {
                    string organizationText = item["OrganizationText"].ToString();
                    string projectText = item["ProjectText"].ToString();
                    string tokenText = item["TokenText"].ToString();

                    UserControl1 newBoxView = new UserControl1();

                    newBoxView.organizationEntry.Text = organizationText;
                    newBoxView.projectEntry.Text = projectText;
                    newBoxView.tokenEntry.Text = tokenText;

                    // 将新框添加到 mainStackLayout 中
                    mainStackLayout.Children.Insert(mainStackLayout.Children.Count - 1, newBoxView);
                }
            }
            else
            {
                MessageBox.Show("No settings found in the JSON file.");
            }
        }

        public async Task<dynamic> GetDevOpsTasks()
        {
            // 替换以下信息为您的 Azure DevOps 组织、项目和个人访问令牌
            string organization = "meetingzen";
            string project = "ams";
            string token = "ff27d6hxivzd7nbd64ivko4h33zy6s2mkksmktb7p7zyei73xxoa";
            string expand = "wiql";
            string depth = "2";

            // 获取Spring任务的 API 地址
            string apiUrl = $"https://dev.azure.com/{organization}/{project}/_apis/wit/queries?$expand={expand}&$depth={depth}&api-version=7.1-preview.2";

            using (HttpClient client = new HttpClient())
            {
                // 设置基本身份验证头部
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                // 发送请求获取任务信息
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // 将 JSON 响应转换为对象
                    dynamic springTasks = JsonConvert.DeserializeObject(responseBody);

                    // 处理任务信息
                    foreach (var task in springTasks.value)
                    {
                        Console.WriteLine($"Task ID: {task.id}, Name: {task.name}");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve tasks. Status code: {response.StatusCode}, Error message: {response.ReasonPhrase}");
                }
            }
            return null;
        }

        public async Task<T> GetWorkItemDetails<T>(string url, string organizationText, string token) where T : WeekReportEntity, new()
        {
            using (HttpClient client = new HttpClient())
            {
                // 设置 Personal Access Token
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{token}"))}");

                // 发送 GET 请求获取工作项详细信息
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    logTextBlock.Text += responseBody + "\n";
                    // 解析 JSON
                    JObject jsonObject = JObject.Parse(responseBody);

                    // 提取字段
                    string id = jsonObject["id"]?.ToString();
                    string project = jsonObject["fields"]["System.TeamProject"]?.ToString();
                    string taskName = jsonObject["fields"]["System.Title"]?.ToString();
                    double originalEstimate = Convert.ToDouble(jsonObject["fields"]?["Microsoft.VSTS.Scheduling.OriginalEstimate"]?.ToString() ?? "0");
                    string parentUserStoryId = jsonObject["fields"]["System.Parent"]?.ToString();
                    double completedWork = Convert.ToDouble(jsonObject["fields"]?["Microsoft.VSTS.Scheduling.CompletedWork"]?.ToString() ?? "0");
                    string createdDate = jsonObject["fields"][key: "System.CreatedDate"]?.ToString();

                    // 构造 Parent User Story 的 URL
                    string parentUserStoryUrl = $"https://dev.azure.com/{organizationText}/{project}/_apis/wit/workItems/{parentUserStoryId}";

                    // 获取 User Story 的实际值
                    logTextBlock.Text += $"**********获取 User Story Id {parentUserStoryId} 的实际值**********" + "\n";
                    string parentUserStory = await GetParentUserStoryValue(parentUserStoryUrl, token);

                    T result = new T();
                    typeof(T).GetProperty("Id")?.SetValue(result, id);
                    typeof(T).GetProperty("Project")?.SetValue(result, project);
                    typeof(T).GetProperty("TaskName")?.SetValue(result, taskName);
                    typeof(T).GetProperty("OriginalEstimate")?.SetValue(result, originalEstimate);
                    typeof(T).GetProperty("ParentUserStory")?.SetValue(result, parentUserStory);
                    typeof(T).GetProperty("CompletedWork")?.SetValue(result, completedWork);
                    typeof(T).GetProperty("CreatedDate")?.SetValue(result, createdDate);
                    return result;
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve work item. Status code: {response.StatusCode}");
                }
                return null;
            }
        }

        public async Task<string> GetFileIdsInfo(string fieldsUrl, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{token}"))}");

                HttpResponseMessage response = await client.GetAsync(fieldsUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else
                {
                    return $"Failed to fetch fileids information. Status code: {response.StatusCode}";
                }
            }
        }

        public async Task CreateReport()
        {
            //var settingsManager = new SettingsManager();
            // 保存配置
            List<UserControl1> allSettings = new List<UserControl1>();

            string fields = "System.Id,System.TeamProject,System.Title,Microsoft.vsts.scheduling.OriginalEstimate,Microsoft.vsts.scheduling.CompletedWork,System.Parent,System.CreatedDate";

            foreach (var child in mainStackLayout.Children)
            {
                if (child is UserControl1 formBoxView)
                {
                    logTextBlock.Text += "===============================================" + "\n";
                    logTextBlock.Text += $"**********开始访问组织{formBoxView.OrganizationText.Trim()},项目{formBoxView.ProjectText.Trim()}**********" + "\n";
                    var data = await QueryByWiql(formBoxView.OrganizationText.Trim(), formBoxView.ProjectText.Trim(), formBoxView.TokenText.Trim());

                    foreach (KeyValuePair<TaskQueryName, dynamic> item in data)
                    {
                        if (item.Value == null)
                            continue;

                        // 得到Task Url
                        List<string> urls = new();

                        // 将 dynamic 转换为 JObject
                        JObject jsonObject = JObject.FromObject(item.Value);

                        var workItems = jsonObject["workItems"].ToArray();
                        if (workItems != null)
                        {
                            foreach (var workItem in workItems)
                            {
                                urls.Add(workItem["url"].ToString() + $"?fields={fields}&api-version=7.1-preview.3");
                            }
                        }
                        // 得到Task 详情
                        foreach (var url in urls)
                        {
                            logTextBlock.Text += $"**********开始访问TaskUrl{url},{item.Key.ToString()}信息**********" + "\n";
                            // 详情插入相应的表格中
                            switch (item.Key)
                            {
                                case TaskQueryName.CreatedTask:
                                    var createTask = await GetWorkItemDetails<CreatedTask>(url, formBoxView.OrganizationText.Trim(), formBoxView.TokenText.Trim());
                                    if (createTask != null)
                                        CreatedTasks.Add(createTask);
                                    break;

                                case TaskQueryName.ClosedTask:
                                    var closedTask = await GetWorkItemDetails<ClosedTask>(url, formBoxView.OrganizationText.Trim(), formBoxView.TokenText.Trim());
                                    if (closedTask != null)
                                        ClosedTasks.Add(closedTask);
                                    break;

                                case TaskQueryName.ClosedBug:
                                    var closedBugs = await GetWorkItemDetails<ClosedBug>(url, formBoxView.OrganizationText.Trim(), formBoxView.TokenText.Trim());
                                    if (closedBugs != null)
                                        ClosedBugs.Add(closedBugs);
                                    break;

                                case TaskQueryName.NewOrActiveTask:
                                    var newOrActiveTask = await GetWorkItemDetails<NewOrActiveTask>(url, formBoxView.OrganizationText.Trim(), formBoxView.TokenText.Trim());
                                    if (newOrActiveTask != null)
                                        NewOrActiveTasks.Add(newOrActiveTask);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    logTextBlock.Text += "===============================================" + "\n";
                    //// 添加新的设置
                    //FormBoxView newSettings = new FormBoxView
                    //{
                    //	OrganizationText = formBoxView.OrganizationText,
                    //	ProjectText = formBoxView.ProjectText,
                    //	TokenText = formBoxView.TokenText
                    //};
                    //allSettings.Add(newSettings);
                }
            }
            Window1 page1 = new Window1(CreatedTasks, ClosedTasks, ClosedBugs, NewOrActiveTasks);
            page1.Show();

            // 保存设置
            //settingsManager.SaveSettings(allSettings);

            // 发送到飞书
        }

        async Task<dynamic> ExecuteWiqlQuery(HttpClient client, string organization, string project, string wiqlQuery)
        {
            // 构建 API 端点
            string apiEndpoint = $"https://dev.azure.com/{organization}/{project}/_apis/wit/wiql?api-version=7.1-preview.2";

            // 构建请求体
            var requestContent = new StringContent($"{{\"query\": \"{wiqlQuery}\"}}", Encoding.UTF8, "application/json");

            // 发送 POST 请求
            HttpResponseMessage response = await client.PostAsync(apiEndpoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                // 读取响应内容
                string responseBody = await response.Content.ReadAsStringAsync();

                logTextBlock.Text += responseBody + "\n";
                // 将 JSON 响应转换为对象
                return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseBody);
            }
            else
            {
                Console.WriteLine($"Failed to execute WIQL query. Status code: {response.StatusCode}, Error message: {response.ReasonPhrase}");
                return null;
            }
        }

        static void PrintWorkItems(dynamic queryResult)
        {
            if (queryResult != null && queryResult.workItems != null)
            {
                foreach (var workItem in queryResult.workItems)
                {
                    Console.WriteLine($"ID: {workItem.id}, Project: {workItem.fields["System.TeamProject"]}, Title: {workItem.fields["System.Title"]}");
                    // 添加其他字段的打印...
                }
            }
            else
            {
                Console.WriteLine("No results found.");
            }
        }

        private async Task<string> GetParentUserStoryValue(string parentUserStoryUrl, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                // 设置 Personal Access Token
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{token}"))}");

                // 发送 GET 请求获取 User Story 详细信息
                HttpResponseMessage response = await client.GetAsync(parentUserStoryUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    logTextBlock.Text += responseBody + "\n";

                    JObject jsonUserStory = JObject.Parse(responseBody);

                    // 获取 User Story 的实际值，这里假设实际值在 "System.Title" 字段中
                    string parentUserStory = (string)jsonUserStory["fields"]["System.Title"];
                    return parentUserStory;
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve parent User Story. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        private async Task<Dictionary<TaskQueryName, dynamic>> QueryByWiql(string organization, string project, string token)
        {
            var startOfWeek = string.IsNullOrEmpty(MyTimePicker.Text) ? (DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? DateTime.Now.AddDays(-6) : DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek)).ToShortDateString() : MyTimePicker.Text;

            //string organization = "meetingzen";
            //string project = "ams";

            /* 1. Created Task（本周创建任务） Total Original Estimate = XXX 小时
			 *	ID
				Project
				Task Name
				Original Estimate
				Parent User Story
			 *
			*/
            string wiqlQuery1 = $"select [System.Id], [System.AreaLevel1], [System.Title], [Microsoft.VSTS.Scheduling.OriginalEstimate], [System.Parent] from WorkItems where [System.TeamProject] = '{project}' and [System.CreatedBy] = @me and [System.CreatedDate] >= '{startOfWeek}' and [System.WorkItemType] = 'Task' order by [Microsoft.VSTS.Scheduling.OriginalEstimate] desc";
            /* 2. Closed Task（本周完成任务， Total Completed Work = XXX 小时
			 *	ID
				Project
				Task Name
				Completed Work
				Parent User Story
			*/
            string wiqlQuery2 = $"select [System.Id], [System.AreaLevel1], [System.Title], [Microsoft.VSTS.Scheduling.CompletedWork], [System.Parent] from WorkItems where [System.TeamProject] = '{project}' and [Microsoft.VSTS.Common.ClosedBy] = @me and [Microsoft.VSTS.Common.ClosedDate] >= '{startOfWeek}' and [System.WorkItemType] = 'Task'";
            /* 3. Closed Bug (本周关闭Bug)，Total Completed Work = XXX 小时
			 *	ID
				Project
				Bug Name
				Completed Work
				Parent User Story
			 */
            string wiqlQuery3 = $"select [System.Id], [System.AreaLevel1], [System.Title], [Microsoft.VSTS.Scheduling.CompletedWork], [System.Parent] from WorkItems where [System.TeamProject] = '{project}' and [System.WorkItemType] = 'Bug' and [Microsoft.VSTS.Common.ResolvedBy] = @me and [Microsoft.VSTS.Common.ClosedDate] >= '{startOfWeek}'";
            /* 4. New or Active Task(还需要完成的任务，在New 或Active 状态的）Total Original Estimate = XXX 小时
			 *	ID
				Project
				Task Name
				Original Estimate
				Parent User Story
			 */
            string wiqlQuery4 = $"select [System.Id], [System.AreaLevel1], [System.Title], [Microsoft.VSTS.Scheduling.OriginalEstimate], [System.Parent] from WorkItems where [System.TeamProject] = '{project}' and [System.WorkItemType] = 'Task' and ([System.State] = 'New' or [System.State] = 'Active') and [System.AssignedTo] = @me";

            using (HttpClient client = new HttpClient())
            {
                // 设置基本身份验证头部
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                // 执行查询
                var query1Result = await ExecuteWiqlQuery(client, organization, project, wiqlQuery1);
                var query2Result = await ExecuteWiqlQuery(client, organization, project, wiqlQuery2);
                var query3Result = await ExecuteWiqlQuery(client, organization, project, wiqlQuery3);
                var query4Result = await ExecuteWiqlQuery(client, organization, project, wiqlQuery4);

                // 处理查询结果
                Console.WriteLine("Query 1 Results:");
                //PrintWorkItems(query1Result);

                Console.WriteLine("\nQuery 2 Results:");
                //PrintWorkItems(query2Result);

                Console.WriteLine("\nQuery 3 Results:");
                //PrintWorkItems(query3Result);

                Console.WriteLine("\nQuery 4 Results:");
                //PrintWorkItems(query4Result);

                return new Dictionary<TaskQueryName, dynamic>() {
                    { TaskQueryName.CreatedTask, query1Result },
                    { TaskQueryName.ClosedTask, query2Result },
                    { TaskQueryName.ClosedBug, query3Result },
                    { TaskQueryName.NewOrActiveTask, query4Result }
                };
            }
        }

        private async void OnCounterClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string logMessage = "Button clicked!";
                logTextBlock.Text += logMessage + "\n";

                CounterBtn.IsEnabled = false;

                var isOk = await Check();

                if (isOk)
                {
                    SaveSetting();

                    await CreateReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                logTextBlock.Text += "Complte!!!";
                CounterBtn.IsEnabled = true;
            }
        }

        private void AddNewBox(object sender, RoutedEventArgs e)
        {
            UserControl1 newBoxView = new UserControl1();

            // 将新框添加到 mainStackLayout 中
            mainStackLayout.Children.Insert(mainStackLayout.Children.Count - 1, newBoxView);
        }

        private async Task<bool> Check()
        {
            //if (string.IsNullOrEmpty(MyTimePicker.Text))
            //{
            //    MessageBox.Show("Please select start time.");
            //    return false;
            //}
            foreach (var child in mainStackLayout.Children)
            {
                if (child is UserControl1 formBoxView)
                {
                    // 在这里进行输入验证，例如检查是否为空
                    bool isOrganizationEmpty = string.IsNullOrEmpty(formBoxView.OrganizationText.Trim());
                    bool isProjectEmpty = string.IsNullOrEmpty(formBoxView.ProjectText.Trim());
                    bool isTokenEmpty = string.IsNullOrEmpty(formBoxView.TokenText.Trim());

                    // 在这里可以执行其他验证逻辑

                    // 你可以根据需要处理验证结果，例如显示警告或执行其他操作
                    if (isOrganizationEmpty || isProjectEmpty || isTokenEmpty)
                    {
                        MessageBox.Show("Please fill in all fields.");
                        return false; // 如果有一个框验证失败，就停止遍历
                    }
                }
            }
            return true;
        }

        private void SaveSetting()
        {
            List<SettingModel> settingsList = new List<SettingModel>();
            foreach (var child in mainStackLayout.Children)
            {
                if (child is UserControl1 formBoxView)
                {
                    string organizationText = formBoxView.OrganizationText.Trim();
                    string projectText = formBoxView.ProjectText.Trim();
                    string tokenText = formBoxView.TokenText.Trim();

                    settingsList.Add(new SettingModel
                    {
                        OrganizationText = organizationText,
                        ProjectText = projectText,
                        TokenText = tokenText
                    });
                }
            }

            if (settingsList.Any())
            {
                // 创建JSON字符串
                string jsonContent = JsonConvert.SerializeObject(new { Setting = settingsList }, Formatting.Indented);
                string jsonFilePath = "settings.json";
                File.WriteAllText(jsonFilePath, jsonContent);
            }
        }
    }

    public enum TaskQueryName
    {
        CreatedTask,
        ClosedTask,
        ClosedBug,
        NewOrActiveTask
    }
}