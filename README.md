Toy Robot Simulator
===================

Running the app
---------------

The only requirement to run the app is just to have dotnet core installed.
Run "dotnet run" in the same folder as the .csproj file.
The C# language version is already set inside the project file at version 7.1 in order to enable support for the async Main function.

Commands can be provided either by file (it must be the "commands.txt" file, which is hard coded and contains the test data used to develop the app) or by issuing them via console. Since reading a file is a batch operation, I've coded the same behavior into the console approach, so you need to issue all commands first and then they'll be executed in a batch.

There are 2 options that can be provided when launching the app:

--autoReport which will cause the Robot to report its status after each successful and non-report command. I've introduced this option to facilitate the development and I think it's more pleasant to see the Robot move one step at a time.

--log*Level* where *Level* is one of "None", "Failed" or "All", which will enable logging a report for each failed command, for all of them or for none. This is an extra kind of log in addition to the output from the Report instruction.
