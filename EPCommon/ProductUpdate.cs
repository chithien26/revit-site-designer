using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

/// <summary>
/// Compares the running add-in with GitHub Releases /latest.
/// Never kills Revit: save, download setup, wait for Revit to exit, then install and relaunch.
/// </summary>
internal static class ProductUpdate
{
	private const string DefaultRepo = "chithien26/revit-site-designer";
	private const int TimeoutMs = 8000;
	private static bool s_checkedThisSession;

	public static void Check(bool dailyOnly)
	{
		if (!TryGetReleaseVersion(out Version local))
			return;
		if (dailyOnly && s_checkedThisSession) return;
		if (dailyOnly && !NotificationsEnabled()) return;
		s_checkedThisSession = true;

		ReleaseInfo latest = FetchLatest();
		if (latest == null)
		{
			if (!dailyOnly)
				RevitMessageBox.Show("Không kiểm tra được phiên bản trên GitHub. Kiểm tra mạng rồi thử lại.", "Site Designer", System.Windows.Forms.MessageBoxIcon.Exclamation);
			return;
		}

		if (latest.Version <= local)
		{
			if (!dailyOnly)
				RevitMessageBox.Show("Bạn đang dùng bản mới nhất (" + local + ").", "Site Designer", System.Windows.Forms.MessageBoxIcon.None);
			return;
		}

		Prompt(latest, local);
	}

	private static bool NotificationsEnabled()
	{
		int disabled = 0;
		if (ApplicationUtility.Settings != null)
			ApplicationUtility.Settings.eplSetE_GetSetting("CheckForUpdateNotificationDoNotShowAgainOption", ref disabled);
		return disabled == 0;
	}

	private static bool TryGetReleaseVersion(out Version version)
	{
		version = new Version(0, 0, 0);
		string text = typeof(ProductUpdate).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		return TryParseVersion(text, out version);
	}

