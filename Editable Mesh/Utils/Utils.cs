using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static List<(int, int)> GetEdgeIndices(int[] indices, int startingIndex)
    {
        var edgeToSkip = new List<int>();
        for (int i = startingIndex; i < startingIndex + 3; i++)
        {
            for (int j = startingIndex + 3; j < startingIndex + 6; j++)
            {
                if (indices[i] == indices[j])
                    edgeToSkip.Add(indices[i]);
            }
        }

        var result = new List<(int, int)>();
        for (int i = 0; i < 6; i++)
        {
            var index = startingIndex + i;
            var nextIndex = i == 5 ? startingIndex : index + 1;
            
            if (edgeToSkip.Contains(indices[index]) && edgeToSkip.Contains(indices[nextIndex]))
                continue;

            result.Add((indices[index], indices[nextIndex]));
        }

        return result;
    }
}
