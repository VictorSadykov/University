using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;

namespace University.BLL
{
    /// <summary>
    /// Класс, оперирующий с JSON хранилищем chatData
    /// </summary>
    public class ChatDataController
    {
        private JsonService _jsonService = new JsonService(DataConfig.DATA_FOLDER_PATH + "chatData.json");       

        /// <summary>
        /// Добавление нового чата в JSON хранилище
        /// </summary>
        /// <param name="chatId"></param>
        public void Add(long chatId)
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
        public ChatData? GetById(long chatId)
        {
            List<ChatData>? chatDataList = _jsonService.GetObject< List<ChatData> >();

            ChatData? chatData = chatDataList?
                .Where(cd => cd.ChatId == chatId)
                .FirstOrDefault();

            return chatData;
        }

        public string? GetGroupName(long chatId)
        {
            List<ChatData>? chatDataList = _jsonService.GetObject<List<ChatData>>();

            ChatData? chatData = chatDataList?
                .Where(cd => cd.ChatId == chatId)
                .FirstOrDefault();

            return chatData?.GroupName;
        }

        public void UpdateNextMenuById(long chatId, MenuType? menuType, ChatData chatData)
        {
            chatData.NextMenu = menuType;
            UpdateById(chatId, chatData);
        }

        public void UpdateById(long chatId, ChatData chatDataEditSource)
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

        public void UpdateCurrentMenuById(long chatId, MenuType menuType, ChatData chatData)
        {
            chatData.CurrentMenu = menuType;
            UpdateById(chatId, chatData);
        }

        public void UpdateGroupName(long chatId, string groupName, ChatData chatData)
        {
            chatData.GroupName = groupName;
            UpdateById(chatId, chatData);
        }

      
    }
}
