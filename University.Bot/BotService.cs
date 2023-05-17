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
using System.Drawing.Drawing2D;

namespace University.Bot
{
    public class BotService : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private IGroupRepository _groupRepo;
        private ILessonRepository _lessonRepo;
        private IExamRepository _examRepo;
        private ChatDataController _chatController = new ChatDataController();
        private AdminController _adminController = new AdminController();
        private InfoController _infoController = new InfoController();
        private ScheduleLoader _scheduleController;

        public BotService(
            ITelegramBotClient telegramClient, 
            IGroupRepository groupRepo, 
            ILessonRepository lessonRepo,
            IExamRepository examRepo,
            ScheduleLoader scheduleLoader)
        {
            _telegramClient = telegramClient;
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
            _examRepo = examRepo;
            _scheduleController = scheduleLoader;
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
                chatData = _chatController.GetById(update.CallbackQuery.From.Id);
                _chatController.UpdateCurrentMenuById(update.CallbackQuery.From.Id, MenuType.MainMenu, chatData);
                await messanger.SendMainMenuAsync(update.CallbackQuery.From.Id);
                return;
            }

            long chatId = update.Message.From.Id; // Проверка новый ли чат
            chatData = _chatController.GetById(chatId);
            if (chatData is null)
            {
                _chatController.Add(chatId);
                chatData = _chatController.GetById(chatId);
            }
            string text = update.Message.Text; // Текст сообщения



            switch (chatData.CurrentMenu) // Проверка в каком меню должен находится пользователь
            {
                case MenuType.Start: // Отрисовка стартового меню
                    {

                        if (_adminController.IsAdmin(update.Message.From.Username)) // Проверка является ли пользователь админом
                        {
                           await messanger.SendChooseMenuAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.ChooseMenu, chatData);
                        }
                        else
                        {
                            await messanger.SendMainMenuAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                        }
                        break;
                    }

                case MenuType.LessonScheduleForToday:
                    {
                        if (_adminController.IsAdmin(update.Message.From.Username))
                        {
                            await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                        }
                        else
                        {
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await messanger.SendMainMenuAsync(chatId);

                        }
                        break;
                    }


                   

                case MenuType.LessonScheduleForWeek:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            if (_adminController.IsAdmin(update.Message.From.Username))
                            {
                                await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            }
                            else
                            {
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                                await messanger.SendMainMenuAsync(chatId);

                            }
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
                            if (_adminController.IsAdmin(update.Message.From.Username))
                            {
                                await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            }
                            else
                            {
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                                await messanger.SendMainMenuAsync(chatId);

                            }

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
                            if (_adminController.IsAdmin(update.Message.From.Username))
                            {
                                await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            }
                            else
                            {
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                                await messanger.SendMainMenuAsync(chatId);

                            }

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

