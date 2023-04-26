using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.BLL
{
    /// <summary>
    /// Статический класс, оперирующий с JSON хранилищем chatData
    /// </summary>
    public static class ChatDataController
    {
        /// <summary>
        /// Путь к JSON хранилищу chatData
        /// </summary>
        private static string _chatDataListJsonStoragePath = @"C:\Users\Витя\YandexDisk\C#\ПРОЕКТЫ\University\Data\chatData.json";

        public static void AddNewChatData(long chatId)
        {
            ChatData chatDataToAdd = new ChatData()
            {
                ChatId = chatId,
                CurrentMenu = MenuType.Start
            };

            List<ChatData>? chatDataList = GetChatDataList();
            chatDataList.Add(chatDataToAdd);

            string newChatDataListInJson = JsonConvert.SerializeObject(chatDataList);

            using (StreamWriter sw = new StreamWriter(_chatDataListJsonStoragePath))
            {
                sw.WriteLine(newChatDataListInJson);
            }
        }

        /// <summary>
        /// Возвращает экземпляр ChatData по chatId
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static ChatData? GetChatDataById(long chatId)
        {
            List<ChatData>? chatDataList = GetChatDataList();

            ChatData? chatData = chatDataList?
                .Where(cd => cd.ChatId == chatId)
                .FirstOrDefault();

            return chatData;
        }

        /// <summary>
        /// Возвращает список ChatData
        /// </summary>
        /// <returns></returns>
        public static List<ChatData>? GetChatDataList()
        {
            List<ChatData>? chatDataList = JsonConvert
                .DeserializeObject<List<ChatData>?>(
                    GetChatDataListInJson()
                );

            if (chatDataList is null) chatDataList = new List<ChatData>();

            return chatDataList;
        }

        /// <summary>
        /// Возвращает список ChatData в формате JSON
        /// </summary>
        /// <returns></returns>
        public static string GetChatDataListInJson()
        {
            string output = null;

            using (StreamReader sr = new StreamReader(_chatDataListJsonStoragePath))
            {
                output = sr.ReadToEnd();
            }

            return output;
        }
    }
}
