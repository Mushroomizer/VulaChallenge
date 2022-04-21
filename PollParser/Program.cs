using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using PollParser.Extensions;
using PollParser.Models;

namespace PollParser
{
    internal class Program
    {
        private static void Main(string[] args)
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
                inputZip = Console.ReadLine()?.Trim();
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
                using var za = ZipFile.OpenRead(inputZip);
                users = za.ParseUsers("\t");
                favourites = za.ParseFavourites(" ");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Could not find file: " + e.FileName);
                Console.ReadLine(); //Readline so the console doesnt close
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.ReadLine(); //Readline so the console doesnt close
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

        private static void ShowMenu(IGrouping<string, Favourite> favouriteColor,
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
    }
}