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
                replyMarkup: DrawBackKeyboard(),
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendMainMenuAsync(long chatId)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(DrawMainMenu())
            {
                ResizeKeyboard = true
            };

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Выберите пункт меню",
                replyMarkup: replyMarkup,
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> GroupIsNotFoundMessageAsync(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Данной группы не существует. Попробуйте ввести название группы правильно.",
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendCorpusInfo(long chatId, List<Corpus>? corpuses)
        {
            string output = null;

            foreach (var corpus in corpuses)
            {
                output += $"Корпус: {corpus.Name}{Environment.NewLine}" +
                    $"Адрес: {corpus.Address}{Environment.NewLine}{Environment.NewLine}";
            }

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: output,
                cancellationToken: _ct,
                parseMode: ParseMode.Html
            );
        }

        private string DrawOneDayLessons(List<Lesson> todayLessons)
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

                

                bool isLessonWithAllSubgroups = false;

                output += $"⏲ {timeStart} - {timeEnd}{Environment.NewLine}" +
                    $"{emoji} {lesson.Name}({lessonAlias}){Environment.NewLine}" +
                    $"👩‍🏫 {lesson.TeacherFullName}{Environment.NewLine}" +
                    $"🏫 корп. \"{lesson.CorpusLetter}\" каб. \"{lesson.CabNumber}\"{Environment.NewLine}{Environment.NewLine}";
                    

            }

            return output;

        }

        public async Task<Message> SendWeekScheduleAsync(long chatId, string groupName, List<Lesson> weekLessons, int weekParity)
        {
            string textMessage = $"Группа: {groupName}{Environment.NewLine}" +
                $"Неделя {weekParity}{Environment.NewLine}{Environment.NewLine}";


            for (int dayNumber = 1; dayNumber <= 6; dayNumber++)
            {
                string dayName = GetDayOfWeekName((DayOfWeek)dayNumber);

                int lessonCount = weekLessons.Count(l => l.DayNumber == (DayOfWeek)dayNumber);

                if (lessonCount == 0)
                {
                    continue;
                }

                

                textMessage += $"📆 <b>{dayName}{Environment.NewLine}{Environment.NewLine}</b>" +
                    $"{DrawOneDayLessons(weekLessons.Where(l => l.DayNumber == (DayOfWeek)dayNumber).ToList())}" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}";
            }

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard(),
                parseMode: ParseMode.Html
            );
        }

        private ReplyKeyboardMarkup DrawBackKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] {MenuMessages.BACK},
                })
            {
                ResizeKeyboard = true
            };
            
        }

        private KeyboardButton[][] DrawMainMenu()
        {
            return new[]
                {
                    new KeyboardButton[] {MenuMessages.WATCH_TODAY_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_WEEK_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_EXAM_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_PRACTICE_SCHEDULE},
                    new KeyboardButton[] {MenuMessages.WATCH_CORPUS_INFO},
                    new KeyboardButton[] {MenuMessages.WATCH_HEAD_INFO},

                };
        }

        private ReplyKeyboardMarkup DrawAdminMainMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {MenuMessages.ADMIN_LOAD_SCHEDULE},
                new KeyboardButton[] {MenuMessages.ADMIN_LOAD_HEAD_INFO},
                new KeyboardButton[] { MenuMessages.ADMIN_LOAD_CORPUS_INFO },
                new KeyboardButton[] { MenuMessages.CHOOSE_MENU }
            })
            {
                ResizeKeyboard = true
            };
        }

        public async Task<Message> SendOneDayScheduleAsync(long chatId, List<Lesson> lessons, string groupName, DateTime dateTime)
        {
            string textMessage = null;

            if (dateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                textMessage = "Воскресенье. Нет пар.";
            }
            else
            {
                textMessage = $"Группа: {groupName}{Environment.NewLine}" +
                    $"{dateTime.Date}. {GetDayOfWeekName(dateTime.DayOfWeek)}. {Environment.NewLine}{Environment.NewLine}" +
                    $"{DrawOneDayLessons(lessons)}";
            }

            InlineKeyboardMarkup inlineKeyboard = new(new[]{
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "<-", callbackData: "Yesterday"),
                    InlineKeyboardButton.WithCallbackData(text: "->", callbackData: "Tomorrow"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Вернуться в главное меню", callbackData: "Back"),
                }
            });

            return await _telegramClient.SendTextMessageAsync(
                    chatId,
                    text: textMessage,
                    cancellationToken: _ct,
                    replyMarkup: inlineKeyboard
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
                default:
                    break;
            }

            return "Воскресенье";
        }

        public async Task<Message> SendPracticeInfoAsync(long chatId, Group group)
        {
            string textMessage = $"{group.Name}{Environment.NewLine}{Environment.NewLine}" +
                $"Начало: {group.PracticeDateStart}{Environment.NewLine}" +
                $"Конец: {group.PracticeDateEnd}{Environment.NewLine}" +
                $"Руководитель: {group.PracticeTeacherFullName}";

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: textMessage,
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendExamScheduleAsync(long chatId, Group group, List<Exam>? exams)
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
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard(),
                parseMode: ParseMode.Html
            );
        }

        public async Task<Message> SendChooseMenuAsync(long chatId)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(new[]
            {
                    new KeyboardButton[] {MenuMessages.ENTER_ADMIN_MENU},
                    new KeyboardButton[] {MenuMessages.ENTER_ORD_MENU},
            })
            { 
                ResizeKeyboard = true
            };

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Ваш username находится в списке админов. Какое меню хотите открыть?",
                cancellationToken: _ct,
                replyMarkup: replyMarkup
                );
        }

        public async Task<Message> SendAdminMainMenuAsync(long chatId)
        {
            


            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Выберите пункт меню",
                cancellationToken: _ct,
                replyMarkup: DrawAdminMainMenu()
                );
        }

        public async Task<Message> SendOrdinaryMenuForAdminAsync(long chatId)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(
                DrawMainMenu().Append(new KeyboardButton[] { MenuMessages.CHOOSE_MENU })
                )
            { ResizeKeyboard = true };


            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Выберите пункт меню",
                cancellationToken: _ct,
                replyMarkup: replyMarkup
                );
        }

        public async Task<Message> SendReadyToProcessSchedules(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Отправьте ссылку на расписание учебным групп. (Можно несколько сразу).",
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendWrongFileForSchedule(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Неверный формат ссылки. Ссылка должна начинаться на \"https://timetable.pallada.sibsau.ru/timetable/\"",
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendGroupAddedMessage(long chatId, string groupName)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Новая группа добавлена: " + groupName,
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendScheduleAddedMessage(long chatId, string groupName)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Добавлено расписание для группы " + groupName,
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendReadyToProcessHeadInfo(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Отправьте txt файл с информацией о кафедре.",
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendReadyToProcessCorpusInfo(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Отправьте txt файл с информацией о корпусах.",
                cancellationToken: _ct,
                replyMarkup: DrawBackKeyboard()
                );
        }

        public async Task<Message> SendInfoHeadSuccess(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Информация о кафедре загружена.",
                cancellationToken: _ct,
                replyMarkup: DrawAdminMainMenu()
                );
        }

        public async Task<Message> SendInfoCorpusSuccess(long chatId)
        {
            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: "Информация о корпусах загружена.",
                cancellationToken: _ct,
                replyMarkup: DrawAdminMainMenu()
                );
        }

        public async Task<Message> SendInfo(string text, long chatId, bool isAdmin)
        {
            ReplyKeyboardMarkup replyKeyboard ;

            if (isAdmin)
            {
                replyKeyboard = new ReplyKeyboardMarkup(
                DrawMainMenu().Append(new KeyboardButton[] { MenuMessages.CHOOSE_MENU })
                )
                { ResizeKeyboard = true };
            }
            else
            {
                replyKeyboard = new ReplyKeyboardMarkup(
                DrawMainMenu()
                )
                { ResizeKeyboard = true };
            }

            

            return await _telegramClient.SendTextMessageAsync(
                chatId,
                text: text,
                cancellationToken: _ct,
                replyMarkup: replyKeyboard
                );
        }
    }
}
