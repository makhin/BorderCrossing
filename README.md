# BorderCrossing

This program parses the history of locations from Google, finds the dates of crossing state borders and calculates the number of days spent in each state.
The required initial data is a file with the history of locations, how to get it, see here.
Limitations: Google records location information almost every minute, and if you check each point, the verification process can take a long time, so the program checks two points per day, which can significantly speed up the process, but, on the other hand, with this algorithm, short trips can be lost.
Known Issues: Sometimes inaccurate country identification in coastal areas. In this version, countries are limited to Europe, and the frequency of checks is twice a day.
Site deployed to https://bordercrossing.azurewebsites.net/
