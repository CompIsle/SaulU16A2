using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ConsoleApp2
{
    internal class Program
    {
        public static readonly string AppRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        static void Main(string[] args)
        {
            //executable for program is not in the same location as the project- hence the need for removing the end of the path
            var fullPath = Path.Combine(AppRoot.Remove(AppRoot.Length - 16, 16), "Data.csv");

            /*
            original code here..
            csv.Context.RegisterClassMap<BookMap>();
            IEnumerable<Book> records = csv.GetRecords<Book>();
            //List to contain a copy of all the generated recrods, as I could not figure out how to write with them properly inside here???
            List<BookTwo> newrecords = new();
            csv.Read();
            csv.ReadHeader();
            */
            // change benefits: the file copying process has been removed. The program now directly reads from the original CSV file specified by Data.csv and writes the modified records to the new CSV file named Data2.csv not quite as they are read, sadly (yet?)
            

            /*
            original code here..
            var reader = new StreamReader(fullPath);
            */
            // change benefits: the using statement ensures the StreamReader and StreamWriter objects will be disposed of automatically at the end of their respective scopes, ensuring proper resource cleanup and avoiding resource leaks
            using (var reader = new StreamReader(fullPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<BookMap>();
                IEnumerable<Book> records = csv.GetRecords<Book>();

                List<BookTwo> newRecords = new List<BookTwo>();

                csv.Read();
                csv.ReadHeader();

                /*
                original method included creating a List to hold a secon record (newrec) of the BookTwo type, which copied the properties of the record of type Book's data into them- and then Hashed the object to get a unique Hash code later.
                Now there is a set method for the Hash (ID), but a List is still used. Simply the most convenient way for me to do it, it seems.
                It no longer creates one that is the same dependant on the record, I'm unsure why (FIXED NOW)
                */

                while (csv.Read())
                {
                    var record = csv.GetRecord<Book>();

                    Console.WriteLine(record.GetHashCode());
                    Console.WriteLine(record.Name, record.Title, record.Place, record.Publisher, record.Date);

                    BookTwo newRecord = new BookTwo
                    {
                        Name = record.Name,
                        Title = record.Title,
                        Place = record.Place,
                        Publisher = record.Publisher,
                        Date = record.Date,
                        Hash = "",
                };

                    // Console.WriteLine($"{newRecord.Name} {newRecord.Title} {newRecord.Place} {newRecord.Publisher} {newRecord.Date} {newRecord.Hash}");

                    newRecords.Add(newRecord);
                    Console.WriteLine(newRecords.Count());
                }

                using (var writer = new StreamWriter(Path.Combine(AppRoot.Remove(AppRoot.Length - 16, 16), "Data2.csv")))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csvWriter.Context.RegisterClassMap<BookTwoMap>();
                    // The brief seems to ask for no headers, but alas it is here if need be and for my sanity
                    /*
                    csvWriter.WriteHeader<BookTwo>();
                    csvWriter.NextRecord();
                    */

                    foreach (BookTwo r in newRecords)
                    {
                        csvWriter.WriteRecord(r);
                        csvWriter.NextRecord();
                    }
                }
            }

            Console.WriteLine(fullPath);
            Console.WriteLine(File.Exists(fullPath));

            // Console.ReadLine();
        }
    }

    public class Book
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Place { get; set; }
        public string Publisher { get; set; }
        public string Date { get; set; }
    }
    public class BookMap : ClassMap<Book>
    {
        public BookMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Title).Name("Title");
            Map(m => m.Place).Name("Place publication");
            Map(m => m.Publisher).Name("Publisher");
            Map(m => m.Date).Name("Date of publication");
        }
    }

    public class BookTwo : Book
    {
        /* 
        original code here..
        public string Name { get; set; }
        public string Title { get; set; }
        public string Place { get; set; }
        public string Publisher { get; set; }
        public string Date { get; set; }
        */
        // just kind of stupid to have redefined them because they're inherited from the base class anyways

        /*
         * original code here..
        public int Hash { get; set; }
        */
        // changed such that a fixed length (padded with 0s on the left ID is generated). IDs with fixed lengths is good form

        private const int FixedLength = 8; // Desired fixed length for the ID

        private string hash;

        public string Hash
        {
            get
            {
                return (hash);
            }
            set
            {
                // Generate the MD5 hash of the book's data
                using (var md5 = MD5.Create())
                {
                    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(ToSecondString()));
                    var hashValue = new BigInteger(hashBytes);
                    var positiveHash = BigInteger.Abs(hashValue); // Convert to positive value
                    var formattedHash = positiveHash.ToString().PadLeft(FixedLength, '0'); // Fixed length formatting
                    hash =  formattedHash.Substring(0, 8);
                }
            }
        }

        public string ToSecondString()
        {
            return ($"{this.Name},{this.Title},{this.Place},{this.Publisher},{this.Date}");
        }
    }
    public class BookTwoMap : ClassMap<BookTwo>
    {
        public BookTwoMap()
        {
            Map(m => m.Hash).Name("ID");
            Map(m => m.Name).Name("Name");
            Map(m => m.Title).Name("Title");
            Map(m => m.Place).Name("Place publication");
            Map(m => m.Publisher).Name("Publisher");
            Map(m => m.Date).Name("Date of publication");         
        }
    }
}