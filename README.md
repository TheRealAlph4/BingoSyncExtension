# BingoSyncExtension

This is a library mod for loading and injecting additional goals and auto-marking logic into BingoSync.

# Usage
The method for loading the goals json file is subject to change if I find a better variant, everything else will be backwards-compatible.
```cs
// load the goals from a json file, same format as for BingoSync
List<LocalBingoSquare> listOfSquares = BingoSquareReader.readFromFile("<full path here>");

// inject them into BingoSync's internal list
BingoSquareInjector.InjectSquares(listOfSquares);

// to access BingoSync's variables, call the same method on VariableProxy as you would on BingoTracker, e.g.
VariableProxy.UpdateBoolean(varName, value);

// it is possible to use "Set" instead of "Update", it is just an alias, e.g.
VariableProxy.SetBoolean(varName, value);

// an alias is provided to make changing integers more convenient, the following two lines are equivalent:
VariableProxy.SetInteger(varName, VariableProxy.GetInteger(varName) + value);
VariableProxy.Increment(varName, value);
// if omitted, the "value" parameter is 1 by default

// to help debug, a way to track accesses to specific variables is provided
// to start tracking a variable, use
VariableProxy.trackVariable(varName);
// after this, any Get and Update operations on the variable will be logged to ModLog.txt
// to stop tracking a variable, use
VariableProxy.untrackVariable(varName);
// note: redundant calls to either function will be quietly ignored
```
