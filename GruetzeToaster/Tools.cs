using System.Reflection;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace GruetzeToaster;

public static class Tools
{
    public static string GetVersion()
    {
        // 1. Titelleiste aus Assembly-Infos
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    }

    public static string GetAppTitle()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var title = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "GruetzeToaster";
        var version = GetVersion();
        var author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Grütze-Software";
        return $"{title} v{version} by {author}";
    }

    public static string GetExcMsg(Exception ex)
    {
        string msg = ex.Message;
        if (ex.InnerException != null && ex.InnerException.Message != msg)
            msg += "\r\n" + GetExcMsg(ex.InnerException);
        return msg;
    }

    public static int CalculatePercentage(int current, int total)
    {
        if (total == 0)
        {
            return 0;
        }

        // Vermeide Ganzzahldivision durch die Multiplikation mit 100.0 (einem double)
        return (int)((current / (double)total) * 100);
    }

    public static string GetOSName()
    {
        string os = Environment.OSVersion.ToString();
        string arch = RuntimeInformation.OSArchitecture.ToString();
        return $"{os} {arch}";
    }
}
