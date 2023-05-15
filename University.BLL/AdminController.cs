using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;

namespace University.BLL
{
    /// <summary>
    /// Класс оперирующий с хранилищем никнеймов админов 
    /// </summary>
    public class AdminController
    {
        private string filePath = DataConfig.DATA_FOLDER_PATH + "adminUserNames.txt";
        public bool IsAdmin(string? username)
        {
            string fileFullText = null;

            using (StreamReader sr = new StreamReader(filePath))
            {
                fileFullText = sr.ReadToEnd();
            }

            string[] userNames = fileFullText.Split("\r\n");

            foreach (var item in userNames)
            {
                if (username == item)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
