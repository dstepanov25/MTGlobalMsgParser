using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConsoleApplication1
{
  class Program
  {
    static void Main(string[] args)
    {
      StreamReader streamReader = new StreamReader(@"c:\output\Debug\Include\mtglobal_msg.h");

      for (int i = 0; i < 98; i++)
        streamReader.ReadLine();

      var listOfValues = new Dictionary<int, string>();
      while (!streamReader.EndOfStream)
      {
        streamReader.ReadLine(); // //
        streamReader.ReadLine(); // // MessageId: CORE_ERR_LOG_MSG
        streamReader.ReadLine(); // //
        streamReader.ReadLine(); // // MessageText:
        streamReader.ReadLine(); // //
        var message = GetMessage(streamReader);
        message = message.Substring(0, message.LastIndexOf("\r\n"));
        var id = ReadId(streamReader);
        streamReader.ReadLine(); // 
        if (id == 0) continue;

        listOfValues.Add(id, message);
      }

      var xmlDoc = new XmlDocument();
      xmlDoc.LoadXml("<root></root>");

      XmlElement root = xmlDoc.DocumentElement;
      foreach (var keyValuePair in listOfValues)
      {
        XmlAttribute nameAttr = xmlDoc.CreateAttribute("name");
        XmlAttribute spaceAttr = xmlDoc.CreateAttribute("xml:space");
        spaceAttr.Value = "preserve";
        XmlNode newElem = xmlDoc.CreateNode("element", "data", "");

        nameAttr.Value = "SQL_ERROR_" + keyValuePair.Key;
        newElem.Attributes.Append(nameAttr);
        newElem.Attributes.Append(spaceAttr);

        XmlNode valueElem = xmlDoc.CreateNode("element", "value", "");
        valueElem.InnerText = keyValuePair.Value;

        newElem.AppendChild(valueElem);

        root.AppendChild(newElem);
      }
    }

    private static int ReadId(StreamReader fs)
    {
      var idStr = fs.ReadLine();
      if (idStr.StartsWith("#define "))
        idStr = idStr.Replace("#define ", "");
      else
        return 0;

      var id1 = idStr.IndexOf("((DWORD)0x", StringComparison.Ordinal);
      idStr = idStr.Substring(id1 + 10);
      var id2 = idStr.IndexOf("L)", StringComparison.Ordinal);
      idStr = idStr.Substring(0, id2);
      return Convert.ToInt32(idStr, 16);
    }

    private static string GetMessage(StreamReader fs)
    {
      var tmpStr = fs.ReadLine();
      if (tmpStr == "//")
        return "";
      var str = tmpStr.Replace("// ", "") + "\r\n";
      str += GetMessage(fs);
      return str;
    }
  }
}