                case MenuType.InsertingGroupName:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            if (_adminController.IsAdmin(update.Message.From.Username))
                            {
                                await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            }
                            else
                            {
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                                await messanger.SendMainMenuAsync(chatId);

                            }
                            break;
                        }

                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await messanger.GroupIsNotFoundMessageAsync(chatId);
                        }
                        else
                        {
                            _chatController.UpdateGroupName(chatId, text, chatData);

                            switch (chatData.NextMenu)
                            {
                                case MenuType.LessonScheduleForToday:
                                    await GoToTodaySchedule(messanger, chatId, chatData, text);
                                    _chatController.UpdateNextMenuById(chatId, null, chatData);

                                    break;

                                default:
                                    break;
                            }
                        }

                        break;
                    }

                case MenuType.ChooseMenu:
                    {
                        switch (text)
                        {
                            case MenuMessages.ENTER_ADMIN_MENU:
                                await messanger.SendAdminMainMenuAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                                break;
                            case MenuMessages.ENTER_ORD_MENU:
                                await messanger.SendOrdinaryMenuForAdminAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                                break;

                            default:
                                break;
                        }

                        break;
                    }

                case MenuType.AdminMainMenu:
                    {
                        switch (text)
                        {
                            case MenuMessages.ADMIN_LOAD_SCHEDULE:
                                await messanger.SendReadyToProcessSchedules(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadSchedule, chatData);
                                break;
                            case MenuMessages.ADMIN_LOAD_CORPUS_INFO:
                                await messanger.SendReadyToProcessCorpusInfo(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadCorpusInfo, chatData);
                                break;
                            case MenuMessages.ADMIN_LOAD_HEAD_INFO:
                                await messanger.SendReadyToProcessHeadInfo(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadHeadInfo, chatData);
                                break;
                            case MenuMessages.CHOOSE_MENU:
                                await messanger.SendChooseMenuAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.ChooseMenu, chatData);
                                break;
                            default:
                                break;
                        }
                        break;
                    }

                case MenuType.AdminLoadSchedule:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await messanger.SendAdminMainMenuAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                        }
                        else
                        {
                            if (text.StartsWith("https://timetable.pallada.sibsau.ru/timetable/"))
                            {
                                (string groupName, bool isGroupNew) = await _scheduleController.AddScheduleAsync(text);

                                if (isGroupNew)
                                {
                                    await messanger.SendGroupAddedMessage(chatId, groupName);
                                }

                                await messanger.SendScheduleAddedMessage(chatId, groupName);
                            }
                            else
                            {
                                await messanger.SendWrongFileForSchedule(chatId);

                            }
                        }

                        


                        

                        /*if (update.Message.Type == MessageType.Document)
                        {
                            if (update.Message.Document.FileName.Contains(".pdf"))
                            {
                                Document documentToDownload = update.Message.Document;
                                string pathDownload = DataConfig.DATA_FOLDER_PATH + "/schedules/PDF/" + documentToDownload.FileName;

                                await DownloadFileAsync(documentToDownload, pathDownload, cancellationToken);

                                await _scheduleController.AddScheduleAsync(pathDownload);
                            }
                            else
                            {
                                await messanger.SendWrongFileForSchedule(chatId);
                            }
                        }*/

                        break;
                    }

                case MenuType.AdminLoadCorpusInfo:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await messanger.SendAdminMainMenuAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                        }
                        else
                        {
                            if (update.Message.Type == MessageType.Document)
                            {
                                var fileInfo = await _telegramClient.GetFileAsync(update.Message.Document.FileId);

                                var filePath = DataConfig.DATA_FOLDER_PATH + "info/corpus.txt";

                                await DownloadFileAsync(update.Message.Document, filePath, cancellationToken);

                                await messanger.SendInfoCorpusSuccess(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                            }
                        }

                        

                        break;
                    }

                case MenuType.AdminLoadHeadInfo:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await messanger.SendAdminMainMenuAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                        }
                        else
                        {
                            if (update.Message.Type == MessageType.Document)
                            {
                                var fileInfo = await _telegramClient.GetFileAsync(update.Message.Document.FileId);

                                var filePath = DataConfig.DATA_FOLDER_PATH + "info/head.txt";

                                await DownloadFileAsync(update.Message.Document, filePath, cancellationToken);

                                await messanger.SendInfoHeadSuccess(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
                            }
                        }



                        break;
                    }

                case MenuType.MainMenu:
                    {
                        switch (text)
                        {
                            case MenuMessages.WATCH_TODAY_SCHEDULE:

                                string? groupName = _chatController.GetGroupName(chatId);

                                if (groupName is null)
                                {
                                    await messanger.SendStartingInsertGroupNameAsync(chatId);
                                    _chatController.UpdateNextMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingGroupName, chatData);
                                }
                                else
                                {
                                    await GoToTodaySchedule(messanger, chatId, chatData, groupName);
                                }

                                /*if (_chatController.GetGroupNameFromChatData(chatId) is null)
                                {
                                    await messanger.SendStartingInsertGroupNameAsync(chatId);

                                }

                                _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);*/

                                break;

                            case MenuMessages.WATCH_WEEK_SCHEDULE:



                                await messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);

                                break;

                            case MenuMessages.WATCH_PRACTICE_SCHEDULE:

                                await messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.PracticeSchedule, chatData);

                                break;

                            case MenuMessages.WATCH_EXAM_SCHEDULE:

                                await messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);

                                break;

                            case MenuMessages.WATCH_CORPUS_INFO:

                                string corpusMessage = await _infoController.GetCorpusInfo();
                                await messanger.SendInfo(corpusMessage, chatId, _adminController.IsAdmin(update.Message.From.Username));
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);

                                break;

                            case MenuMessages.WATCH_HEAD_INFO:
                                string headMessage = await _infoController.GetHeadInfo();
                                await messanger.SendInfo(headMessage, chatId, _adminController.IsAdmin(update.Message.From.Username));
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);


                                break;
                            case MenuMessages.CHOOSE_MENU:

                                if (_adminController.IsAdmin(update.Message.From.Username))
                                {
                                    await messanger.SendChooseMenuAsync(chatId);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.ChooseMenu, chatData);
                                }

                                break;
                            default:
                                break;
                        }

                        break;                        
                    }
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

        public async Task GoToMainMenu(Messanger messager, long chatId, ChatData chatData)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await messager.SendMainMenuAsync(chatId);
        }

        public async Task GoToTodaySchedule(Messanger messanger, long chatId, ChatData chatData, string groupName)
        {
            List<Lesson> todayLessons = await Task.Run(() => _lessonRepo.GetTodayLessonsByGroupNameAsync(groupName).Result);

            await messanger.SendOneDayScheduleAsync(chatId, todayLessons, groupName, DateTime.Now);
            _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
        }
        public async Task DownloadFileAsync(Document document, string path, CancellationToken cancellationToken)
        {
            Telegram.Bot.Types.File file = await _telegramClient.GetFileAsync(document.FileId, cancellationToken);
            

            using (Stream fileStream = new FileStream(path, FileMode.Create))
            {
                await _telegramClient.DownloadFileAsync(
                filePath: file.FilePath,
                destination: fileStream,
                cancellationToken: cancellationToken
                    );
            }
        }
    }
}
