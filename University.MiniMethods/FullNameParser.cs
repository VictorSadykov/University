using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.MiniMethods
{
    public static class FullNameParser
    {
        public static (string firstName, string lastName, string secondName) Parse(string fullName) // Иванов И. И.
        {
            string[] strings = fullName.Replace(".", "")
                .Split(" ");

            string firstName = strings[1];
            string lastName = strings[0];
            string secondName = strings[2];

            return (firstName, lastName, secondName);
        }
    }
}
