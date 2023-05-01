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
            string text = update.Message.Text;

            long chatId = update.Message.From.Id;
            ChatData? chatData = ChatDataController.GetChatDataById(chatId);

            if (chatData is null) ChatDataController.AddNewChatData(chatId);

            switch (chatData.CurrentMenu)
            {
                case MenuType.Start: // Отрисовка главного меню

                    ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(new[]
                       {
                            new KeyboardButton[] {MenuMessages.SCHEDULE_MESSAGE},
                            new KeyboardButton[] {"Option 2"},
                            new KeyboardButton[] {"Option 3"},
                       }
                    );

                    await _telegramClient.SendTextMessageAsync(
                        chatId,
                        text: "Выберите пункт меню",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );

                    chatData.CurrentMenu = MenuType.MainMenu;
                    ChatDataController.UpdateChatDataById(chatId, chatData);

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
