using System;

namespace Project.Scripts.Utils
{
    public class TextUtils
    {
        public static string FuckingFilter(string text)
        {
            var lastSpace = text.LastIndexOf(' ');

            if (lastSpace == -1)
            {
                return "Fucking " + text;
            }
            
            var startIndex = UnityEngine.Random.Range(0, lastSpace - 1);
            var insertPosition = text.IndexOf(' ', startIndex);

            return text.Insert(insertPosition, " fucking");
        }
    }
}