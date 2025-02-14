using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grepid.BetterRandom
{
    public class BetterRandomDemoScript : MonoBehaviour
    {
        public int Repetitions;
        public bool NoDupes;
        //Annoyingly this has to be a non-reorderable array or else the elements clip through eachother in the inspector :(
        [NonReorderable] public ItemDemo[] Items;
        private float[] ItemWeights => Weighted.ToWeights(Items, "SpawnChance");

        [ContextMenu("Weighted/RandomFromCollection")]
        public void RandomFromCollection()
        {
            print("RandomFromCollection:");
            print(Weighted.RandomFromCollection(Items, "SpawnChance").Name);
        }
        [ContextMenu("Weighted/RandomFromCollection Multiple")]
        public void RandomFromCollectionMulti()
        {
            print("RandomFromCollection (Multiple):");
            foreach (ItemDemo i in Weighted.RandomFromCollection(Items, "SpawnChance", Repetitions, NoDupes))
            {
                print(i.Name);
            }
        }
        [ContextMenu("Weighted/ToWeights")]
        public void ToWeights()
        {
            print("ToWeights:");
            foreach (float f in Weighted.ToWeights(Items, "SpawnChance"))
            {
                print(f);
            }
        }
        [ContextMenu("Weighted/RandomIndex")]
        public void RandomIndex()
        {
            print("RandomIndex:");
            print(Weighted.RandomIndex(ItemWeights));
        }
        [ContextMenu("Weighted/RandomIndexes")]
        public void RandomIndexes()
        {
            print("RandomIndexes:");
            foreach (int index in Weighted.RandomIndexes(ItemWeights, Repetitions, NoDupes))
            {
                print(index);
            }
        }
        [ContextMenu("Weighted/FlipWeights")]
        public void FlipWeights()
        {
            print("FlipWeights:");
            foreach (float f in Weighted.FlipWeights(ItemWeights))
            {
                print(f);
            }
        }
        [ContextMenu("Weighted/ShiftValuesToPositive")]
        public void ShiftValuesToPositive()
        {
            print("ShiftValuesToPositive:");
            foreach (float f in Weighted.ShiftWeightsToPositive(ItemWeights))
            {
                print(f);
            }
        }
        [ContextMenu("Weighted/ShiftValuesToNegative")]
        public void ShiftValuesToNegative()
        {
            print("ShiftValuesToNegative:");
            foreach (float f in Weighted.ShiftWeightsToNegative(ItemWeights))
            {
                print(f);
            }
        }
        [ContextMenu("Weighted/AbsWeights")]
        public void AbsWeights()
        {
            print("AbsWeights:");
            foreach (float f in Weighted.AbsWeights(ItemWeights))
            {
                print(f);
            }
        }

        [ContextMenu("Rand/RandomFromCollection")]
        public void RandFromCollection()
        {
            print("RandFromCollection:");
            print(Rand.RandFromCollection(Items).Name);
        }
        [ContextMenu("Rand/RandomFromCollection Multiple")]
        public void RandFromCollectionMulti()
        {
            print("RandFromCollection (Multiple):");
            foreach (ItemDemo i in Rand.RandFromCollection(Items, Repetitions, NoDupes))
            {
                print(i.Name);
            }
        }
    }
}