using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using PollParser.Models;

namespace PollParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputZip = "";
            //Handle invalid input and print a help message
            if (args.Length != 1)
            {
                Console.WriteLine("Cmdline Usage: PollParser.exe <path to zipped poll file>");
                Console.WriteLine("Note: Parses users.txt");
                Console.WriteLine("Note: Parses favorites.txt");
                //So you dont have to type the filepath every time
                Console.WriteLine("Enter the filepath to a zipped data file(press enter for \"data.zip\"):");
                inputZip = Console.ReadLine().Trim();
                if (string.IsNullOrEmpty(inputZip))
                {
                    inputZip = "data.zip";
                }
            }

            //Initialize arrays for holding our parsed data
            List<User> users;
            List<Favourite> favourites;

            var sw = Stopwatch.StartNew();
            //Disposable handler for reading the zipped file
            try
            {
                using (ZipArchive za = ZipFile.OpenRead(inputZip))
                {
                    users = ParseUsers(za, "\t");
                    favourites = ParseFavourites(za, " ");
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Could not find file: " + e.FileName);
                Console.ReadLine();//Readline so the console doesnt close
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.ReadLine();//Readline so the console doesnt close
                return;
            }
            Console.WriteLine($"Successfully parsed the files in {sw.Elapsed.TotalSeconds} seconds");
            

            // Process the parsed data
            var favouriteColor = favourites.GroupBy(x => x.Colour).OrderByDescending(x => x.Count()).ToList()
                .First();
            var userIdsWhoVotedForFavouriteColour =
                favourites.Where(x => x.Colour == favouriteColor.Key).Select(x => x.UserId).ToList();
            var usersWhoVotedForTheColourOrdered = users.Where(x => userIdsWhoVotedForFavouriteColour.Contains(x.Id))
                .OrderBy(x => x.Name);

            //Show a menu with the results pre-calculated to save time
            ShowMenu(favouriteColor, usersWhoVotedForTheColourOrdered);
        }

        static void ShowMenu(IGrouping<string, Favourite> favouriteColor,
            IOrderedEnumerable<User> usersWhoVotedForTheColourOrdered)
        {
            //Loop, allowing the user to select a menu option
            while (true)
            {
                Console.WriteLine("1. Print most popular colour");
                Console.WriteLine("2. Print list of users who voted for the colour, ordered");
                Console.WriteLine("q. Quit");
                var input = Console.ReadLine();
                //Handle breaking out of the loop
                if (input!.Trim().Equals("q", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }

                //Switch on the input
                switch (input.Trim())
                {
                    case "1":
                        Console.WriteLine(
                            $"Most voted for colour: {favouriteColor.Key} with {favouriteColor.Count()} votes");
                        break;
                    case "2":
                        usersWhoVotedForTheColourOrdered.ToList().ForEach(x => Console.WriteLine(x.Name));
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }

        static List<User> ParseUsers(ZipArchive za, string separator)
        {
            var usersFile = za.GetEntry("users.txt");
            if (usersFile == null)
            {
                Console.WriteLine("No users.txt file found");
                return new List<User>();
            }

            using var reader = new StreamReader(usersFile.Open());

            var users = new List<User>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                try
                {
                    var parts = line?.Split(separator);
                    users.Add(new User
                    {
                        Id = int.Parse(parts[0].Trim()),
                        Name = parts[1].Trim()
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not parse line {line}", e);
                }
            }

            return users;
        }

        static List<Favourite> ParseFavourites(ZipArchive za, string separator)
        {
            var favouritesFile = za.GetEntry("favourites.txt");
            if (favouritesFile == null)
            {
                Console.WriteLine("Favourites file not found");
                return new List<Favourite>();
            }

            using var reader = new StreamReader(favouritesFile.Open());

            var users = new List<Favourite>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                try
                {
                    var parts = line?.Split(separator);
                    users.Add(new Favourite()
                    {
                        UserId = int.Parse(parts[0].Trim()),
                        Colour = parts[1].Trim()
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not parse line {line}", e);
                }
            }

            return users;
        }
    }
}