	private static void Prompt(ReleaseInfo latest, Version local)
	{
		TaskDialog dialog = new TaskDialog("Site Designer");
		dialog.TitleAutoPrefix = false;
		dialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
		dialog.MainInstruction = "Đã có phiên bản mới " + latest.Version;
		dialog.MainContent = "Bản đang dùng: " + local + "\nBản trên GitHub: " + latest.Version
			+ (string.IsNullOrEmpty(latest.Name) ? "" : " (" + latest.Name + ")")
			+ "\n\nCài đặt cần đóng Revit vì file add-in đang bị khóa.";
		dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
			"Lưu dự án, tắt Revit và cập nhật",
			"Lưu file đang mở, tải bộ cài. Sau khi bạn đóng Revit, trình cài chạy rồi mở Revit lại.");
		dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
			"Tiếp tục sử dụng, cập nhật sau",
			"Giữ phiên làm việc hiện tại. Có thể kiểm tra lại ở About.");
		dialog.CommonButtons = TaskDialogCommonButtons.None;
		dialog.AllowCancellation = true;
		TaskDialogResult result = dialog.Show();
		if (result == TaskDialogResult.CommandLink1)
			BeginUpdate(latest);
	}

	private static void BeginUpdate(ReleaseInfo latest)
	{
		UIApplication uiApp = RevitCommandRuntime.Current;
		string documentPath = TrySaveOpenDocuments(uiApp);
		if (string.IsNullOrEmpty(latest.SetupUrl))
		{
			OpenUrl(latest.HtmlUrl);
			RevitMessageBox.Show("Không tìm thấy file cài SiteDesigner-" + ApplicationUtility.RevitVersion
				+ "-Setup.exe trong release. Đã mở trang GitHub.", "Site Designer", System.Windows.Forms.MessageBoxIcon.Exclamation);
			return;
		}

		string setupPath;
		try
		{
			setupPath = DownloadSetup(latest.SetupUrl);
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("Update download failed: " + ex.Message);
			OpenUrl(latest.HtmlUrl);
			RevitMessageBox.Show("Không tải được bộ cài. Đã mở trang GitHub để tải tay.\n\n" + ex.Message,
				"Site Designer", System.Windows.Forms.MessageBoxIcon.Exclamation);
			return;
		}

		string revitExe = ProcessPath();
		WriteAndStartHelper(setupPath, revitExe, documentPath);
		RevitMessageBox.Show(
			"Đã tải bản " + latest.Version + ".\n\nHãy đóng Revit. Trình cài sẽ chạy khi Revit tắt, sau đó mở lại dự án.",
			"Site Designer", System.Windows.Forms.MessageBoxIcon.None);
	}

	private static string TrySaveOpenDocuments(UIApplication uiApp)
	{
		if (uiApp?.Application == null) return null;
		string activePath = uiApp.ActiveUIDocument?.Document?.PathName;
		int unsaved = 0;
		foreach (Document document in uiApp.Application.Documents)
		{
			if (document == null || !document.IsModified) continue;
			if (string.IsNullOrEmpty(document.PathName) || document.IsReadOnly)
			{
				unsaved++;
				continue;
			}
			try
			{
				document.Save();
			}
			catch (Exception ex)
			{
				unsaved++;
				RuntimeDiagnostics.Write("Update save failed: " + document.PathName + " " + ex.Message);
			}
		}
		if (unsaved > 0)
			RevitMessageBox.Show("Có " + unsaved + " tài liệu chưa lưu được (file mới chưa đặt tên, workshared, hoặc read-only). Hãy Save As trước khi đóng Revit.",
				"Site Designer", System.Windows.Forms.MessageBoxIcon.Exclamation);
		return activePath;
	}

	private static void WriteAndStartHelper(string setupPath, string revitExe, string documentPath)
	{
		int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
		string helper = Path.Combine(Path.GetTempPath(), "SiteDesigner-update.cmd");
		StringBuilder script = new StringBuilder();
		script.AppendLine("@echo off");
		script.AppendLine("setlocal");
		script.AppendLine(":wait");
		script.AppendLine("tasklist /FI \"PID eq " + pid + "\" | findstr /I \"" + pid + "\" >nul");
		script.AppendLine("if not errorlevel 1 (");
		script.AppendLine("  timeout /t 2 /nobreak >nul");
		script.AppendLine("  goto wait");
		script.AppendLine(")");
		script.AppendLine("timeout /t 2 /nobreak >nul");
		script.AppendLine("start /wait \"\" \"" + setupPath + "\" /VERYSILENT /NORESTART /SUPPRESSMSGBOXES");
		if (!string.IsNullOrEmpty(revitExe) && File.Exists(revitExe))
		{
			if (!string.IsNullOrEmpty(documentPath) && File.Exists(documentPath))
				script.AppendLine("start \"\" \"" + revitExe + "\" \"" + documentPath + "\"");
			else
				script.AppendLine("start \"\" \"" + revitExe + "\"");
		}
		File.WriteAllText(helper, script.ToString(), Encoding.Default);
		System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
		{
			FileName = helper,
			UseShellExecute = true,
			WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
			CreateNoWindow = true
		});
		RuntimeDiagnostics.Write("Update helper started; setup=" + setupPath + " pid=" + pid);
	}

	private static string ProcessPath()
	{
		try { return System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName; }
		catch (Exception) { return null; }
	}

	private static ReleaseInfo FetchLatest()
	{
		string repo = DefaultRepo;
		string configured = string.Empty;
		if (ApplicationUtility.Settings != null)
			ApplicationUtility.Settings.eplSetE_GetSetting("UpdateGitHubRepo", ref configured);
		if (!string.IsNullOrWhiteSpace(configured))
			repo = configured.Trim();

		EnableTls12();
		string json = DownloadString("https://api.github.com/repos/" + repo + "/releases/latest");
		if (string.IsNullOrEmpty(json)) return null;

		string tag = JsonString(json, "tag_name");
		Version version;
		if (!TryParseVersion(tag, out version)) return null;

		string year = ApplicationUtility.RevitVersion ?? "2025";
		string setupUrl = FindSetupAssetUrl(json, "SiteDesigner-" + year + "-Setup");
		string html = JsonString(json, "html_url");
		string name = JsonString(json, "name");
		return new ReleaseInfo
		{
			Version = version,
			Name = name,
			HtmlUrl = string.IsNullOrEmpty(html) ? "https://github.com/" + repo + "/releases/latest" : html,
			SetupUrl = setupUrl
		};
	}

	private static string DownloadSetup(string url)
	{
		string dest = Path.Combine(Path.GetTempPath(), "SiteDesigner-" + (ApplicationUtility.RevitVersion ?? "2025") + "-Setup.exe");
		DownloadToFile(url, dest);
		return dest;
	}

	private static string DownloadString(string url)
	{
		HttpWebRequest request = CreateRequest(url);
		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
		using (Stream stream = response.GetResponseStream())
		using (StreamReader reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
			return reader.ReadToEnd();
	}

	private static void DownloadToFile(string url, string path)
	{
		HttpWebRequest request = CreateRequest(url);
		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
		using (Stream input = response.GetResponseStream())
		using (FileStream output = File.Create(path))
		{
			if (input != null) input.CopyTo(output);
		}
	}

	private static HttpWebRequest CreateRequest(string url)
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.Method = "GET";
		request.UserAgent = "SiteDesigner";
		request.Accept = "application/vnd.github+json";
		request.Timeout = TimeoutMs;
		request.ReadWriteTimeout = TimeoutMs;
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		request.AllowAutoRedirect = true;
		return request;
	}

	private static void EnableTls12()
	{
		try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; }
		catch (Exception) { }
	}

	private static void OpenUrl(string url)
	{
		if (string.IsNullOrEmpty(url)) return;
		try
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("OpenUrl failed: " + ex.Message);
		}
	}

	private static bool TryParseVersion(string text, out Version version)
	{
		version = new Version(0, 0, 0);
		if (string.IsNullOrWhiteSpace(text)) return false;
		text = text.Trim();
		if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase))
			text = text.Substring(1);
		int plus = text.IndexOf('+');
		if (plus >= 0) text = text.Substring(0, plus);
		int dash = text.IndexOf('-');
		if (dash >= 0) text = text.Substring(0, dash);
		return Version.TryParse(text, out version);
	}

	private static string JsonString(string json, string key)
	{
		if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
		string needle = "\"" + key + "\"";
		int keyAt = json.IndexOf(needle, StringComparison.Ordinal);
		if (keyAt < 0) return null;
		int colon = json.IndexOf(':', keyAt + needle.Length);
		if (colon < 0) return null;
		int q1 = json.IndexOf('"', colon + 1);
		if (q1 < 0) return null;
		int q2 = q1 + 1;
		StringBuilder value = new StringBuilder();
		while (q2 < json.Length)
		{
			char c = json[q2++];
			if (c == '\\' && q2 < json.Length)
			{
				value.Append(json[q2++]);
				continue;
			}
			if (c == '"') return value.ToString();
			value.Append(c);
		}
		return null;
	}

	private static string FindSetupAssetUrl(string json, string namePrefix)
	{
		if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(namePrefix)) return null;
		string fallback = null;
		int from = 0;
		while (from < json.Length)
		{
			int keyAt = json.IndexOf("\"browser_download_url\"", from, StringComparison.Ordinal);
			if (keyAt < 0) break;
			int colon = json.IndexOf(':', keyAt);
			int q1 = json.IndexOf('"', colon + 1);
			int q2 = q1 < 0 ? -1 : json.IndexOf('"', q1 + 1);
			if (q1 < 0 || q2 < 0) break;
			string url = json.Substring(q1 + 1, q2 - q1 - 1).Replace("\\/", "/");
			from = q2 + 1;
			if (url.IndexOf(namePrefix, StringComparison.OrdinalIgnoreCase) < 0) continue;
			if (!url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
			if (url.IndexOf(namePrefix + "-", StringComparison.OrdinalIgnoreCase) >= 0)
				return url;
			if (fallback == null) fallback = url;
		}
		return fallback;
	}

	private sealed class ReleaseInfo
	{
		public Version Version;
		public string Name;
		public string HtmlUrl;
		public string SetupUrl;
	}
}
