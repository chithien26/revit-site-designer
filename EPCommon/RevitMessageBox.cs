using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogIcon = Autodesk.Revit.UI.TaskDialogIcon;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace EPCommon;

public class RevitMessageBox
{
	public static DialogResult Show(string _strText, string _strCaption)
	{
		return Show(_strText, _strCaption, MessageBoxIcon.None);
	}

	public static DialogResult Show(string _strText, string _strCaption, MessageBoxIcon _Icon)
	{
		return Show(_strText, _strCaption, MessageBoxButtons.OK, _Icon);
	}

	public static DialogResult Show(string _strText, string _strCaption, MessageBoxButtons _Buttons, MessageBoxIcon _Icon)
	{
		_strText = TerrainText.ForDisplay(_strText);
		_strCaption = TerrainText.ForDisplay(_strCaption);
		TaskDialogCommonButtons commonButtons = TaskDialogCommonButtons.None;
		TaskDialogResult defaultButton = TaskDialogResult.None;
		switch (_Buttons)
		{
		case MessageBoxButtons.OK:
			defaultButton = TaskDialogResult.Ok;
			commonButtons = TaskDialogCommonButtons.Ok;
			break;
		case MessageBoxButtons.OKCancel:
			defaultButton = TaskDialogResult.Ok;
			commonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
			break;
		case MessageBoxButtons.YesNo:
			defaultButton = TaskDialogResult.Yes;
			commonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
			break;
		case MessageBoxButtons.YesNoCancel:
			defaultButton = TaskDialogResult.Yes;
			commonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel;
			break;
		case MessageBoxButtons.RetryCancel:
			defaultButton = TaskDialogResult.Retry;
			commonButtons = TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Retry;
			break;
		case MessageBoxButtons.AbortRetryIgnore:
			return MessageBoxShow(_strText, _strCaption, _Buttons, _Icon);
		}
		TaskDialogIcon mainIcon = TaskDialogIcon.TaskDialogIconNone;
		if (_Icon == MessageBoxIcon.Hand || _Icon == MessageBoxIcon.Exclamation || _Icon == MessageBoxIcon.Asterisk)
		{
			mainIcon = TaskDialogIcon.TaskDialogIconWarning;
		}
		TaskDialog taskDialog = new TaskDialog(_strCaption);
		taskDialog.MainIcon = mainIcon;
		taskDialog.MainInstruction = _strText;
		taskDialog.AllowCancellation = true;
		taskDialog.TitleAutoPrefix = false;
		taskDialog.CommonButtons = commonButtons;
		taskDialog.DefaultButton = defaultButton;
		TaskDialogResult taskDialogResult = taskDialog.Show();
		DialogResult result = DialogResult.None;
		switch (taskDialogResult)
		{
		case TaskDialogResult.Yes:
			result = DialogResult.Yes;
			break;
		case TaskDialogResult.No:
			result = DialogResult.No;
			break;
		case TaskDialogResult.Cancel:
			result = DialogResult.Cancel;
			break;
		case TaskDialogResult.Retry:
			result = DialogResult.Retry;
			break;
		case TaskDialogResult.Ok:
			result = DialogResult.OK;
			break;
		}
		return result;
	}

	private static DialogResult MessageBoxShow(string _strText, string _strCaption, MessageBoxButtons _Buttons, MessageBoxIcon _Icon)
	{
		if (ApplicationUtility.RevitWindowWrapper != null)
		{
			return MessageBox.Show(ApplicationUtility.RevitWindowWrapper, _strText, _strCaption, _Buttons, _Icon);
		}
		return MessageBox.Show(_strText, _strCaption, _Buttons, _Icon);
	}
}
