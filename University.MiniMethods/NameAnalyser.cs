using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.MiniMethods
{
    public static class NameAnalyser
    {
        public static (string firstName, string lastName, string secondName) FullNameParseToStrings(string fullName) // Иванов И. И.
        {
            string[] strings = fullName.Replace(".", "")
                .Split(" ");

            string firstName = strings[1];
            string lastName = strings[0];
            string secondName = strings[2];

            return (firstName, lastName, secondName);
        }

        public static bool IsStringIsOnlyLastName(string input)
        {
            string[] strings = input.Trim().Split(" ");
            if (strings.Length > 1)
            {
                return false;
            }

            return true;
        }
    }
}
