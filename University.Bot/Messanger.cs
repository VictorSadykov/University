using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace University.Bot
{
    /// <summary>
    /// Класс отвечает за отправку и отрисовку сообщений ботом
    /// </summary>
    public class Messanger
    {
        /*p*//*rivate ITelegramBotClient _telegramClient;

        public Messanger(ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        public void SendMainMenuAsync()
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] {"Расписание"},
                    new KeyboardButton[] {"Option 2"},
                    new KeyboardButton[] {"Option 3"},
                }
                );

            return _telegramClient.SendTextMessageAsync()
        }*/
    }
}
