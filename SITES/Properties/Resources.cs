using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SITES.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("SITES.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap AboutBoxImage
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "AboutBoxImage", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap AutodeskLogo
	{
		get
		{
			object obj = EPCommon.SafeResources.GetObject(ResourceManager, "AutodeskLogo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
