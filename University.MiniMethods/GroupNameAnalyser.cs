using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.MiniMethods
{
    public static class GroupNameAnalyser
    {
        public static bool DefineIsStringGroupName(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (int.TryParse(input[i].ToString(), out int result))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
