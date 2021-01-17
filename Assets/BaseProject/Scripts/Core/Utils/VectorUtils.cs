
using UnityEngine;

namespace CreativeMode
{
    public static class VectorUtils
    {
        public static Vector2 Round(this Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }
    }
}