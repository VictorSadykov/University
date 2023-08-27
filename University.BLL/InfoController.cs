using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;

namespace University.BLL
{
    /// <summary>
    /// Класс оперирующий с файлами для информационных сообщений
    /// </summary>
    public class InfoController
    {
        private const string HEAD_PATH = DataConfig.DATA_FOLDER_PATH + "info/head.txt";
        private const string CORPUS_PATH = DataConfig.DATA_FOLDER_PATH + "info/corpus.txt";
        private const string LINKS_PATH = DataConfig.DATA_FOLDER_PATH + "info/links.txt";

        private async Task<string> GetInfo(string path)
        {
            string output = null;
            using (StreamReader sr = new StreamReader(path))
            {
                output = await sr.ReadToEndAsync();
            }

            return output;
        }

        public async Task<string> GetHeadInfo()
        {
            return await GetInfo(HEAD_PATH);
        }

        public async Task<string> GetCorpusInfo()
        {
            return await GetInfo(CORPUS_PATH);
        }

        public async Task<string> GetLinksInfo()
        {
            return await GetInfo(LINKS_PATH);
        }
    }
}
