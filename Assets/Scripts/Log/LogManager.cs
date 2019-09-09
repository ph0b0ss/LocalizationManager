using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;

public static class LogManager {

    public enum LevelType { Info = 0, Warning = 1, Error = 2};

    public static int TEXT_SIZE = 12;
    public static int HEADER_SIZE = 16;

    public static void Log(string s)
    {
        UnityEngine.Debug.Log(s);
    }
    public static void Log(string s, LevelType l)
    {
        StackTrace trace = new StackTrace(true);
        string callingClass = Path.GetFileNameWithoutExtension(trace.GetFrame(1).GetFileName());
        if (l == LevelType.Warning)
        {
            s = "#"+ callingClass + "#<color=orange><size="+ HEADER_SIZE + "><b>["+ callingClass + "]</b></size><size="+TEXT_SIZE+">" + s + "</size></color>";
        }
        else if(l == LevelType.Error)
        {
            s = "#"+ callingClass + "#<color=red><size=" + HEADER_SIZE + "><b>[" + callingClass + "]</b></size><size=" + TEXT_SIZE + ">" + s + "</size></color>";
        }
        Log(s);
    }
    public static void Log(string header, string content, LevelType l)
    {
        header = "<size="+ HEADER_SIZE + "><b>" + header + " : </b></size>";
        string s = string.Empty;
        StackTrace trace = new StackTrace(true);
        string callingClass = Path.GetFileNameWithoutExtension(trace.GetFrame(1).GetFileName());

        if (l == LevelType.Info)
        {
            s = "#" + callingClass + "#<color=white>" + header + "<size=" + TEXT_SIZE + ">" + content + "</size></color>";
        }
        else if (l == LevelType.Warning)
        {
            s = "#"+ callingClass + "#<color=orange>" + header + "<size="+TEXT_SIZE+">" +content + "</size></color>";
        }
        else if (l == LevelType.Error)
        {
            s = "#"+ callingClass + "#<color=red>" + header + "<size=" + TEXT_SIZE + ">" + content + "</size></color>";
        }
        Log(s);
    }

}
