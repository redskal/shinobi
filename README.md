# shinobi
###### Crude secure deletion

A crude secure deletion tool written in C# for portability.

I wouldn't suggest using option 'f' anymore. When I wrote this it was rare to see a terabyte harddrive. Now large drives are commonplace and I can't see managed code running through them smoothly nor quickly.

TODO:
1) Clean up the option selection code. Specifically 'f' - it's a mess!
2) Error handling needs moving inside the loops.  Had some issues with crashing if drive access fails.