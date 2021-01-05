using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CollectionUtils
{
    public static bool RemoveWhere<T>(this List<T> list, Predicate<T> item)
    {
        var hasDeletions = false;
        
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (item(list[i]))
            {
                list.RemoveAt(i);
                hasDeletions = true;
            }
        }

        return hasDeletions;
    }
    
    public static ICollection<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        return Shuffle(collection.ToArray());
    }
    
    public static T[] Shuffle<T>(this T[] array)
    {
        if (array == null)
            return null;
        
        var shuffledArray = new T[array.Length];
        Array.Copy(array, shuffledArray, array.Length);

        for (var i = 0; i < shuffledArray.Length; i++)
        {
            var tempItem = shuffledArray[i];
            var shuffleIndex = UnityEngine.Random.Range(0, shuffledArray.Length);
            shuffledArray[i] = shuffledArray[shuffleIndex];
            shuffledArray[shuffleIndex] = tempItem;
        }

        return shuffledArray;
    }

    public static float MaxRange(this float[] array, float from, float to)
    {
        var fromIndex = Mathf.FloorToInt(array.Length * from);
        var toIndex = Mathf.FloorToInt(array.Length * to);
        var maxValue = 0f;

        for (var i = fromIndex; i < toIndex; i++)
        {
            maxValue = Mathf.Max(maxValue, array[i]);
        }
        
        return maxValue;
    }

    public static void ResampleTo(this float[] array, float[] target, bool useAverage = false)
    {
        var pointsPerSample = (float) array.Length / target.Length;
        var pointsPerSampleInt = Mathf.CeilToInt(pointsPerSample);
        var sampleIndex = 0f;

        for (var i = 0; i < target.Length; i++)
        {
            float sample;
            
            if (useAverage)
            {
                sample = 0f;
                
                for (var s = 0; s < pointsPerSample; s++)
                {
                    sample += array[Mathf.FloorToInt(sampleIndex)];
                    sampleIndex += 1;
                }

                sample /= pointsPerSampleInt;
            }
            else
            {
                sample = -float.MaxValue;
                
                for (var s = 0; s < pointsPerSample; s++)
                {
                    sample = Mathf.Max(array[Mathf.FloorToInt(sampleIndex)], sample);
                    sampleIndex += 1;
                }
            }
            
            target[i] = sample;
        }
    }
}
