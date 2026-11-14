using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Book
    {
        string Name { get; set; }
        string Year { get; set; }
        string Author { get; set; }
        public Book(string name, string year, string author)
        {
            Name = name;
            Year = year;
            Author = author;
        }
    }
}
