﻿using Newtonsoft.Json;
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

        public string? GetSearchQueryName(long chatId)
        {
            List<ChatData>? chatDataList = _jsonService.GetObject<List<ChatData>>();

            ChatData? chatData = chatDataList?
                .Where(cd => cd.ChatId == chatId)
                .FirstOrDefault();

            return chatData?.SearchQueryName;
        }

       

        public void UpdateNextMenuById(long chatId, MenuType? menuType, ChatData chatData)
        {
            chatData.NextMenu = menuType;
            UpdateById(chatId, chatData);
        }

        public void UpdateDayOffset(long chatId, int? dayOffset, ChatData chatData)
        {
            chatData.DayOffset = dayOffset;
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

        public void UpdateCurrentMenuById(long chatId, MenuType? menuType, ChatData chatData)
        {
            if (chatData is null)
            {
                return;
            }
            chatData.CurrentMenu = menuType;
            UpdateById(chatId, chatData);
        }

        public void UpdateSearchQueryName(long chatId, string? entityName, ChatData chatData)
        {
            chatData.SearchQueryName = entityName;
            UpdateById(chatId, chatData);
        }

        public void UpdateIsEntityGroupFlagById(long chatId, bool isEntityGroupFlag, ChatData chatData)
        {
            chatData.isEntityGroup = isEntityGroupFlag;
            UpdateById(chatId, chatData);
        }

        public void UpdateAdminCurrentEditingGroupName(long chatId, string groupName, ChatData chatData)
        {
            chatData.AdminCurrentGroupEditingName = groupName;
            UpdateById(chatId, chatData);
        }

        public void UpdateDayScheduleById(long chatId, DayOfWeek dayOfWeek, ChatData chatData)
        {
            chatData.CurrentScheduleDay = dayOfWeek;
            UpdateById(chatId, chatData);
        }

        public void UpdateWeekParityById(long chatId, int weekNumber, ChatData chatData)
        {
            chatData.CurrentWeekParity = weekNumber;
            UpdateById(chatId, chatData);
        }
    }
}
