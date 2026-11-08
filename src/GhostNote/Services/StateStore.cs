using System;
using System.IO;
using System.Text.Json;
using GhostNote.Models;

namespace GhostNote.Services;

/// <summary>
/// 把状态存到 %AppData%\GhostNote\state.json。
/// </summary>
public static class StateStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GhostNote",
        "state.json");

    public static NoteState Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new NoteState();
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<NoteState>(json, Options) ?? new NoteState();
        }
        catch
        {
            // 配置损坏不应该阻止程序启动，直接回到默认状态。
            return new NoteState();
        }
    }

    public static void Save(NoteState state)
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);

            // 先写临时文件再替换，避免写一半断电导致 JSON 损坏。
            var temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(state, Options));
            File.Move(temp, FilePath, overwrite: true);
        }
        catch
        {
            // 保存失败（磁盘只读等）不弹窗打扰用户。
        }
    }
}
