using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Grepid.BetterRandom
{
    #region Weighted
    public static class Weighted
    {
        [Tooltip("Holds the keys for Reflection lookups to save needing to do the Reflection every time for the same fields")]
        private static ConcurrentDictionary<string,FieldInfo> s_fieldCache = new ConcurrentDictionary<string,FieldInfo>();

        /// <summary>
        /// Will return an object from a collection based on weighted calculation given the field name for the weights.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="weightFieldName"></param>
        /// <returns></returns>
        public static T RandomFromCollection<T>(ICollection<T> objects,string weightFieldName)
        {
            var asWeights = ToWeights(objects, weightFieldName);
            return objects.ElementAt(RandomIndex(asWeights));
        }

        /// <summary>
        /// Will return an Array of objects from a collection based on weight calculations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="weightFieldName"></param>
        /// <param name="repetitions"></param>
        /// <param name="noDupes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T[] RandomFromCollection<T>(ICollection<T> objects, string weightFieldName, int repetitions, bool noDupes)
        {
            List<T> modifiedObjects = new List<T>(objects);
            if (noDupes && repetitions > modifiedObjects.Count)
            {
                throw new ArgumentException("Cannot have more repetitions than objects whilst not allowing dupes.");
            }
            List<T> result = new List<T>();

            for (int i = 0; i < repetitions; i++)
            {
                T rand = RandomFromCollection(modifiedObjects, weightFieldName);
                result.Add(rand);
                if (noDupes) modifiedObjects.Remove(rand);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Will convert any given collection of objects into a float array given a field name to look for.
        /// Useful for creating a collection of weights from an Array of Class instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float[] ToWeights<T>(ICollection<T> objects, string fieldName)
        {
            float[] result = new float[objects.Count];

            Type type = typeof(T);
            string cacheKey = type.FullName + "_" + fieldName;
            FieldInfo fi;
            if (s_fieldCache.ContainsKey(cacheKey))
            {
                fi = s_fieldCache[cacheKey];
            }
            else
            {
                fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                s_fieldCache[cacheKey] = fi;
            }

            if(fi == null)
            {
                throw new ArgumentException("The passed field " + fieldName + " was not found in " + type);
            }

            int index = 0;
            foreach (T obj in objects)
            {
                float value = (float)fi.GetValue(obj);
                result[index] = value;
                index++;
            }
            return result;
        } 

        /// <summary>
        /// Will return the index of the weight that was selected through weighted calculation.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int RandomIndex(ICollection<float> weights)
        {
            float totalWeights = 0;

            //Adds all weights together to know the total
            foreach (float f in weights) totalWeights += f;

            if(totalWeights == 0)
            {
                throw new ArgumentException("All weights are 0. Cannot have a list of all empty weights");
            }
            if(totalWeights < 0)
            {
                throw new ArgumentException("Cannot have negative weights. Try absolute values, or Absolute values into the <><><><>");
            }

            //Will loop until it returns out via a winning weight being selected
            while (true)
            {
                //Will get a random index to challenge against
                int randIndex = UnityEngine.Random.Range(0, weights.Count);
                //If chance is 0 then just skip
                if (weights.ElementAt(randIndex) == 0) continue;
                //
                float checkWeight = UnityEngine.Random.Range(0, totalWeights);
                if (weights.ElementAt(randIndex) >= checkWeight) return randIndex;
            }
        }

        /// <summary>
        /// Returns an array of indexes based on multiple iterations on weight calculations
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="repetitions"></param>
        /// <param name="noDupes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int[] RandomIndexes(ICollection<float> weights, int repetitions, bool noDupes)
        {
            List<float> modifiedWeights = new List<float>(weights);
            List<int> usedIndexes = new List<int>();
            if (noDupes && repetitions > modifiedWeights.Count)
            {
                throw new ArgumentException("Cannot have more repetitions than objects whilst not allowing dupes.");
            }
            List<int> result = new List<int>();
            for (int i = 0; i < repetitions; i++)
            {
                int rand = RandomIndex(modifiedWeights);
                while (usedIndexes.Contains(rand) && noDupes)
                {
                    rand = RandomIndex(modifiedWeights);
                }
                result.Add(rand);
                if(noDupes) usedIndexes.Add(rand);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Will Flip all the values in place. Lowest value becomes highest, highest lowest, and everything between flipped.
        /// If not producing expected results, please refer to documentation
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float[] FlipWeights(ICollection<float> weights)
        {
            float[] result = weights.ToArray();
            float top = 0;
            float bottom = result[0];
            int index = 0;
            foreach (float f in result)
            {
                if (f < 0)
                {
                    throw new ArgumentException("All weights should be positive. Use AbsWeights or ShiftWeightsToPositive if needed.");
                }
                if (f > top) top = f;
                if (f < bottom) bottom = f;
                result[index] *= -1;
                index++;
            }
            for(int i = 0; i < result.Length; i++)
            {
                result[i] += (bottom + top);
            }
            return result;
        }

        /// <summary>
        /// Shifts all numbers positively the distance of the lowest negative value's Absolute value + 1 (so it doesn't get flagged as 0) ({-2,3,6} would go to {1,6,9})
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static float[] ShiftWeightsToPositive(ICollection<float> weights)
        {
            float[] result = weights.ToArray();
            float shiftFactor = 0;
            foreach(float f in weights)
            {
                if (f < shiftFactor) shiftFactor = f;
            }
            if(shiftFactor == 0) return result;
            for(int i = 0; i < result.Length; i++)
            {
                result[i] += Mathf.Abs(shiftFactor-1);
            }
            return result;
        }

        /// <summary>
        /// Shifts all numbers negatively the distance of the highest positive value + 1 (so it doesn't get flagged as 0).({2,-3,-6} would go to {-1,-6,-9}) 
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static float[] ShiftWeightsToNegative(ICollection<float> weights)
        {
            float[] result = weights.ToArray();
            float shiftFactor = 0;
            foreach (float f in weights)
            {
                if (f > shiftFactor) shiftFactor = f;
            }
            if (shiftFactor == 0) return result;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] -= shiftFactor+1;
            }
            return result;
        }

        /// <summary>
        /// Will convert all values to their Absolute value.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static float[] AbsWeights(ICollection<float> weights)
        {
            float[] result = weights.ToArray();
            for(int i = 0;i < result.Length; i++)
            {
                result[i] = Mathf.Abs(result[i]);
            }
            return result;
        }
    }
    #endregion



    #region Random
    public static class Rand 
    {
        /// <summary>
        /// Will return a random element from a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static T RandFromCollection<T>(ICollection<T> objects)
        {
            return objects.ElementAt(UnityEngine.Random.Range(0, objects.Count));
        }

        /// <summary>
        /// Will return an array of elements from a collection with extra controls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="repetitions"></param>
        /// <param name="noDupes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T[] RandFromCollection<T>(ICollection<T> objects,int repetitions,bool noDupes)
        {
            T[] result = new T[repetitions];
            List<int> usedIndexes = new List<int>();
            if(noDupes && repetitions > objects.Count)
            {
                throw new ArgumentException("Cannot have more repetitions than objects whilst not allowing dupes.");
            }
            int x = 0;
            while(x < repetitions)
            {
                int rand = UnityEngine.Random.Range(0, objects.Count);
                if (usedIndexes.Contains(rand) && noDupes) continue;
                result[x] = objects.ElementAt(rand);
                usedIndexes.Add(rand);
                x++;
            }
            return result;
        }
    }
    #endregion
}