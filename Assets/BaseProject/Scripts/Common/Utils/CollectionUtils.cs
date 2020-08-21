﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CollectionUtils
{
    public static ICollection<T> Shuffle<T>(this ICollection<T> collection)
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
}
