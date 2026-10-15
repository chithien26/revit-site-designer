using System;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;

namespace EPCommon;

internal class SDTimer
{
	internal Stopwatch watch = Stopwatch.StartNew();

	internal double m_totalElapsedTime;

	internal string m_taskName;

	internal string m_keyword = "";

	internal string m_timeFinished;

	internal string m_revitBuildNumber;

	internal Application m_app;

	public string Result => $"{m_taskName},{m_keyword},{m_totalElapsedTime},{m_revitBuildNumber},{m_timeFinished}";

	public void Start()
	{
		watch.Reset();
		watch.Start();
	}

	public void Stop()
	{
		watch.Stop();
		m_totalElapsedTime = watch.Elapsed.TotalSeconds;
		m_timeFinished = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
		m_revitBuildNumber = $"{m_app.VersionBuild} {m_app.VersionNumber}";
		m_app.WriteJournalComment("[SiteDesigner]" + Result, timeStamp: false);
	}

	public double GetTotalElapsedTime()
	{
		return watch.Elapsed.TotalSeconds;
	}

	public string GetTimeFinished()
	{
		return DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
	}

	public SDTimer(string taskName, Application app)
	{
		m_taskName = taskName;
		m_app = app;
	}
}
