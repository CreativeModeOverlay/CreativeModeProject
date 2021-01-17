using System.Runtime.InteropServices;
using UnityEngine;

namespace CreativeMode
{
    public static class UnityUtils
    {
        public static void FixFreezingWindow()
        {
            if (!Application.isEditor)
                DisableProcessWindowsGhosting();
        }
        
        [DllImport("user32.dll")]  
        private static extern void DisableProcessWindowsGhosting();
    }
}