using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using University.Common;

namespace University.Bot
{
    /// <summary>
    /// Класс отвечает за отрисовку текстовых сообщений и клавиатур
    /// </summary>
    public static class MessageDrawer
    {

        public static ReplyKeyboardMarkup GetMainMenuKeyboard(bool isUserAdmin, bool isNullEntity)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup;
            KeyboardButton[][] buttons = GetMainMenuKeys();

            if (!isNullEntity) // Если записано название сущности по которой ищется расписание, то добавляется кнопка сброса поискового запроса
            {
                buttons = buttons.Append(new KeyboardButton[] { MenuMessages.RESET_SEARCH_QUERY }).ToArray();
            }

            if (isUserAdmin) // Если пользователь является админом, добавляем кнопку выхода из главного меню 
            {
                buttons = buttons.Append(new KeyboardButton[] { MenuMessages.ENTER_CHOOSE_MENU }).ToArray();
            }

            return new ReplyKeyboardMarkup(buttons) 
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetAdminMainMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_SCHEDULE },
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_EXAMS },
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_HEAD_INFO },
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_CORPUS_INFO },
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_LINKS_INFO },
                new KeyboardButton[] { MenuMessages.ADMIN_FILL_GROUP_PRACTICE_INFO },
                new KeyboardButton[] { MenuMessages.ADMIN_FILL_GROUP_INFO },
                new KeyboardButton[] { MenuMessages.ENTER_CHOOSE_MENU },
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetChooseMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { MenuMessages.ENTER_ADMIN_MENU },
                new KeyboardButton[] { MenuMessages.ENTER_ORD_MENU }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetBackKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {MenuMessages.BACK},
                })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

        }
        private static string GetDayOfWeekName(DayOfWeek dayOfWeek)
        {

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Понедельник";
                case DayOfWeek.Tuesday:
                    return "Вторник";
                case DayOfWeek.Wednesday:
                    return "Среда";
                case DayOfWeek.Thursday:
                    return "Четверг";
                case DayOfWeek.Friday:
                    return "Пятница";
                case DayOfWeek.Saturday:
                    return "Суббота";
                default:
                    break;
            }

            return "Воскресенье";
        }
        private static KeyboardButton[][] GetMainMenuKeys()
        {
            return new[]
                {
                    new KeyboardButton[] {MenuMessages.WATCH_TODAY_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_WEEK_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_EXAM_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_PRACTICE_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_GROUP_INFO},
                    new KeyboardButton[] {MenuMessages.WATCH_CORPUS_INFO},
                    new KeyboardButton[] {MenuMessages.WATCH_HEAD_INFO},
                    new KeyboardButton[] {MenuMessages.WATCH_LINKS_INFO},

                };
        }
    }
}
