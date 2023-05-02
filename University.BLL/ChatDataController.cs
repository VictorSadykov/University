using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Configuration;

namespace University.BLL
{
    /// <summary>
    /// Статический класс, оперирующий с JSON хранилищем chatData
    /// </summary>
    public class ChatDataController
    {
        private JsonService _jsonService = new JsonService(DataConfig.DataFolderPath + "chatData.json");       

        /// <summary>
        /// Добавление нового чата в JSON хранилище
        /// </summary>
        /// <param name="chatId"></param>
        public void AddNewChatData(long chatId)
        {
            ChatData chatDataToAdd = new ChatData()
            {
                ChatId = chatId,
                CurrentMenu = MenuType.Start
            };

            List<ChatData>? chatDataList = _jsonService.GetObject< List<ChatData> >();

            chatDataList.Add(chatDataToAdd);

            _jsonService.WriteObjectToJson(chatDataList);
        }

        /// <summary>
        /// Возвращает экземпляр ChatData по chatId
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public ChatData? GetChatDataById(long chatId)
        {
            List<ChatData>? chatDataList = _jsonService.GetObject< List<ChatData> >();

            ChatData? chatData = chatDataList?
                .Where(cd => cd.ChatId == chatId)
                .FirstOrDefault();

            return chatData;
        }


        public void UpdateChatDataById(long chatId, ChatData chatDataEditSource)
        {
            var chatDataArray = _jsonService.GetObject< List<ChatData> >()
                .ToArray();

            for (int i = 0; i < chatDataArray.Length; i++)
            {
                if (chatDataArray[i].ChatId == chatId)
                {
                    chatDataArray[i] = chatDataEditSource;
                }
            }

            _jsonService.WriteObjectToJson(chatDataArray.ToList());
        }

        public void UpdateChatDataCurrentMenuById(long chatId, MenuType menuType, ChatData chatData)
        {
            chatData.CurrentMenu = menuType;
            UpdateChatDataById(chatId, chatData);
        }

        public bool UpdateChatDataGroupName(long chatId, string? text, ChatData chatData)
        {
            if (true) // GroupRepo.GetGroupName(text) 
            {
                chatData.GroupName = text;
                return true;
            }

            return false;
        }
    }
}
