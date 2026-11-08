using System.Runtime.InteropServices;

namespace InputSim;

/// <summary>
/// 本地联调工具：模拟「按住 Ctrl+Alt，左键拖动」。
/// 用来验证 GhostNote 的全局鼠标钩子是否真的在工作。
/// 用法：dotnet run --project tools/InputSim -- [起始X] [起始Y] [位移X] [位移Y]
/// </summary>
internal static class Program
{
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private const uint MouseEventMove = 0x0001;
    private const uint KeyEventKeyUp = 0x0002;

    private const byte VkControl = 0x11;
    private const byte VkMenu = 0x12;

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

    private static void Main(string[] args)
    {
        var startX = args.Length > 0 ? int.Parse(args[0]) : 400;
        var startY = args.Length > 1 ? int.Parse(args[1]) : 400;
        var deltaX = args.Length > 2 ? int.Parse(args[2]) : 240;
        var deltaY = args.Length > 3 ? int.Parse(args[3]) : 120;

        Console.WriteLine($"移动到 {startX},{startY} 并按 Ctrl+Alt 拖动 {deltaX},{deltaY}");

        SetCursorPos(startX, startY);
        Thread.Sleep(200);

        keybd_event(VkControl, 0, 0, IntPtr.Zero);
        keybd_event(VkMenu, 0, 0, IntPtr.Zero);
        Thread.Sleep(120);

        mouse_event(MouseEventLeftDown, 0, 0, 0, IntPtr.Zero);
        Thread.Sleep(150);

        var steps = 12;
        for (var i = 1; i <= steps; i++)
        {
            mouse_event(MouseEventMove, deltaX / steps, deltaY / steps, 0, IntPtr.Zero);
            Thread.Sleep(40);
        }

        Thread.Sleep(120);
        mouse_event(MouseEventLeftUp, 0, 0, 0, IntPtr.Zero);
        Thread.Sleep(80);

        keybd_event(VkMenu, 0, KeyEventKeyUp, IntPtr.Zero);
        keybd_event(VkControl, 0, KeyEventKeyUp, IntPtr.Zero);

        Console.WriteLine("完成");
    }
}
