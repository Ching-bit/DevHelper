using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace Framework.Utils.Helpers;

public static class ObjectHelper
{
    public static T FromXmlDir<T>(string xmlDir)
    {
        Type type = typeof(T);
        string xmlPath = Path.Combine(xmlDir, $"{type.Name}.xml");
        return FromXmlFile<T>(xmlPath);
    }

    public static T FromXmlFile<T>(string xmlPath)
    {
        XmlDocument doc = new();
        doc.Load(xmlPath);
        if (null == doc.DocumentElement)
        {
            throw new Exception($"Parse error: {xmlPath}");
        }

        return FromXmlString<T>(doc.InnerXml);
    }

    public static T FromXmlString<T>(string xml)
    {
        Type type = typeof(T);
        XmlSerializer serializer = new(type);
        using StringReader xmlReader = new(xml);
        T? ret = (T?)serializer.Deserialize(xmlReader);
        if (null == ret)
        {
            throw new Exception($"Parse error: {xml}");
        }
        return ret;
    }

    public static bool ToXmlDir<T>(string xmlDir, T obj)
    {
        Type type = typeof(T);
        string xmlPath = Path.Combine(xmlDir, $"{type.Name}.xml");
        return ToXml(xmlPath, obj);
    }

    public static bool ToXml<T>(string xmlPath, T obj)
    {
        Type type = typeof(T);
        XmlSerializer serializer = new(type);

        try
        {
            using FileStream stream = new(xmlPath, FileMode.Create);
            XmlWriterSettings settings = new()
            {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = Environment.NewLine,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = true,
            };

            using XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
            XmlSerializerNamespaces namespaces = new();
            namespaces.Add(string.Empty, string.Empty);
            serializer.Serialize(xmlWriter, obj, namespaces);
            xmlWriter.Close();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string ToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static void Copy<T>(T dest, T src)
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            property.SetValue(dest, property.GetValue(src));
        }
    }

}
