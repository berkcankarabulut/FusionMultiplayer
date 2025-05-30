﻿using System.Collections.Generic;

namespace BehaviourSystem
{
    public static class Utils
    {
        public static System.Random r = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}