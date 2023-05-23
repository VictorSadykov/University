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
using System.Runtime.InteropServices;
using System.Threading;

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
        private Messanger _messanger;

        public BotService(
            ITelegramBotClient telegramClient, 
            IGroupRepository groupRepo, 
            ILessonRepository lessonRepo,
            IExamRepository examRepo,
            ScheduleLoader scheduleLoader,
            Messanger messanger)
        {
            _telegramClient = telegramClient;
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
            _examRepo = examRepo;
            _scheduleController = scheduleLoader;
            _messanger = messanger;
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
            ChatData? chatData = null;

            // TODO: ПЕРЕДЕЛАТЬ КОЛЛБЭК КНОПКИ
            /*if (update.Type == UpdateType.CallbackQuery) 
            {
                chatData = _chatController.GetById(update.CallbackQuery.From.Id);
                _chatController.UpdateCurrentMenuById(update.CallbackQuery.From.Id, MenuType.MainMenu, chatData);
                await _messanger.SendMainMenuAsync(update.CallbackQuery.From.Id);
                return;
            }*/

            long chatId = update.Message.From.Id; // Проверка новый ли чат
            chatData = _chatController.GetById(chatId);
            if (chatData is null)
            {
                _chatController.Add(chatId);
                chatData = _chatController.GetById(chatId);
            }

            string text = update.Message.Text; // Текст сообщения

            bool isUserAdmin = _adminController.IsAdmin(update.Message.From.Username); // проверка есть ли пользователь в списке админов

            switch (chatData.CurrentMenu) // Проверка в каком меню должен находится пользователь
            {
                case MenuType.Start: // Отрисовка стартового меню
                    {

                        if (isUserAdmin) // Если пользователь есть в списке админов, даём выбирать в какое меню входить
                        {
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            await GoToMainMenu(chatId, chatData, isUserAdmin, cancellationToken);
                        }
                        break;
                    }

                case MenuType.LessonScheduleForToday:
                    {
                        /*_chatController

                        if (_adminController.IsAdmin(update.Message.From.Username))
                        {
                            await _messanger.SendOrdinaryMenuForAdminAsync(chatId);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                        }
                        else
                        {
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
                            await _messanger.SendMainMenuAsync(chatId);

                        }
                        break;*/
                        break;
                    }                  

                case MenuType.LessonScheduleForWeek:
                    {
                        /*if (await IfMessageIsBackGoToMainMenu(chatId, chatData, text, isUserAdmin, cancellationToken)) break;                       

                        if (groups is null)
                        {
                            await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                        }
                        else
                        {
                            int currentWeekParity = WeekParityChecker.GetCurrentWeekParity();

                            List<Lesson> weekLessons = await Task.Run(() => _lessonRepo.GetWeekLessonsByGroupNameAsync(
                                groups.FirstOrDefault().Name,
                                currentWeekParity)
                            .Result);

                            await _messanger.SendWeekScheduleAsync(chatId, groups.FirstOrDefault().Name, weekLessons, currentWeekParity, cancellationToken);
                        }*/

                        break;
                    }

                case MenuType.PracticeSchedule:
                    {
                        /*if (await IfMessageIsBackGoToMainMenu(chatId, chatData, text, isUserAdmin, cancellationToken)) break;

                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                        }
                        else
                        {
                            Group group = groups.FirstOrDefault();
                            await _messanger.SendPracticeInfoAsync(chatId, groups.FirstOrDefault(), cancellationToken);
                        }*/

                        break;
                    }

                case MenuType.ExamSchedule:
                    {
                        /*if (await IfMessageIsBackGoToMainMenu(chatId, chatData, text, isUserAdmin, cancellationToken)) break;

                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                        }
                        else
                        {
                            List<Exam> exams = await Task.Run(() => _examRepo.GetExamsByGroupName(groups.FirstOrDefault().Name).Result);
                            await _messanger.SendExamScheduleAsync(chatId, groups.FirstOrDefault(), exams, cancellationToken);
                        }*/

                        break;
                    }

                case MenuType.InsertingGroupName:
                    {
                       /* if (await IfMessageIsBackGoToMainMenu(chatId, chatData, text, isUserAdmin, cancellationToken)) break;

                        List<Group>? groups = await Task.Run(() => _groupRepo.GetAllGroupsByNameAsync(text).Result);

                        if (groups is null)
                        {
                            await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                        }
                        else
                        {
                            _chatController.UpdateSearchQueryName(chatId, text, chatData);

                            switch (chatData.NextMenu)
                            {
                                case MenuType.LessonScheduleForToday:
                                    await GoToTodaySchedule(_messanger, chatId, chatData, text);
                                    _chatController.UpdateNextMenuById(chatId, null, chatData);

                                    break;

                                default:
                                    break;
                            }
                        }*/

                        break;
                    }

                case MenuType.ChooseMenu:
                    {
                        switch (text)
                        {
                            case MenuMessages.ENTER_ADMIN_MENU:
                                await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ENTER_ORD_MENU:
                                await GoToMainMenu(chatId, chatData, isUserAdmin, cancellationToken);
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
                                await StartLoadingSchedule(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ADMIN_LOAD_CORPUS_INFO:
                                await StartLoadingCorpusInfo(chatId, chatData, cancellationToken);                                
                                break;
                            case MenuMessages.ADMIN_LOAD_HEAD_INFO:
                                await StartLoadingHeadInfo(chatId, chatData, cancellationToken);                                
                                break;
                            case MenuMessages.ENTER_CHOOSE_MENU:
                                await GoToChooseMenu(chatId, chatData, cancellationToken);
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
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            if (text.StartsWith("https://timetable.pallada.sibsau.ru/timetable/"))
                            {
                                (string groupName, bool isGroupNew) = await _scheduleController.AddScheduleAsync(text);

                                if (isGroupNew)
                                {
                                    await _messanger.SendGroupAddedMessageAsync(chatId, groupName, cancellationToken);
                                }

                                await _messanger.SendScheduleAddedMessageAsync(chatId, groupName, cancellationToken);
                            }
                            else
                            {
                                await _messanger.SendWrongFileForScheduleAsync(chatId, cancellationToken);

                            }
                        }

                        break;
                    }

                case MenuType.AdminLoadCorpusInfo:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            if (update.Message.Type == MessageType.Document)
                            {
                                string corpusInfoFilePath = "info/corpus.txt";
                                await DownloadFileToInfoFolder(update, corpusInfoFilePath, cancellationToken);
                                await SendCorpusFileLoadedSuccessfully(chatId, chatData, cancellationToken);
                            }
                        }                       

                        break;
                    }

                case MenuType.AdminLoadHeadInfo:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            if (update.Message.Type == MessageType.Document)
                            {
                                string headInfoFilePath = "info/head.txt";
                                await DownloadFileToInfoFolder(update, headInfoFilePath, cancellationToken);
                                await SendHeadFileLoadedSuccessfully(chatId, chatData, cancellationToken);
                            }
                        }

                        break;
                    }

                case MenuType.MainMenu:
                    {
                        switch (text)
                        {
                            case MenuMessages.WATCH_TODAY_SCHEDULE:

                                /*string? groupName = _chatController.GetSearchQueryName(chatId);

                                if (groupName is null)
                                {
                                    await _messanger.SendStartingInsertGroupNameAsync(chatId);
                                    _chatController.UpdateNextMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingGroupName, chatData);
                                }
                                else
                                {
                                    await GoToTodaySchedule(_messanger, chatId, chatData, groupName);
                                }*/

                                /*if (_chatController.GetGroupNameFromChatData(chatId) is null)
                                {
                                    await _messanger.SendStartingInsertGroupNameAsync(chatId);

                                }

                                _chatController.UpdateChatDataCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);*/

                                break;

                            case MenuMessages.WATCH_WEEK_SCHEDULE:

                                /*string? searchQueryName = _chatController.GetSearchQueryName(chatId); // Получаем либо имя группы, либо имя преподавателя
                                // по которым будет производится поиск расписания

                                if (searchQueryName is null)
                                {
                                    _chatController.UpdateNextMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingGroupName, chatData);
                                    await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
                                }

                                await _messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);*/

                                break;

                            case MenuMessages.WATCH_PRACTICE_SCHEDULE:

                               /* await _messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.PracticeSchedule, chatData);*/

                                break;

                            case MenuMessages.WATCH_EXAM_SCHEDULE:

                               /* await _messanger.SendStartingInsertGroupNameAsync(chatId);
                                _chatController.UpdateCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);*/

                                break;

                            case MenuMessages.WATCH_CORPUS_INFO:

                                string corpusMessage = await _infoController.GetCorpusInfo();
                                await SendInfoAndGoToMainMenu(chatId, corpusMessage, chatData, isUserAdmin, cancellationToken);

                                break;

                            case MenuMessages.WATCH_HEAD_INFO:

                                string headMessage = await _infoController.GetHeadInfo();
                                await SendInfoAndGoToMainMenu(chatId, headMessage, chatData, isUserAdmin, cancellationToken);

                                break;

                            case MenuMessages.ENTER_CHOOSE_MENU:

                                if (_adminController.IsAdmin(update.Message.From.Username))
                                {
                                    await GoToChooseMenu(chatId, chatData, cancellationToken);
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


        /*
         ========CHAT CONTROLLER AND MESSANGER COMBINER METHODS===========
         */

        /// <summary>
        /// Обновляет информацию в ChatData про меню в котором находится пользователь, после этого отправляет сообщение главного меню
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="chatData"></param>
        /// <param name="isUserAdmin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task GoToMainMenu(long chatId, ChatData chatData, bool isUserAdmin, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendMainMenuAsync(chatId, isUserAdmin, ct);
        }

        /// <summary>
        /// Переход в главное меню админа
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="chatData"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task GoToAdminMainMenu(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
            await _messanger.SendAdminMainMenuAsync(chatId, ct);
        }

        /// <summary>
        /// Обновляет информацию в ChatData про меню в котором находится пользователь, после этого отправляет сообщение меню выбора 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="chatData"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task GoToChooseMenu(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.ChooseMenu, chatData);
            await _messanger.SendChooseMenuAsync(chatId, ct);            
        }

        /// <summary>
        /// Отправляет информационное сообщение, затем переходит в главное меню
        /// (информационное сообщение - кафедра, корпуса, ссылки)
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="infoTextMessage"></param>
        /// <param name="chatData"></param>
        /// <param name="isUserAdmin"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task SendInfoAndGoToMainMenu(long chatId, string infoTextMessage, ChatData chatData, bool isUserAdmin, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendInfoAsync(chatId, infoTextMessage, isUserAdmin, ct);
        }

        /// <summary>
        /// Проверяет, если пользователь нажал назад, то отправляет в главное меню
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IfMessageIsBackGoToMainMenu(long chatId, ChatData chatData, string text, bool isUserAdmin, CancellationToken ct)
        {
            if (text == MenuMessages.BACK)
            {
                await GoToMainMenu(chatId, chatData, isUserAdmin, ct);
                return true;
            }

            return false;
        }
        public async Task StartLoadingCorpusInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadCorpusInfo, chatData);
            await _messanger.SendReadyToProcessCorpusInfoAsync(chatId, ct);
        }
        public async Task SendCorpusFileLoadedSuccessfully(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendCorpusFileLoadedSuccessfullyAsync(chatId, ct);
        }

        public async Task StartLoadingHeadInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadHeadInfo, chatData);
            await _messanger.SendReadyToProcessHeadInfoAsync(chatId, ct);
        }
        public async Task SendHeadFileLoadedSuccessfully(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendHeadFileLoadedSuccessfullyAsync(chatId, ct);
        }

        public async Task StartLoadingSchedule(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadSchedule, chatData);
            await _messanger.SendReadyToProcessSchedulesAsync(chatId, ct);
        }

        /// <summary>
        /// Скачивает файл с текстом информационного сообщения, переходит в админское меню
        /// (Файл должен быть txt формата)
        /// </summary>
        /// <param name="update"></param>
        /// <param name="chatData"></param>
        /// <param name="infoMessageText"></param>
        /// <param name="infoPath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task DownloadFileToInfoFolder(Update update, string infoFileName, CancellationToken ct)
        {
            var filePath = DataConfig.DATA_FOLDER_PATH + infoFileName;
            await DownloadFileAsync(update.Message.Document, filePath, ct);            
        }


        /*public async Task GoToTodaySchedule(Messanger messanger, long chatId, ChatData chatData, string groupName)
        {
            List<Lesson> todayLessons = await Task.Run(() => _lessonRepo.GetTodayLessonsByGroupNameAsync(groupName).Result);

            await messanger.SendOneDayScheduleAsync(chatId, todayLessons, groupName, DateTime.Now);
            _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
        }*/
        

    }
}
