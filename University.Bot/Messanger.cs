using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using University.BLL;
using University.Common;
using University.DLL.Sqlite.Entities;

namespace University.Bot
{
    /// <summary>
    /// Класс отвечает за отправку сообщений ботом
    /// </summary>
    public class Messanger
    {
        private readonly ITelegramBotClient _telegramClient;

        public Messanger(ITelegramBotClient telegramBotClient)
        {
            _telegramClient = telegramBotClient;
        }


        private async Task<Message> SendTextMessageAsync(long chatId, string textMessage, CancellationToken ct)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                parseMode: ParseMode.Html
            );
        }

        private async Task<Message> SendTextMessageWithKeyboardAsync(long chatId, string textMessage, ReplyKeyboardMarkup replyMarkup, CancellationToken ct)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                replyMarkup: replyMarkup,
                parseMode: ParseMode.Html
            );
        }
        private async Task<Message> SendTextMessageWithBackKeyboardAsync(long chatId, string textMessage, CancellationToken ct)
        {
            string text = textMessage;
            var backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendMainMenuAsync(long chatId, bool isUserAdmin, bool isNullEntity, CancellationToken ct)
        {
            string text = MenuMessages.SELECT_MENU_ITEM;
            var mainMenuKeyBoard = MessageDrawer.GetMainMenuKeyboard(isUserAdmin, isNullEntity);
            return await SendTextMessageWithKeyboardAsync(chatId, text, mainMenuKeyBoard, ct);
        }

        public async Task<Message> SendChooseMenuAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SELECT_MENU_ITEM;
            var mainMenuKeyBoard = MessageDrawer.GetChooseMenu();
            return await SendTextMessageWithKeyboardAsync(chatId, text, mainMenuKeyBoard, ct);
        }

        public async Task<Message> SendAdminMainMenuAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SELECT_MENU_ITEM;
            var mainMenuKeyBoard = MessageDrawer.GetAdminMainMenu();
            return await SendTextMessageWithKeyboardAsync(chatId, text, mainMenuKeyBoard, ct);
        }

        public async Task<Message> GroupIsNotFoundMessageAsync(long chatId, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад в главное меню.", callbackData: "backToMenu"),
                }
            });

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                MenuMessages.GROUP_IS_NOT_FOUND,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );

        }

        // TODO: СГРУППИРОВАТЬ МЕТОДЫ 

        private string DrawOneDayLessons(List<Lesson> todayLessons, bool isGroupSchedule)
        {
            string output = null;

            todayLessons = todayLessons.OrderBy(l => l.TimeNumber).ToList();

            foreach (var lesson in todayLessons)
            {
                string lessonAlias = null;
                string emoji = null;
                switch (lesson.LessonType)
                {
                    case LessonType.Lecture:
                        lessonAlias = ScheduleLessonTypes.LESSON_NAME_LECTURE;
                        emoji = "✍";
                        break;
                    case LessonType.Practice:
                        lessonAlias = ScheduleLessonTypes.LESSON_NAME_PRACTICE;
                        emoji = "🛠";
                        break;
                    case LessonType.LabWork:
                        lessonAlias = ScheduleLessonTypes.LESSON_NAME_LABWORK;
                        emoji = "🔬";
                        break;
                    default:
                        break;
                }

                string timeStart = null;
                string timeEnd = null;

                switch (lesson.TimeNumber)
                {
                    case 1:
                        timeStart = ScheduleTimer.FIRST_START;
                        timeEnd = ScheduleTimer.FIRST_END;
                        break;
                    case 2:
                        timeStart = ScheduleTimer.SECOND_START;
                        timeEnd = ScheduleTimer.SECOND_END;
                        break;
                    case 3:
                        timeStart = ScheduleTimer.THIRD_START;
                        timeEnd = ScheduleTimer.THIRD_END;
                        break;
                    case 4:
                        timeStart = ScheduleTimer.FOURTH_START;
                        timeEnd = ScheduleTimer.FOURTH_END;
                        break;
                    case 5:
                        timeStart = ScheduleTimer.FIFTH_START;
                        timeEnd = ScheduleTimer.FIFTH_END;
                        break;
                    case 6:
                        timeStart = ScheduleTimer.SIXTH_START;
                        timeEnd = ScheduleTimer.SIXTH_END;
                        break;
                    case 7:
                        timeStart = ScheduleTimer.SEVENTH_START;
                        timeEnd = ScheduleTimer.SEVENTH_END;
                        break;
                    case 8:
                        timeStart = ScheduleTimer.EIGHTH_START;
                        timeEnd = ScheduleTimer.EIGHTH_END;
                        break;
                    default:
                        break;
                }



                string subGroup = lesson.SubGroup;
                if (subGroup != "0")
                {
                    output += subGroup + " подгруппа" + Environment.NewLine;
                }

                output += $"⏲ {timeStart} - {timeEnd}{Environment.NewLine}" +
                    $"{emoji} {lesson.Name}({lessonAlias}){Environment.NewLine}";

                if (isGroupSchedule)
                {
                    output += $"👩‍🏫 {lesson.Teacher.LastName} {lesson.Teacher.FirstName}. {lesson.Teacher.SecondName}.{Environment.NewLine}";
                }
                else
                {
                    foreach (var item in lesson.Groups)
                    {
                        output += $"👨‍🎓 {item.Name}{Environment.NewLine}";
                    }
                }
                    


                output += $"🏫 корп. \"{lesson.CorpusLetter}\" каб. \"{lesson.CabNumber}\"{Environment.NewLine}{Environment.NewLine}";


            }

            return output;

        }

        public async Task<Message> SendWeekScheduleAsync(long chatId, string entityName, bool isGroupSchedule, List<Lesson> weekLessons, int weekParity, CancellationToken ct)
        {
            string textMessage;
            if (isGroupSchedule)
            {
                textMessage = $"Группа: {entityName}{Environment.NewLine}" +
                $"Неделя {weekParity}{Environment.NewLine}{Environment.NewLine}";
            }
            else
            {
                textMessage = $"Преподаватель: {entityName}{Environment.NewLine}" +
                $"Неделя {weekParity}{Environment.NewLine}{Environment.NewLine}";
            }



            for (int dayNumber = 1; dayNumber <= 6; dayNumber++)
            {
                string dayName = GetDayOfWeekName((DayOfWeek)dayNumber);

                int lessonCount = weekLessons.Count(l => l.DayNumber == (DayOfWeek)dayNumber);

                if (lessonCount == 0)
                {
                    continue;
                }



                textMessage += $"📆 <b>{dayName}{Environment.NewLine}{Environment.NewLine}</b>" +
                    $"{DrawOneDayLessons(weekLessons.Where(l => l.DayNumber == (DayOfWeek)dayNumber).ToList(), isGroupSchedule)}" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}";
            }

            InlineKeyboardMarkup backButton = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад.", callbackData: "back")
                }
            );

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                replyMarkup: backButton,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendOneDayScheduleAsync(long chatId, string entityName, bool isGroupSchedule, int workingDaysCount, List<Lesson> dayLessons, DateTime dateTime, int weekParity, CancellationToken ct)
        {
            string textMessage;
            if (isGroupSchedule)
            {
                textMessage = $"Группа: {entityName}{Environment.NewLine}{Environment.NewLine}";
            }
            else
            {
                textMessage = $"Преподаватель: {entityName}{Environment.NewLine}{Environment.NewLine}";
            }

            string dayOfWeekName = dateTime.ToString("dddd");
            var strb = new StringBuilder(dayOfWeekName);
            strb[0] = dayOfWeekName[0].ToString().ToUpper()[0];
            dayOfWeekName = strb.ToString();

            textMessage += $"Неделя {weekParity}{Environment.NewLine}" +
            $"{dayOfWeekName}{Environment.NewLine}" +
            $"{dateTime.ToString("M")}{Environment.NewLine}{Environment.NewLine}";

           
            textMessage += $"{DrawOneDayLessons(dayLessons, isGroupSchedule)}" +
                $"{Environment.NewLine}" +
                $"{Environment.NewLine}";

            InlineKeyboardMarkup keyBoard;

            if (workingDaysCount > 1)
            {
                keyBoard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Предыдущий день.", callbackData: "prev"),
                        InlineKeyboardButton.WithCallbackData(text: "Следующий день.", callbackData: "next")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Назад.", callbackData: "back")
                    }
                });
            }
            else
            {
                keyBoard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Назад.", callbackData: "back")
                    }
                });
            }

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                replyMarkup: keyBoard,
                parseMode: ParseMode.Html
            );
        }

        private string GetDayOfWeekName(DayOfWeek dayOfWeek)
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
                case DayOfWeek.Sunday:
                    return "Воскресенье";             
            }

            return null;
        }

        private string GetDayOfMonthName(DateTime dateTime)
        {
            switch (dateTime.Month)
            {
                case 1:
                    return "января";
                case 2:
                    return "февраля";
                case 3:
                    return "марта";
                case 4:
                    return "апреля";
                case 5:
                    return "мая";
                case 6:
                    return "июня";
                case 7:
                    return "июля";
                case 8:
                    return "августа";
                case 9:
                    return "сентября";
                case 10:
                    return "октября";
                case 11:
                    return "ноября";
                case 12:
                    return "декабря";
            }

            return null;
        }





        /*public async Task<Message> SendPracticeInfoAsync(long chatId, Group group, CancellationToken ct)
        {
            string textMessage = $"{group.Name}{Environment.NewLine}{Environment.NewLine}" +
                $"Начало: {group.PracticeDateStart}{Environment.NewLine}" +
                $"Конец: {group.PracticeDateEnd}{Environment.NewLine}" +
                $"Руководитель: {group.PracticeTeacherFullName}";

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                replyMarkup: DrawBackKeyboard()
                );
        }*/

        /*public async Task<Message> SendExamScheduleAsync(long chatId, Group group, List<Exam>? exams, CancellationToken ct)
        {
            string textMessage = $"Группа {group.Name}.{Environment.NewLine}" +
                $"Расписание сессии.{Environment.NewLine}{Environment.NewLine}";


            foreach (var exam in exams)
            {
                string examName = null;
                switch (exam.ExaminationType)
                {
                    case ExaminationType.Exam:
                        examName = "Экзамен";
                        break;
                    default:
                        break;
                }

                textMessage +=
                    $"📆 {exam.StartDateTime.ToString("dd/MM/yyyy")}{Environment.NewLine}{Environment.NewLine}" +
                    $"⏲ {exam.StartDateTime.ToString("HH:mm")}{Environment.NewLine}" +
                    $"⚡ {exam.Name} ({examName}){Environment.NewLine}" +
                    $"👨‍🏫 {exam.TeacherFullName}{Environment.NewLine}" +
                    $"🏫 корп. \"{exam.Corpus.Name}\" каб. \"{exam.CabNumber}\"{Environment.NewLine}{Environment.NewLine}";  
            }

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: ct,
                replyMarkup: DrawBackKeyboard(),
                parseMode: ParseMode.Html
            );
        }*/


        public async Task<Message> SendReadyToProcessSchedulesAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SEND_GROUP_SCHEDULE_LINK;
            var backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }


        public async Task<Message> SendWrongFileForScheduleAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.WRONG_LINK_FORMAT;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendGroupAddedMessageAsync(long chatId, string groupName, CancellationToken ct)
        {

            string text = MenuMessages.NEW_GROUP_ADDED + " " + groupName;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendScheduleAddedMessageAsync(long chatId, string groupName, CancellationToken ct)
        {
            string text = MenuMessages.NEW_SCHEDULE_ADDED + " " + groupName;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendReadyToProcessHeadInfoAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SEND_HEAD_FILE;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendReadyToProcessLinksInfoAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SEND_LINKS_FILE;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendReadyToProcessCorpusInfoAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.SEND_CORPUS_FILE;
            ReplyKeyboardMarkup backKeyboard = MessageDrawer.GetBackKeyboard();
            return await SendTextMessageWithKeyboardAsync(chatId, text, backKeyboard, ct);
        }

        public async Task<Message> SendCorpusFileLoadedSuccessfullyAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.CORPUS_FILE_LOADED_SUCCESSFULLY;
            ReplyKeyboardMarkup menuKeyboard = MessageDrawer.GetAdminMainMenu();
            return await SendTextMessageWithKeyboardAsync(chatId, text, menuKeyboard, ct);
        }
        public async Task<Message> SendLinksFileLoadedSuccessfullyAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.LINKS_FILE_LOADED_SUCCESSFULLY;
            ReplyKeyboardMarkup menuKeyboard = MessageDrawer.GetAdminMainMenu();
            return await SendTextMessageWithKeyboardAsync(chatId, text, menuKeyboard, ct);
        }

        public async Task<Message> SendHeadFileLoadedSuccessfullyAsync(long chatId, CancellationToken ct)
        {
            string text = MenuMessages.HEAD_FILE_LOADED_SUCCESSFULLY;
            ReplyKeyboardMarkup menuKeyboard = MessageDrawer.GetAdminMainMenu();
            return await SendTextMessageWithKeyboardAsync(chatId, text, menuKeyboard, ct);
        }

        public async Task<Message> SendInfoAsync(long chatId, string text, bool isAdmin, bool isNullEntity, CancellationToken ct)
        {
            ReplyKeyboardMarkup mainMenuKeyboard = MessageDrawer.GetMainMenuKeyboard(isAdmin, isNullEntity);
            return await SendTextMessageWithKeyboardAsync(chatId, text, mainMenuKeyboard, ct);
        }

        public async Task<Message> StartInsertingSearchQueryAsync(long chatId, CancellationToken cancellationToken)
        {

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад в главное меню.", callbackData: "backToMenu"),
                }
            });

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                MenuMessages.INSERT_ENTITY_NAME,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );

        }

        public async Task<Message> SendTeacherVariants(long chatId, List<Teacher> allTeachersWithSameLastName, CancellationToken cancellationToken)
        {
            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

            foreach (var item in allTeachersWithSameLastName)
            {
                buttons.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{item.LastName} {item.FirstName}. {item.SecondName}.",
                        callbackData: item.Id.ToString()
                    ) 
                });
            }

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            string textMessage = "Найдено несколько преподавателей по введённой фамилии. Уточните по какому преподавателю производить поиск.";

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                textMessage,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
        }

        public async Task<Message> SendWeekParityKeyboard(long chatId, CancellationToken ct)
        {
            string text = "Выберите какой недели хотите посмотреть расписание.";

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "1 неделя", callbackData: "1"),
                    InlineKeyboardButton.WithCallbackData(text: "2 неделя", callbackData: "2"),
                },

                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад в главное меню.", callbackData: "backToMenu"),
                }
            });

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> RemoveKeyboard(long chatId, CancellationToken ct)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingGroupNameAsync(long chatId, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад в главное меню", callbackData: "backToMenu"),
                }
            });

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: MenuMessages.INSERT_GROUP_NAME,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> SendSearchQueryResetedSuccessfully(long chatId, bool isAdmin, bool isNullEntity, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup mainMenuKeyboard = MessageDrawer.GetMainMenuKeyboard(isAdmin, isNullEntity);
            return await SendTextMessageWithKeyboardAsync(chatId, MenuMessages.RESET_SEARCH_QUERY_SUCCESSFULLY, mainMenuKeyboard, cancellationToken);
        }

        public async Task<Message> StartInsertingGroupCode(long chatId, string groupName, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа {groupName} надена.\n\n" + MenuMessages.INSERT_GROUP_CODE;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingPracticeTeacherFullName(long chatId, string groupName, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа {groupName} надена.\n\n" + MenuMessages.INSERT_GROUP_PRACTICE_TEACHER_FULLNAME;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingGroupSpecialization(long chatId, string groupName, string groupCode, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа: {groupName}.\nКод: {groupCode}.\n\n" + MenuMessages.INSERT_GROUP_SPECIALIZATION;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingPracticeStartDate(long chatId, string groupName, string teacherFullName, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа: {groupName}.\nРуководитель: {teacherFullName}.\n\n" + MenuMessages.INSERT_PRACTICE_START_DATE;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingGroupOrientation(long chatId, string groupName, string groupCode, string groupSpecialization, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа: {groupName}.\nКод: {groupCode}.\nНаправление: {groupSpecialization}.\n\n" + MenuMessages.INSERT_GROUP_ORIENTATION;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> StartInsertingPracticeEndDate(long chatId, string groupName, string fullname, string startDate, CancellationToken ct)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            });

            string message = $"Группа: {groupName}.\nРуководитель: {fullname}.\nНачало: {startDate}.\n\n" + MenuMessages.INSERT_PRACTICE_END_DATE;

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
                );
        }

        public async Task<Message> GroupInfoSavedAsync(long chatId, string groupName, string groupCode, string groupSpecialization, string groupOrientation, CancellationToken ct)
        {
            string message = $"Информация о группе сохранена.\n\nГруппа: {groupName}.\nКод: {groupCode}.\nНаправление: {groupSpecialization}.\nНаправленность: {groupOrientation}";

            return await SendTextMessageAsync(chatId, message, ct);
        }

        public async Task<Message> GroupPracticeInfoSavedAsync(long chatId, string groupName, string fullname, string startDate, string endDate, CancellationToken ct)
        {
            string message = $"Информация о группе сохранена.\n\nГруппа: {groupName}.\nРуководитель: {fullname}.\nНачало: {startDate}.\nКонец: {endDate}";

            return await SendTextMessageAsync(chatId, message, ct);
        }
    }
}
