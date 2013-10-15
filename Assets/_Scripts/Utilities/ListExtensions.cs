using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions {
    public static partial class ListExtensions {
        public static List<T> Shuffle<T>(this List<T> list)  
        {  
            if (list == null) {
                return null;
            }

            int n = list.Count;
            while (n > 1) {
                n--;
                int k = UnityEngine.Random.Range(0,n + 1);  
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}

