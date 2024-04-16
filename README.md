# BingoSyncExtension

This is a library mod for loading and injecting additional goals and auto-marking logic into BingoSync.

# Usage
The method for loading the goals json file is subject to change if I find a better variant, everything else will be backwards-compatible.
```cs
// the "hollow_knight_Data"-folder's name is distinct depending on the install being from steam or GOG
// for those 2 variants, there's this function to return any one that exists, string.Empty if neither exist
string hk_data = BingoSquareReader.GetHKDataFolderName();
string path = @$".\{hk_data}\Managed\Mods\";
// load the goals from a json file, same format as for BingoSync
List<LocalBingoSquare> listOfSquares = BingoSquareReader.readFromFile(@$"{path}\<goal_pack_mod>\<goal_pack>.json");

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
VariableProxy.TrackVariable(varName);
// after this, any Get and Update operations on the variable will be logged to ModLog.txt
// to stop tracking a variable, use
VariableProxy.TntrackVariable(varName);
// note: redundant calls to either function will be quietly ignored
```
