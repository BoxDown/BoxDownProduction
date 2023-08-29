using System;
using System.Collections.Generic;
using System.IO;

namespace Utility
{
    static class TextDocumentReadWrite
    {        
        static public string[] FileRead(string fileLocation)
        {
            List<string> document = new List<string>();

            StreamReader streamReader = new StreamReader(fileLocation);
            while (streamReader.EndOfStream == false)
            {
                string line = streamReader.ReadLine();
                line.Trim();
                document.Add(line);
            }
            streamReader.Close();
            return document.ToArray();
        }
        static public bool FileWrite(string fileLocation, string[] newTextDoc)
        {
            StreamWriter streamWriter = new StreamWriter(fileLocation);
            for(int i = 0; i < newTextDoc.Length; i++)
            {
                streamWriter.WriteLine(newTextDoc[i]);
            }
            streamWriter.Close();
            return true;
        }
    }
}
