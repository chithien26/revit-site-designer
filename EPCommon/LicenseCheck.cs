using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Autodesk.Revit.UI;

namespace EPCommon;

/// <summary>
/// Checks add-in license against Addin Store using the Autodesk/Revit login identity.
/// </summary>
internal static class LicenseCheck
{
	private const string CheckUrl = "https://addin-store.onrender.com/api/licenses/check";
	private const int TimeoutMs = 20000;
	private static bool s_allowed;
	private static DateTime s_allowedUntilUtc;

	public static bool EnsureAllowed(UIApplication uiApp)
	{
		if (s_allowed && DateTime.UtcNow < s_allowedUntilUtc)
			return true;

		string username;
		string email;
		ResolveIdentity(uiApp, out username, out email);
		RuntimeDiagnostics.Write("License check identity username=" + (username ?? "") + "; email=" + (email ?? ""));
		if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email))
		{
			Deny("Không lấy được tài khoản đang đăng nhập trong Revit. Liên hệ admin để được cấp.");
			return false;
		}

		try
		{
			CheckResult result = PostCheck(username, email);
			if (result.Valid)
			{
				s_allowed = true;
				s_allowedUntilUtc = result.ExpiresAtUtc ?? DateTime.UtcNow.AddHours(12);
				if (s_allowedUntilUtc < DateTime.UtcNow.AddMinutes(5))
					s_allowedUntilUtc = DateTime.UtcNow.AddMinutes(5);
				RuntimeDiagnostics.Write("License valid until " + s_allowedUntilUtc.ToString("u") + ".");
				return true;
			}
		}
		catch (Exception ex)
		{
			RuntimeDiagnostics.Write("License check failed: " + ex.Message);
			Deny("Không kiểm tra được license. Kiểm tra mạng hoặc liên hệ admin để được cấp.");
			return false;
		}

		string account = !string.IsNullOrWhiteSpace(email) ? email : username;
		Deny("Chưa có license. Liên hệ admin để được cấp.\n\nTài khoản: " + account);
		return false;
	}

	private static void Deny(string message)
	{
		s_allowed = false;
		TaskDialog dialog = new TaskDialog("Site Designer");
		dialog.TitleAutoPrefix = false;
		dialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
		dialog.MainInstruction = "Chưa có license";
		dialog.MainContent = message;
		dialog.CommonButtons = TaskDialogCommonButtons.Close;
		dialog.Show();
	}

	private static void ResolveIdentity(UIApplication uiApp, out string username, out string email)
	{
		username = uiApp?.Application?.Username;
		email = ReadAutodeskLoginEmail();
		if (string.IsNullOrWhiteSpace(email) && LooksLikeEmail(username))
			email = username;
	}

	private static bool LooksLikeEmail(string value)
	{
		return !string.IsNullOrWhiteSpace(value) && value.IndexOf('@') > 0;
	}

	private static string ReadAutodeskLoginEmail()
	{
		string[] folders =
		{
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Autodesk", "Web Services"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "Web Services")
		};
		foreach (string folder in folders)
		{
			string path = Path.Combine(folder, "LoginState.xml");
			if (!File.Exists(path)) continue;
			try
			{
				XDocument xml = XDocument.Load(path);
				foreach (XElement node in xml.Descendants())
				{
					string value = node.Value?.Trim();
					if (LooksLikeEmail(value) && value.IndexOf('\n') < 0)
						return value;
				}
			}
			catch (Exception ex)
			{
				RuntimeDiagnostics.Write("Could not read Autodesk login: " + ex.Message);
			}
		}
		return null;
	}

	private static CheckResult PostCheck(string username, string email)
	{
		EnableTls12();
		string body = "{\"username\":" + JsonEscape(username) + ",\"email\":" + JsonEscape(email) + "}";
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CheckUrl);
		request.Method = "POST";
		request.ContentType = "application/json; charset=utf-8";
		request.Accept = "application/json";
		request.UserAgent = "SiteDesigner";
		request.Timeout = TimeoutMs;
		request.ReadWriteTimeout = TimeoutMs;
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		byte[] payload = Encoding.UTF8.GetBytes(body);
		request.ContentLength = payload.Length;
		using (Stream output = request.GetRequestStream())
			output.Write(payload, 0, payload.Length);

		try
		{
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				return ReadResult(response);
		}
		catch (WebException ex)
		{
			using (HttpWebResponse error = ex.Response as HttpWebResponse)
			{
				if (error != null)
					return ReadResult(error);
			}
			throw;
		}
	}

	private static CheckResult ReadResult(HttpWebResponse response)
	{
		using (Stream stream = response.GetResponseStream())
		using (StreamReader reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
		{
			string json = reader.ReadToEnd();
			int code = (int)response.StatusCode;
			if (code < 200 || code >= 300)
				throw new InvalidOperationException("License server HTTP " + code);
			return Parse(json);
		}
	}

	private static CheckResult Parse(string json)
	{
		CheckResult result = new CheckResult();
		if (string.IsNullOrEmpty(json)) return result;
		string compact = json.Replace(" ", "");
		result.Valid = compact.IndexOf("\"valid\":true", StringComparison.OrdinalIgnoreCase) >= 0;
		string expires = JsonString(json, "expiresAt");
		if (!string.IsNullOrEmpty(expires) && DateTime.TryParse(expires, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime when))
			result.ExpiresAtUtc = when.ToUniversalTime();
		return result;
	}

	private static string JsonEscape(string value)
	{
		if (string.IsNullOrEmpty(value)) return "null";
		StringBuilder text = new StringBuilder("\"");
		foreach (char c in value)
		{
			if (c == '\\' || c == '"') text.Append('\\');
			if (c == '\r' || c == '\n' || c == '\t') continue;
			text.Append(c);
		}
		text.Append('"');
		return text.ToString();
	}

	private static string JsonString(string json, string key)
	{
		string needle = "\"" + key + "\"";
		int keyAt = json.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
		if (keyAt < 0) return null;
		int colon = json.IndexOf(':', keyAt + needle.Length);
		if (colon < 0) return null;
		int i = colon + 1;
		while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
		if (i >= json.Length || json[i] == 'n') return null;
		if (json[i] != '"') return null;
		int q1 = i;
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

	private static void EnableTls12()
	{
		try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; }
		catch (Exception) { }
	}

	private sealed class CheckResult
	{
		public bool Valid;
		public DateTime? ExpiresAtUtc;
	}
}
