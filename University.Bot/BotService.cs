using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using University.BLL;
using University.Common;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.Bot
{
    public class BotService : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private IGroupRepository _groupRepo;
        private ILessonRepository _lessonRepo;
        private IExamRepository _examRepo;
        private ICorpusRepository _corpusRepo;
        private ChatDataController _chatController = new ChatDataController();

        public BotService(
            ITelegramBotClient telegramClient, 
            IGroupRepository groupRepo, 
            ILessonRepository lessonRepo,
            IExamRepository examRepo,
            ICorpusRepository corpusRepo)
        {
            _telegramClient = telegramClient;
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
            _examRepo = examRepo;
            _corpusRepo = corpusRepo;
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
            ChatData? chatData = null;
            if (update.Type == UpdateType.CallbackQuery)
            {
                chatData = _chatController.GetChatDataById(update.CallbackQuery.From.Id);
                _chatController.UpdateChatDataCurrentMenuById(update.CallbackQuery.From.Id, MenuType.MainMenu, chatData);
                await messanger.SendMainMenuAsync(update.CallbackQuery.From.Id);
                return;
            }

            long chatId = update.Message.From.Id; // Проверка новый ли чат
            chatData = _chatController.GetChatDataById(chatId);
            if (chatData is null)
            {
                _chatController.AddNewChatData(chatId);
                chatData = _chatController.GetChatDataById(chatId);
            }
            string text = update.Message.Text; // Текст сообщения



            switch (chatData.CurrentMenu) // Проверка в каком меню должен находится пользователь
            {
                case MenuType.Start: // Отрисовка стартового меню

                    await messanger.SendMainMenuAsync(chatId);

                    _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);

                    break;

                case MenuType.LessonScheduleForToday:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await messanger.SendMainMenuAsync(chatId);

                            break;
                        }
                        // Вернуться в главное меню


                            List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await messanger.GroupIsNotFoundMessageAsync(chatId);
                        }
                        else
                        {
                            List<Lesson> todayLessons = await Task.Run(() => _lessonRepo.GetTodayLessonsByGroupNameAsync(groups.FirstOrDefault().Name).Result);
                            await messanger.SendOneDayScheduleAsync(chatId, todayLessons, groups.FirstOrDefault().Name, DateTime.Now);
                        }

                        break;
                    }


                   

                case MenuType.LessonScheduleForWeek:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await messanger.SendMainMenuAsync(chatId);
                            break;
                        }                        

                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await messanger.GroupIsNotFoundMessageAsync(chatId);
                        }
                        else
                        {
                            int currentWeekParity = WeekParityChecker.GetCurrentWeekParity();

                            List<Lesson> weekLessons = await Task.Run(() => _lessonRepo.GetWeekLessonsByGroupNameAsync(
                                groups.FirstOrDefault().Name,
                                currentWeekParity)
                            .Result);

                            await messanger.SendWeekScheduleAsync(chatId, groups.FirstOrDefault().Name, weekLessons, currentWeekParity);
                        }

                        break;
                    }

                case MenuType.PracticeSchedule:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await messanger.SendMainMenuAsync(chatId);

                            break;
                        }
                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await messanger.GroupIsNotFoundMessageAsync(chatId);
                        }
                        else
                        {
                            Group group = groups.FirstOrDefault();
                            await messanger.SendPracticeInfoAsync(chatId, groups.FirstOrDefault());
                        }

                        break;
                    }

                case MenuType.ExamSchedule:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await messanger.SendMainMenuAsync(chatId);

                            break;
                        }
                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await messanger.GroupIsNotFoundMessageAsync(chatId);
                        }
                        else
                        {
                            List<Exam> exams = await Task.Run(() => _examRepo.GetExamsByGroupName(groups.FirstOrDefault().Name).Result);
                            await messanger.SendExamScheduleAsync(chatId, groups.FirstOrDefault(), exams);
                        }

                        break;
                    }

                    

                case MenuType.MainMenu:

                    switch (text)
                    {
                        case MenuMessages.WATCH_TODAY_SCHEDULE:
                            string groupName = null;

                            if (_chatController.GetGroupNameFromChatData(chatId) is null)
                            {
                                await messanger.SendStartingInsertGroupNameAsync(chatId);

                            }
                            else
                            {
                                List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(_chatController.GetGroupNameFromChatData(chatId)).Result);

                                List<Lesson> todayLessons = await Task.Run(() => _lessonRepo.GetTodayLessonsByGroupNameAsync(groups.FirstOrDefault().Name).Result);
                                await messanger.SendOneDayScheduleAsync(chatId, todayLessons, groups.FirstOrDefault().Name, DateTime.Now);
                            }

                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);


                            break;

                        case MenuMessages.WATCH_WEEK_SCHEDULE:

                            await messanger.SendStartingInsertGroupNameAsync(chatId);
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);

                            break;

                        case MenuMessages.WATCH_PRACTICE_SCHEDULE:

                            await messanger.SendStartingInsertGroupNameAsync(chatId);
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.PracticeSchedule, chatData);

                            break;

                        case MenuMessages.WATCH_EXAM_SCHEDULE:

                            await messanger.SendStartingInsertGroupNameAsync(chatId);
                            _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);

                            break;

                        case MenuMessages.WATCH_CORPUS_INFO:

                            List<Corpus>? corpuses = await Task.Run(() => _corpusRepo.GetAllAsync().Result);

                            await messanger.SendCorpusInfo(chatId, corpuses);
                            await messanger.SendMainMenuAsync(chatId);

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
