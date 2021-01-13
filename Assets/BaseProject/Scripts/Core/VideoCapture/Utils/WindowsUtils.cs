using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CreativeMode;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class WindowsUtils
{
    private const uint GW_OWNER = 4;
    
    [DllImport("user32.dll")]  
    private static extern void DisableProcessWindowsGhosting();
    
    [DllImport("user32.dll")]  
    private static extern IntPtr GetForegroundWindow();  
    
    [DllImport("user32.dll")]  
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    
    [DllImport("user32.dll")]  
    [return: MarshalAs(UnmanagedType.Bool)]  
    private static extern bool GetWindowRect(IntPtr hWnd, out WindowsRect lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

    private static Dictionary<int, Process> processCache = new Dictionary<int, Process>();

    public static void DisableWindowGhosting()
    {
        if (!Application.isEditor)
        {
            DisableProcessWindowsGhosting();
        }
    }

    public static Process GetProcessById(int id)
    {
        if (processCache.TryGetValue(id, out var p)) 
            return p;

        try
        {
            var process = Process.GetProcessById(id);
            processCache[id] = process;
            return process;
        }
        catch (Exception e)
        {
            processCache[id] = null;
            Debug.LogError(e);
            return null;
        }
    }

    public static WindowInfo GetFocusedWindow()
    {
        var hwnd = GetForegroundWindow();

        if (hwnd == IntPtr.Zero)
            return default;
        
        GetWindowThreadProcessId(hwnd, out var processId);
        
        if (processId == 0)
            return default;
        
        var process = GetProcessById((int) processId);
        GetWindowRect(hwnd, out var windowRect);
        
        var programRect = windowRect;
        var hwndOwner = GetWindow(hwnd, GW_OWNER);

        if (hwndOwner != hwnd && hwndOwner != IntPtr.Zero)
        {
            GetWindowRect(hwndOwner, out var ownerWindowRect);

            programRect.left = Mathf.Min(programRect.left, ownerWindowRect.left);
            programRect.top = Mathf.Min(programRect.top, ownerWindowRect.top);
            programRect.right = Mathf.Max(programRect.right, ownerWindowRect.right);
            programRect.bottom = Mathf.Max(programRect.bottom, ownerWindowRect.bottom);
        }

        return new WindowInfo
        {
            valid = true,
            processId = (int) processId,
            processName = process?.ProcessName,
            exePath = process?.MainModule?.FileName,
            title = process?.MainWindowTitle,
            rect = GetUnityRect(windowRect),
            programRect = GetUnityRect(programRect)
        };
    }

    private static Rect GetUnityRect(WindowsRect rect)
    {
        return Rect.MinMaxRect(
            rect.left, Screen.height - rect.bottom,
            rect.right, Screen.height - rect.top);
    }

    [StructLayout(LayoutKind.Sequential)]  
    private struct WindowsRect  
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}