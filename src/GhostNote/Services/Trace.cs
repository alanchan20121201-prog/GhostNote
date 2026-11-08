using System;
using System.IO;

namespace GhostNote.Services;

/// <summary>
/// 轻量诊断日志。只有设置环境变量 GHOSTNOTE_TRACE=1 时才写入，
/// 平时对性能零影响（enable 判断在静态构造时完成）。
/// 日志路径：%TEMP%\ghostnote-trace.log
/// </summary>
internal static class Trace
{
    private static readonly bool Enabled =
        string.Equals(Environment.GetEnvironmentVariable("GHOSTNOTE_TRACE"), "1", StringComparison.Ordinal);

    private static readonly string LogPath =
        Path.Combine(Path.GetTempPath(), "ghostnote-trace.log");

    public static void Log(string message)
    {
        if (!Enabled)
        {
            return;
        }

        try
        {
            File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff}  {message}{Environment.NewLine}");
        }
        catch
        {
            // 诊断日志失败不能影响主流程
        }
    }
}
