/* Guiding principles: Focus on modularity, reusability and scalability

Written by: Yenuli De Silva 
Start date: Wednesday, October 7th, 2026 

*/

run with --> 

cd ~/Desktop/sycamore/program_v2/FitParser

dotnet build FitParser.Web/FitParser.Web.csproj

dotnet run --project FitParser.Api/FitParser.Api.csproj --urls http://localhost:5000

dotnet run --project FitParser.Web/FitParser.Web.csproj


TODO: 
Work on implementing a way for the user to select time-period ( so must convert times, epoch, combine same timestaps, and what not ) 
// we might have to convert times into normal times by finding which fields are of type date_time? 




and implement a way for the user to select exactly which message they want. 

Test TODO: 
Check data intergrity

TODO: mention whether uses scales and offsets. 
