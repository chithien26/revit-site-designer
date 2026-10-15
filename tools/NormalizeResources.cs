using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using System.Xml;

// Run once with .NET Framework against the backed-up, trusted 2020 resources.
class NormalizeResources
{
    static int Main(string[] args)
    {
        int count = 0;
        foreach (string folder in new[] { "EPCommon", "SITES" })
        foreach (string path in Directory.GetFiles(folder, "*.resx", SearchOption.AllDirectories))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            using (ResXResourceReader reader = new ResXResourceReader(path))
            {
                reader.UseResXDataNodes = true;
                foreach (DictionaryEntry entry in reader)
                {
                    ResXDataNode node = (ResXDataNode)entry.Value;
                    XmlElement element = doc.SelectSingleNode("/root/data[@name=\"" + node.Name + "\"]") as XmlElement;
                    if (element == null || element.GetAttribute("mimetype") != "application/x-microsoft.net.object.binary.base64") continue;
                    object value = node.GetValue((System.ComponentModel.Design.ITypeResolutionService)null);
                    byte[] bytes = null;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Image img = value as Image;
                        Icon icon = value as Icon;
                        if (img != null) img.Save(stream, ImageFormat.Png);
                        else if (icon != null) icon.Save(stream);
                        else { element.ParentNode.RemoveChild(element); continue; }
                        bytes = stream.ToArray();
                    }
                    element.RemoveAttribute("mimetype");
                    element.SetAttribute("type", "System.Byte[], mscorlib");
                    element.SelectSingleNode("value").InnerText = Convert.ToBase64String(bytes);
                    count++;
                }
            }
            foreach (XmlNode metadata in doc.SelectNodes("/root/metadata")) metadata.ParentNode.RemoveChild(metadata);
            doc.Save(path);
        }
        Console.WriteLine("Converted " + count + " serialized images to raw PNG/ICO bytes.");
        return 0;
    }
}
