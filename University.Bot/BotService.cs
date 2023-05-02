﻿using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using University.Configuration;
using Telegram.Bot.Polling;
using University.BLL;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace University.Bot
{
    public class BotService : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private Messanger _messanger;

        public BotService(ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } },
                cancellationToken: stoppingToken
                );

            Console.WriteLine("Бот запущен:");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Messanger messanger = new Messanger(botClient, cancellationToken); // Инициализация объекта службы отправки сообщений

            string text = update.Message.Text; // Текст сообщения

            long chatId = update.Message.From.Id; // Проверка новый ли чат
            ChatData? chatData = ChatDataController.GetChatDataById(chatId);

            if (chatData is null)
            {
                ChatDataController.AddNewChatData(chatId);
                chatData = ChatDataController.GetChatDataById(chatId);
            }

            switch (chatData.CurrentMenu) // Проверка в каком меню должен находится пользователь
            {
                case MenuType.Start: // Отрисовка стартового меню

                    await messanger.SendStartMenuMessageAsync(chatId);

                    ChatDataController.UpdateChatDataCurrentMenuById(chatId, MenuType.StartMenu, chatData);

                    break;

                case MenuType.StartMenu: // Отрисовка главного меню или меню ввода группы

                    switch (text)
                    {
                        case MenuMessages.START_INSERT_GROUP_NAME:

                            await messanger.SendStartingInsertGroupNameAsync(chatId);                            

                            ChatDataController.UpdateChatDataCurrentMenuById(chatId, MenuType.InsertingGroupName, chatData);

                            break;

                        case MenuMessages.START_SKIP:

                            await messanger.SendMainMenuAsync(chatId);

                            ChatDataController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);

                            break;

                        default:
                            break;
                    }

                    break;

                case MenuType.InsertingGroupName:

                    if (text == MenuMessages.BACK) ChatDataController.UpdateChatDataCurrentMenuById(chatId, MenuType.StartMenu, chatData);

                    bool isGroupNameIsValid = ChatDataController.UpdateChatDataGroupName(chatId, text, chatData);

                    if (isGroupNameIsValid)
                    {
                        await messanger.SendMainMenuAsync(chatId);
                        ChatDataController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                    }
                    else
                    {
                        await messanger.SendWrongGroupMessage(chatId);
                    }

                    break;

                case MenuType.MainMenu:

                    switch (text)
                    {
                        case MenuMessages.SCHEDULE_MESSAGE:

                            await _telegramClient.SendTextMessageAsync(
                                chatId,
                                "РАСПИСАНИЕ",
                                replyMarkup: new ReplyKeyboardMarkup(new[]
                                {
                                    new KeyboardButton[] {"Назад"}
                                }),
                                cancellationToken: cancellationToken
                                );

                            chatData.CurrentMenu = MenuType.Schedule;
                            ChatDataController.UpdateChatDataById(chatId, chatData);

                            break;

                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }

        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Задаем сообщение об ошибке в зависимости от того, какая именно ошибка произошла
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            Console.WriteLine("Ожидаем 10 секунд перед повторным подключением.");
            Thread.Sleep(10000);

           return Task.CompletedTask;        
        }
    }
}
