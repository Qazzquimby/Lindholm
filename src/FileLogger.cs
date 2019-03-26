using System;
using System.IO;

public class FileLogger
{
    public static string FilePath = @"Log.txt";
    public static void Log(string message)
    {


        using (StreamWriter streamWriter = File.AppendText(FilePath))
        {
            DateTime now = DateTime.Now;

            streamWriter.WriteLine(now);
            streamWriter.WriteLine(message);
            streamWriter.WriteLine("\n\n");

            streamWriter.Close();
        }
    }
}