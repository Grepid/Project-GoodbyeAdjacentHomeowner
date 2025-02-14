<!DOCTYPE html>
<html>
<head>
<style>
body {
  background-color: black;
  color: white;
}
</style>
</head>
<body>

# <font color = #00FFFF>Better Random Documentation</font>
-type- is equivalent to angled brackets with type.</br>
If you open this and it is not formatted, there is a PDF Version in the file too :)

## <font color = #FF0000>Usage</font>
To use, in your script please use the namespace Grepid.BetterRandom</br>
From there, you can access both static "Weighted" and "Rand" classes

## <font color=#00FF00>Weighted</font>

### <font color = #f5dd42>RandomFromCollection</font>
RandomFromCollection(ICollection-T- objects , string weightFieldName) takes a Collection of T and given a field name, can use that field name to make a weighted decision as to which element to return.</br>
</br>
Also has an Override RandomFromCollection(ICollection-T- objects , string weightFieldName , int repetitions , bool noDupes) that returns T[]. With the repetitions and noDupes you can set it to do the random weight check x amount of times with repetition and you can choose to allow the same element to be selected more than once with noDupes set to false. If you have noDupes set to true, your repetitions *must* be less than or equal to the number of elements in your collection.

### <font color = #f5dd42>ToWeights</font>
ToWeights(ICollection-T- , string fieldName) takes in a Collection of objects, and given a field name for a float will return an array with the same order as entered, but with floats from the field instead of initial T. (Useful for converting a collection of Class Instances to an array of their weights. Can technically also be used for any other float variable if you really want and need :) )

### <font color = #f5dd42>RandomIndex</font>
RandomIndex(ICollection-float- weights) gives you the index of a chosen element from the collection of floats passed in. Useful in a scenario where you want to spawn an Item, you can do:<br/>
`Item[] items;`<br/>
`int index = Weighted.RandomIndex(Weighted.ToWeights(items , "spawnChance"));`<br/>
`Item chosenItem = items[index];`<br/>
This has an array of items to spawn, and chooses a random index based on a variable in the Item class called "spawnChance".</br>
</br>
### <font color = #f5dd42>RandomIndexes</font>
RandomIndexes(ICollection-float- weights , int repetitions , bool noDupes) Same as RandomIndex, except returns an array of indexes and has extra controls. Controls are the same as RandomFromCollection repetitions. 

### <font color = #f5dd42>FlipWeights</font>
FlipWeights(ICollection-float- weights) will take a collection of floats and flip the floats around in place. For example<br/>
`{5,10,15,20,50}`<br/>
Will become<br/>
`{50,45,40,35,5}`<br/>
as it swaps their value from bottom to top. Any number will always stay the same distance away from any other number, but will be essentially flipped around the mid-point of all numbers present. If flipped again, will go back to the original arrangement.

### <font color = #f5dd42>ShiftValuesToPositive</font>
ShiftValuesToPositive(ICollection-float- weights) will take in a collection of floats that includes negatives, and shift until all values are positive, and the lowest value is between 1 and 2.

### <font color = #f5dd42>ShiftValuesToNegative</font>
ShiftValuesToPositive(ICollection-float- weights) will take in a collection of floats that includes negatives, and shift until all values are negative, and the lowest value is between -1 and -2. This is only useful if you intend on using AbsWeights or manipulating them until they are all positive, as any calculations require all positive values

### <font color = #f5dd42>AbsWeights</font>
AbsWeights(ICollection-float- weights) will take a collection of floats, and convert any negative numbers to positive (Their Abs / Absolute value)

## <font color=#00FF00>Rand</font>

### <font color = #f5dd42>RandomFromCollection</font>
RandomFromCollection(ICollection-T- objects) will take a collection, and return a random one, not based on weights. The same as getting a random index from Random.Range, but is quicker and easier to use. You can also use RandomFromCollection(ICollection-T- objects , int repetitions , bool noDupes) to get multiple random elements, and choose to have duplicates or not. Once again same requirements as all repeatable methods.

## <font color = #FF0000>Notes</font>
If you have any questions, feedback, or just want to speak to other users / me there is a discord! :D You can join [here!](https://discord.gg/kaSEJnrEuM)
</body>
