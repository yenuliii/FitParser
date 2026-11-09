/* Guiding principles: Focus on modularity, reusability and scalability

Written by: Yenuli De Silva 
Start date: Wednesday, October 7th, 2026 

*/

run console with --> 

cd ~/Desktop/sycamore/program_v2/FitParser

dotnet build FitParser.Console/FitParser.Console.csproj

dotnet run --project FitParser.Console/FitParser.Console.csproj

/Users/ /Desktop/fit_csv/program v2/FitParser/yourfile.fit

/Users/ /Desktop/fit_csv/program v2/FitParser/FitOutput/activity3.csv

run webpage with --> 

cd /Users/yenulidesilva/Desktop/sycamore/program_v2/FitParser

dotnet run --project FitParser.Api

//open another terminal and run 
cd /Users/yenulidesilva/Desktop/sycamore/program_v2/FitParser
dotnet run --project FitParser.Web

http://localhost:5247

TODO: 
Work on implementing a way for the user to select time-period ( so must convert times, epoch, combine same timestaps, and what not ) 
// we might have to convert times into normal times by finding which fields are of type date_time? 

and implement a way for the user to select exactly which message they want. 

Test TODO: 
Check data intergrity

TODO: mention whether uses scales and offsets. 


I broke the exporrt and program.cs something something, you'll handle it. cause rn itsjust showing me some random values. 
and the fit epoch time 
and also the program.cs should show all the fields 