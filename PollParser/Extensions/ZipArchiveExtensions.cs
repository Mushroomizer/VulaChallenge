using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using PollParser.Models;

namespace PollParser.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static List<User> ParseUsers(this ZipArchive za, string separator)
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

        public static List<Favourite> ParseFavourites(this ZipArchive za, string separator)
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
                    users.Add(new Favourite
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