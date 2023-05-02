using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using University.Configuration;

namespace University.Bot
{
    /// <summary>
    /// Класс отвечает за отправку и отрисовку сообщений ботом
    /// </summary>
    public class Messanger
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly CancellationToken _ct;

        public Messanger(ITelegramBotClient telegramBotClient, CancellationToken ct)
        {
            _telegramClient = telegramBotClient;
            _ct = ct;
        }

        public async Task<Message> SendStartMenuMessageAsync(long chatId)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {MenuMessages.START_INSERT_GROUP_NAME},
                    new KeyboardButton[] {MenuMessages.START_SKIP}
                }
            );

            string textMarkup = $"Выберите пункт меню.{Environment.NewLine}" +
                $"<b>Ввод названия группы для просмотра расписания занятий и экзаменов данной группы</b>{Environment.NewLine}" +
                $"<b>Пропустить, нужно будет постоянно вводить название группы перед просмотром расписаний</b>";

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMarkup,
                replyMarkup: replyMarkup,
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendStartingInsertGroupNameAsync(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Введите название группы",
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendMainMenuAsync(long chatId)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {MenuMessages.SCHEDULE_MESSAGE},
                }
            );

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Выберите пункт меню",
                replyMarkup: replyMarkup,
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendWrongGroupMessage(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "По данной группе ничего не найдено",
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }
    }
}
