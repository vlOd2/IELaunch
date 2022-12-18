using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class IELaunch
{
	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(IntPtr hWnd);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr SetWindowLongPtrA(IntPtr hWnd, int nIndex, long dwNewLong);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
		int X, int Y, int cx, int cy, uint uFlags);
		
	[DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, 
		IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
	
	[DllImport("user32.dll")]
	public static extern int GetWindowTextA(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
	
	public enum WndStyle : long
	{
		WS_OVERLAPPED = 0x00000000L,
		WS_POPUP = 0x80000000L,
		WS_CHILD = 0x40000000L,
		WS_MINIMIZE = 0x20000000L,
		WS_VISIBLE = 0x10000000L,
		WS_DISABLED = 0x08000000L,
		WS_CLIPSIBLINGS = 0x04000000L,
		WS_CLIPCHILDREN = 0x02000000L,
		WS_MAXIMIZE = 0x01000000L,
		WS_CAPTION = 0x00C00000L,
		WS_BORDER = 0x00800000L,
		WS_DLGFRAME = 0x00400000L,
		WS_VSCROLL = 0x00200000L,
		WS_HSCROLL = 0x00100000L,
		WS_SYSMENU = 0x00080000L,
		WS_THICKFRAME = 0x00040000L,
		WS_GROUP = 0x00020000L,
		WS_TABSTOP = 0x00010000L,
		WS_MINIMIZEBOX = 0x00020000L,
		WS_MAXIMIZEBOX = 0x00010000L
	}
	
	public static IntPtr[] GetProcessWindows(int pid) {
		List<IntPtr> windows = new List<IntPtr>();
		IntPtr lastWindow = IntPtr.Zero;
		
		do
		{
			lastWindow = FindWindowEx(IntPtr.Zero, lastWindow, null, null);

			int lastWindowPID;
			GetWindowThreadProcessId(lastWindow, out lastWindowPID);

			if (lastWindowPID == pid)
				windows.Add(lastWindow);
		}
		while (lastWindow != IntPtr.Zero);
		
        return windows.ToArray();
    }
	
	public static void Main(string[] args)
	{
		Console.WriteLine("IELaunch - v1.0 - Written by vlOd");
		Console.WriteLine("Usage: IELaunch.exe [IEPATH]");
		Console.WriteLine("".PadRight(Console.BufferWidth, '-'));
		
		Process ie = new Process();
		ie.StartInfo.FileName = args.Length > 0 ? args[0] : @"C:\Program Files\Internet Explorer\iexplore.exe";
		ie.StartInfo.Arguments = "-embedding -noframemerging";
		
		try 
		{
			ie.Start();
			ie.WaitForInputIdle();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Unable to start Internet Explorer: " + ex.Message);
			return;
		}
		Console.WriteLine("Launched Internet Explorer (" + ie.StartInfo.FileName + ")");

		int pid = ie.Id;
		Console.WriteLine("Process ID: " + pid);
		
		IntPtr[] windows = GetProcessWindows(pid);
		IntPtr mainWindow = IntPtr.Zero;
		Console.WriteLine("Process Windows Count: " + windows.Length);
		
		foreach (IntPtr window in windows)
		{
			StringBuilder windowTitle = new StringBuilder(128);
			GetWindowTextA(window, windowTitle, 128);
			Console.WriteLine("Process Window: " + window + " (" + windowTitle.ToString() + ")");
			
			if (windowTitle.ToString().Contains("Internet Explorer"))
			{
				mainWindow = window;
			}
		}
		
		StringBuilder mainWindowTitle = new StringBuilder(128);
		GetWindowTextA(mainWindow, mainWindowTitle, 128);
		Console.WriteLine("Process Main Window: " + mainWindow + " (" + mainWindowTitle.ToString() + ")");
		Console.WriteLine("".PadRight(Console.BufferWidth, '-'));
		
		IntPtr setWindowLongResult = SetWindowLongPtrA(mainWindow, -16, (long)(WndStyle.WS_CAPTION | WndStyle.WS_SYSMENU | 
			WndStyle.WS_THICKFRAME | WndStyle.WS_MINIMIZEBOX | WndStyle.WS_MAXIMIZEBOX | WndStyle.WS_VISIBLE));
		int setWindowLongError = Marshal.GetLastWin32Error();
		bool setWindowPosResult = SetWindowPos(mainWindow, IntPtr.Zero, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0004 | 0x0020);
		int setWindowPosError = Marshal.GetLastWin32Error();

		Console.WriteLine("CALL -> SetWindowLongPtrA(): " + setWindowLongResult);
		Console.WriteLine("CALL -> SetWindowLongPtrA()::GetLastError(): " + setWindowLongError);
		Console.WriteLine("CALL -> SetWindowPos(): " + setWindowPosResult);
		Console.WriteLine("CALL -> SetWindowPos()::GetLastError(): " + setWindowPosError);
		Console.WriteLine("".PadRight(Console.BufferWidth, '-'));
		Console.WriteLine("Process Main Window Visible: " + IsWindowVisible(mainWindow));
		
		ie.WaitForExit();
		Console.WriteLine("Exited: " + ie.ExitCode);
	}
}