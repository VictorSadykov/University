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
using University.MiniMethods;

namespace University.Bot
{
    public class BotService : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private IGroupRepository _groupRepo;
        private ILessonRepository _lessonRepo;
        private ITeacherRepository _teacherRepo;
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
            ITeacherRepository teacherRepo,
            ScheduleLoader scheduleLoader,
            Messanger messanger)
        {
            _telegramClient = telegramClient;
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
            _examRepo = examRepo;
            _scheduleController = scheduleLoader;
            _messanger = messanger;
            _teacherRepo = teacherRepo;
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
            long chatId;
            // TODO: ПЕРЕДЕЛАТЬ КОЛЛБЭК КНОПКИ
            if (update.Type == UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
                chatData = _chatController.GetById(chatId);
                string callBackData = update.CallbackQuery.Data;
                bool isCallbackUserAdmin = _adminController.IsAdmin(update.CallbackQuery.From.Username);
                bool isCallbackNullEntity = _chatController.GetSearchQueryName(chatId) is null;
                bool isCallbackEntityTeacher = _chatController.IsEntityTeacher(chatId);
                switch (chatData.CurrentMenu)
                {
                    case MenuType.ExamSchedule:
                        if (callBackData == "back")
                        {
                            _chatController.UpdateNextMenuById(chatId, null, chatData);
                            await GoToMainMenu(chatId, chatData, isCallbackUserAdmin, isCallbackEntityTeacher, isCallbackNullEntity, cancellationToken);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingGroupName:
                        if (callBackData == "backToMenu")
                        {
                            _chatController.UpdateNextMenuById(chatId, null, chatData);
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingGroupCode:
                        if (callBackData == "back")
                        {
                            await StartFillingGroupInfo(chatId, chatData, cancellationToken);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingPracticeTeacherFullName:
                        if (callBackData == "back")
                        {
                            await StartFillingGroupInfo(chatId, chatData, cancellationToken);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingGroupSpecialization:
                        if (callBackData == "back")
                        {
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _messanger.StartInsertingGroupCode(chatId, group.Name, cancellationToken);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupCode, chatData);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingPracticeStartDate:
                        if (callBackData == "back")
                        {
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _messanger.StartInsertingPracticeTeacherFullName(chatId, group.Name, cancellationToken);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingPracticeTeacherFullName, chatData);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingGroupOrientation:
                        if (callBackData == "back")
                        {
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _messanger.StartInsertingGroupSpecialization(chatId, group.Name, group.Code, cancellationToken);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupSpecialization, chatData);
                            return;
                        }
                        break;
                    case MenuType.AdminInsertingPracticeEndDate:
                        if (callBackData == "back")
                        {
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _messanger.StartInsertingPracticeStartDate(chatId, group.Name, group.PracticeTeacherFullName, cancellationToken);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingPracticeStartDate, chatData);
                            return;
                        }
                        break;
                    case MenuType.InsertingEntityName:
                        {
                            if (callBackData == "backToMenu")
                            {
                                await GoToMainMenu(chatId, chatData, isCallbackUserAdmin, isCallbackEntityTeacher, isCallbackNullEntity, cancellationToken);
                                return;
                            }
                            else if (int.TryParse(callBackData, out int result))
                            {
                                Teacher teacher = await _teacherRepo.FindByIdAsync(int.Parse(callBackData));
                                string teacherFullName = $"{teacher.LastName} {teacher.FirstName}. {teacher.SecondName}.";
                                _chatController.UpdateSearchQueryName(chatId, teacherFullName, chatData);
                                _chatController.UpdateIsEntityGroupFlagById(chatId, false, chatData);
                                chatData.CurrentMenu = chatData.NextMenu;

                                switch (chatData.CurrentMenu)
                                {
                                    case MenuType.LessonScheduleForWeek:
                                        await _messanger.SendWeekParityKeyboard(chatId, cancellationToken);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.WeekParityInsert, chatData);
                                        break;
                                    case MenuType.LessonScheduleForToday:
                                        var workingDays = _teacherRepo.GetWorkingDays(teacherFullName);
                                        var weekParity = WeekParityChecker.GetCurrentWeekParity();
                                        List<Lesson> dayLessons = await _lessonRepo.GetOneDayLessonsByTeacherFullNameAsync(teacherFullName, weekParity, DateTime.Now.DayOfWeek);
                                        await _messanger.SendOneDayScheduleAsync(chatId, teacherFullName, false, workingDays.Count, dayLessons, DateTime.Now, weekParity, cancellationToken);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
                                        break;
                                }
                            }

                            
                            break;
                        }

                    case MenuType.WeekParityInsert:
                        {
                            if (callBackData == "backToMenu")
                            {
                                await GoToMainMenu(chatId, chatData, isCallbackUserAdmin, isCallbackEntityTeacher, isCallbackNullEntity, cancellationToken);
                                return;
                            }
                            else if (int.TryParse(callBackData, out int result))
                            {
                                int weekParity = int.Parse(callBackData);

                                ChatData foundChatData = _chatController.GetById(chatId);

                                _chatController.UpdateNextMenuById(chatId, null, chatData);

                                string foundEntityName = foundChatData.SearchQueryName;
                                List<Lesson> lessons;
                                if (foundChatData.isEntityGroup)
                                {
                                    lessons = await _lessonRepo.GetWeekLessonsByGroupNameAsync(foundEntityName, weekParity);
                                }
                                else
                                {
                                    lessons = await _lessonRepo.GetWeekLessonsByTeacherFullNameAsync(foundEntityName, weekParity);
                                }

                                _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);


                                await _messanger.SendWeekScheduleAsync(
                                    chatId,
                                    foundEntityName,
                                    foundChatData.isEntityGroup,
                                    lessons,
                                    weekParity,
                                    cancellationToken);
                            }

                            

                            break;
                        }

                    case MenuType.LessonScheduleForWeek:
                        if (callBackData == "back")
                        {
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.WeekParityInsert, chatData);
                            await _messanger.SendWeekParityKeyboard(chatId, cancellationToken);
                        }
                        break;
                    case MenuType.LessonScheduleForToday:
                        if (callBackData == "back")
                        {
                            await GoToMainMenu(chatId, chatData, isCallbackUserAdmin, isCallbackEntityTeacher, isCallbackNullEntity, cancellationToken);
                        }
                        else if (callBackData == "prev" || callBackData == "next")
                        {
                            List<(int weekNumber, DayOfWeek dayOfWeek)> days;
                            if (chatData.isEntityGroup)
                            {
                                days = _groupRepo.GetWorkingDays(chatData.SearchQueryName);

                            }
                            else
                            {
                                days = _teacherRepo.GetWorkingDays(chatData.SearchQueryName);
                            }

                            int? currentWeekNumber = chatData.CurrentWeekParity;
                            DayOfWeek? currentDayOfWeek = chatData.CurrentScheduleDay;


                            List<(int weekNumber, DayOfWeek dayOfWeek, bool isDayWorking)> daysInfo = new List<(int, DayOfWeek, bool)>();

                            for (int weekNumber = 1; weekNumber <= 2; weekNumber++)
                            {
                                for (int dayOfWeekNumber = 1; dayOfWeekNumber <= 7; dayOfWeekNumber++)
                                {
                                    (int weekNumber, DayOfWeek dayOfWeek, bool isDayWorking) dayToAdd = (weekNumber, (DayOfWeek)dayOfWeekNumber, false);

                                    foreach (var item in days)
                                    {
                                        if ((int)item.dayOfWeek == dayOfWeekNumber && item.weekNumber == weekNumber)
                                        {
                                            dayToAdd.isDayWorking = true;
                                        }
                                    }

                                    daysInfo.Add(dayToAdd);
                                }
                            }

                            int index = daysInfo.FindIndex(x => x.weekNumber == currentWeekNumber && x.dayOfWeek == currentDayOfWeek);
                            int? dayOffset = chatData.DayOffset ?? 0;
                            if (callBackData == "prev")
                            {
                                do
                                {
                                    index--;
                                    if (index < 0)
                                    {
                                        index = daysInfo.Count - 1;

                                    }
                                    dayOffset--;


                                } while (daysInfo[index].isDayWorking == false);

                                
                            }
                            else if (callBackData == "next")
                            {

                                do
                                {
                                    index++;
                                    if (index > daysInfo.Count - 1)
                                    {
                                        index = 0;
                                    }

                                    dayOffset++;

                                } while (daysInfo[index].isDayWorking == false);                                
                            }


                            _chatController.UpdateDayOffset(chatId, dayOffset, chatData);
                            (int newWeekNumber, DayOfWeek newWeekDay) = (daysInfo[index].weekNumber, daysInfo[index].dayOfWeek);
                            await LoadOneDaySchedule(chatId, chatData, newWeekDay, newWeekNumber, DateTime.Now.AddDays((double)dayOffset), cancellationToken);
                        }
                        break;

                }

                return;
            }

            chatId = update.Message.From.Id; // Проверка новый ли чат
            chatData = _chatController.GetById(chatId);
            if (chatData is null)
            {
                _chatController.Add(chatId);
                chatData = _chatController.GetById(chatId);
            }

            string text = update.Message.Text; // Текст сообщения

            bool isUserAdmin = _adminController.IsAdmin(update.Message.From.Username); // проверка есть ли пользователь в списке админов
            bool isEntityTeacher = _chatController.IsEntityTeacher(chatId);
            bool isNullEntity = _chatController.GetSearchQueryName(chatId) is null;

            switch (chatData.CurrentMenu) // Проверка в каком меню должен находится пользователь
            {
                case MenuType.Start: // Отрисовка стартового меню
                    {

                        if (isUserAdmin) // Если пользователь есть в списке админов, даём выбирать в какое меню входить
                        {
                            await GoToChooseMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                        }
                        break;
                    }

                case MenuType.InsertingEntityName:
                    {
                        if (await IfMessageIsBackGoToMainMenu(chatId, chatData, text, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken)) break;

                        
                        bool isStringGroupName = GroupNameAnalyser.DefineIsStringGroupName(text);
                        Group? group = null;
                        Teacher? teacher = null;
                        MenuType? nextMenu = _chatController.GetById(chatId).NextMenu;
                        int weekParity = WeekParityChecker.GetCurrentWeekParity();
                        if (isStringGroupName || nextMenu == MenuType.PracticeSchedule || nextMenu == MenuType.GroupInfo) // Если ввели название группы, то ведётся поиск по группам
                        {
                            group = _groupRepo.FindByName(text);

                            if (group is null)
                            {
                                if (nextMenu == MenuType.PracticeSchedule || nextMenu == MenuType.GroupInfo)
                                {
                                    await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                                }
                                else
                                {
                                    await _messanger.GroupOrTeacherIsNotFoundMessageAsync(chatId, cancellationToken);
                                }
                                return;
                            }
                            else
                            {
                                _chatController.UpdateIsEntityGroupFlagById(chatId, isStringGroupName, chatData);
                                _chatController.UpdateSearchQueryName(chatId, group.Name, chatData);
                                
                            }
                            
                        }
                        else // Поиск по учителям
                        {
                            bool isStringOnlyLastname = NameAnalyser.IsStringIsOnlyLastName(text);

                            if (isStringOnlyLastname) // Если введется поиск расписания только по фамилии учителя
                            {
                                List<Teacher> allTeachersWithSameLastName = _teacherRepo.FindAllByLastName(text);

                                if (allTeachersWithSameLastName.Count == 0)
                                {
                                    await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                                    return;
                                }
                                else if (allTeachersWithSameLastName.Count == 1)
                                {
                                    teacher = allTeachersWithSameLastName[0];
                                    string fullName = NameAnalyser.CombineToFullName(teacher.FirstName, teacher.LastName, teacher.SecondName);
                                    _chatController.UpdateSearchQueryName(chatId, fullName, chatData);
                                    _chatController.UpdateIsEntityGroupFlagById(chatId, isStringGroupName, chatData);
                                }
                                else
                                {
                                    await _messanger.SendTeacherVariants(chatId, allTeachersWithSameLastName, cancellationToken);
                                    return;
                                }
                            }
                            else // Если ведется поиск расписания только по инициалам учителя
                            {
                                teacher = _teacherRepo.FindByFullName(text);
                                if (teacher is null)
                                {
                                    await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                                }
                                else
                                {
                                    string fullName = NameAnalyser.CombineToFullName(teacher.FirstName, teacher.LastName, teacher.SecondName);
                                    _chatController.UpdateIsEntityGroupFlagById(chatId, isStringGroupName, chatData);
                                    _chatController.UpdateSearchQueryName(chatId, fullName, chatData);
                                  
                                }
                            }
                        }
                        isEntityTeacher = !isStringGroupName;
                        isNullEntity = false;

                        switch (nextMenu)
                        {
                            case MenuType.LessonScheduleForWeek:
                                {
                                    await _messanger.SendWeekParityKeyboard(chatId, cancellationToken);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.WeekParityInsert, chatData);
                                    break;
                                }
                            case MenuType.LessonScheduleForToday:
                                {
                                    List<Lesson> dayLessons;
                                    string entityName = _chatController.GetSearchQueryName(chatId);
                                    int workingDaysCount = 0;
                                    if (isStringGroupName)
                                    {
                                        dayLessons = await _lessonRepo.GetDayLessonsByGroupNameAsync(entityName, weekParity, DateTime.Now.DayOfWeek);
                                        workingDaysCount = _groupRepo.GetWorkingDays(entityName).Count;
                                    }
                                    else
                                    {
                                        dayLessons = await _lessonRepo.GetOneDayLessonsByTeacherFullNameAsync(entityName, weekParity, DateTime.Now.DayOfWeek);
                                        workingDaysCount = _teacherRepo.GetWorkingDays(entityName).Count;

                                    }
                                    await _messanger.SendOneDayScheduleAsync(chatId, entityName, isStringGroupName, workingDaysCount, dayLessons, DateTime.Now, weekParity, cancellationToken);
                                    _chatController.UpdateNextMenuById(chatId, null, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
                                    break;
                                }
                            case MenuType.ExamSchedule:
                                {
                                    string entityName = _chatController.GetSearchQueryName(chatId);
                                    List<Exam> exams;
                                    if (isStringGroupName)
                                    {
                                        exams = await _examRepo.GetExamsByGroupNameAsync(entityName);
                                    }
                                    else
                                    {
                                        exams = await _examRepo.GetExamsByTeacherFullNameAsync(entityName);
                                    }
                                    await _messanger.SendExamScheduleAsync(chatId, entityName, isStringGroupName, exams, cancellationToken);
                                    _chatController.UpdateNextMenuById(chatId, null, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);
                                    break;
                                }
                            case MenuType.PracticeSchedule:
                                {
                                    string? practiceBossName = group.PracticeTeacherFullName;
                                    DateTime? practiceDateStart = group.PracticeDateStart;
                                    DateTime? practiceDateEnd = group.PracticeDateEnd;

                                    if (practiceBossName is null || practiceDateStart is null || practiceDateEnd is null)
                                    {
                                        await _messanger.SendGroupInfoIsNotYetFilledMessageAsync(chatId, cancellationToken);
                                        _chatController.UpdateNextMenuById(chatId, null, chatData);
                                        await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                    }
                                    else
                                    {
                                        await _messanger.SendGroupPracticeInfoAsync(chatId, practiceBossName, practiceDateStart, practiceDateEnd, cancellationToken);
                                        _chatController.UpdateNextMenuById(chatId, null, chatData);
                                        await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                    }

                                    break;
                                }
                            case MenuType.GroupInfo:
                                {
                                    string groupName = group.Name;
                                    string? groupCode = group.Code;
                                    string? groupSpecialization = group.Specialization;
                                    string? groupOrientation = group.Orientation;

                                    if (groupCode is null || groupSpecialization is null || groupOrientation is null)
                                    {
                                        await _messanger.SendGroupInfoIsNotYetFilledMessageAsync(chatId, cancellationToken);
                                        _chatController.UpdateNextMenuById(chatId, null, chatData);
                                        await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                    }
                                    else
                                    {
                                        await _messanger.SendGroupInfoAsync(chatId, groupName, groupCode, groupSpecialization, groupOrientation, cancellationToken);
                                        _chatController.UpdateNextMenuById(chatId, null, chatData);
                                        await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                    }

                                    break;
                                }
                            default:
                                break;
                        }
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
                                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
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
                            case MenuMessages.ADMIN_LOAD_EXAMS:
                                await StartLoadingExams(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ADMIN_LOAD_CORPUS_INFO:
                                await StartLoadingCorpusInfo(chatId, chatData, cancellationToken);                                
                                break;
                            case MenuMessages.ADMIN_LOAD_HEAD_INFO:
                                await StartLoadingHeadInfo(chatId, chatData, cancellationToken);                                
                                break;
                            case MenuMessages.ADMIN_LOAD_LINKS_INFO:
                                await StartLoadingLinksInfo(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ADMIN_FILL_GROUP_INFO:
                                await StartFillingGroupInfo(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ADMIN_FILL_GROUP_PRACTICE_INFO:
                                await StartFillingGroupPracticeInfo(chatId, chatData, cancellationToken);
                                break;
                            case MenuMessages.ENTER_CHOOSE_MENU:
                                await GoToChooseMenu(chatId, chatData, cancellationToken);
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case MenuType.AdminInsertingGroupName:
                    {
                        text = text.Trim();

                        Group? group = _groupRepo.FindByName(text);
                        if (group == null)
                        {
                            await _messanger.GroupIsNotFoundMessageAsync(chatId, cancellationToken);
                        }
                        else
                        {
                            switch (chatData.NextMenu)
                            {
                                case MenuType.AdminInsertingGroupCode:
                                    await _messanger.StartInsertingGroupCode(chatId, group.Name, cancellationToken);
                                    _chatController.UpdateAdminCurrentEditingGroupName(chatId, group.Name, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupCode, chatData);
                                    break;
                                case MenuType.AdminInsertingPracticeTeacherFullName:
                                    await _messanger.StartInsertingPracticeTeacherFullName(chatId, group.Name, cancellationToken);
                                    _chatController.UpdateAdminCurrentEditingGroupName(chatId, group.Name, chatData);
                                    _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingPracticeTeacherFullName, chatData);
                                    break;
                                default:
                                    break;
                            }
                        }

                        break;
                    }
                case MenuType.AdminInsertingGroupCode:
                    {
                        text = text.Trim();
                        Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                        await _groupRepo.UpdateCodeAsync(group, text);
                        await _messanger.StartInsertingGroupSpecialization(chatId, group.Name, text, cancellationToken);
                        _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupSpecialization, chatData);
                        break;
                    }
                case MenuType.AdminInsertingPracticeTeacherFullName:
                    {
                        text = text.Trim();
                        Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                        await _groupRepo.UpdatePracticeTeacherFullNameAsync(group, text);
                        await _messanger.StartInsertingPracticeStartDate(chatId, group.Name, text, cancellationToken);
                        _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingPracticeStartDate, chatData);
                        break;
                    }
                case MenuType.AdminInsertingGroupSpecialization:
                    {
                        text = text.Trim();
                        Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                        await _groupRepo.UpdateSpecializationAsync(group, text);
                        await _messanger.StartInsertingGroupOrientation(chatId, group.Name, group.Code, text, cancellationToken);
                        _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupOrientation, chatData);
                        break;
                    }

                case MenuType.AdminInsertingPracticeStartDate:
                    {
                        try
                        {
                            text = text.Trim();
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _groupRepo.UpdatePracticeStartDateAsync(group, text);
                            await _messanger.StartInsertingPracticeEndDate(chatId, group.Name, group.PracticeTeacherFullName, text, cancellationToken);
                            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingPracticeEndDate, chatData);
                        }
                        catch (Exception)
                        {
                            await _messanger.WrongDateInsertedMessage(chatId, cancellationToken);
                        }
                        break;
                    }
                case MenuType.AdminInsertingGroupOrientation:
                    {
                        text = text.Trim();
                        Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                        await _groupRepo.UpdateOrientationAsync(group, text);
                        await _messanger.GroupInfoSavedAsync(chatId, group.Name, group.Code, group.Specialization, text, cancellationToken);
                        _chatController.UpdateAdminCurrentEditingGroupName(chatId, null, chatData);
                        _chatController.UpdateNextMenuById(chatId, null, chatData);
                        await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        break;
                    }
                case MenuType.AdminInsertingPracticeEndDate:
                    {
                        try
                        {
                            text = text.Trim();
                            Group group = _groupRepo.FindByName(chatData.AdminCurrentGroupEditingName);
                            await _groupRepo.UpdatePracticeEndDateAsync(group, text);
                            await _messanger.GroupPracticeInfoSavedAsync(chatId, group.Name, group.PracticeTeacherFullName, group.PracticeDateStart.Value.ToString("dd:MM:YYYY"), text, cancellationToken);
                            _chatController.UpdateAdminCurrentEditingGroupName(chatId, null, chatData);
                            _chatController.UpdateNextMenuById(chatId, null, chatData);
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        catch (Exception)
                        {
                            await _messanger.WrongDateInsertedMessage(chatId, cancellationToken);
                        }                        
                        break;
                    }

                case MenuType.AdminLoadExams:
                    {
                        if (text == MenuMessages.BACK)
                        {
                            await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                        }
                        else
                        {
                            if (text.StartsWith("https://timetable.pallada.sibsau.ru/timetable/"))
                            {
                                (string groupName, bool isGroupNew) = await _scheduleController.AddExamScheduleAsync(text);

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
                                (string entityName, bool isEntityNew) = await _scheduleController.AddScheduleAsync(text);

                                if (isEntityNew)
                                {
                                    await _messanger.SendGroupAddedMessageAsync(chatId, entityName, cancellationToken);
                                }

                                await _messanger.SendScheduleAddedMessageAsync(chatId, entityName, cancellationToken);
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

                case MenuType.AdminLoadLinksInfo:
                    if (text == MenuMessages.BACK)
                    {
                        await GoToAdminMainMenu(chatId, chatData, cancellationToken);
                    }
                    else
                    {
                        if (update.Message.Type == MessageType.Document)
                        {
                            string corpusInfoFilePath = "info/links.txt";
                            await DownloadFileToInfoFolder(update, corpusInfoFilePath, cancellationToken);
                            await SendLinksFileLoadedSuccessfully(chatId, chatData, cancellationToken);
                        }
                    }

                    break;

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
                                {
                                    _chatController.UpdateDayOffset(chatId, 0, chatData);
                                    await LoadOneDaySchedule(chatId, chatData, DateTime.Now.DayOfWeek, WeekParityChecker.GetCurrentWeekParity(), DateTime.Now, cancellationToken);
                                    break;
                                }



                            case MenuMessages.WATCH_WEEK_SCHEDULE:
                                {
                                    string? searchQueryName = _chatController.GetSearchQueryName(chatId);
                                    if (searchQueryName is null)
                                    {
                                        _chatController.UpdateNextMenuById(chatId, MenuType.LessonScheduleForWeek, chatData);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                                        await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
                                    }
                                    else
                                    {
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.WeekParityInsert, chatData);

                                        await _messanger.SendWeekParityKeyboard(chatId, cancellationToken);
                                    }

                                    break;
                                }

                            case MenuMessages.WATCH_EXAM_SCHEDULE:
                                {
                                    string? searchQueryName = _chatController.GetSearchQueryName(chatId);
                                    if (searchQueryName is null)
                                    {
                                        _chatController.UpdateNextMenuById(chatId, MenuType.ExamSchedule, chatData);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                                        await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
                                    }
                                    else
                                    {
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);

                                        List<Exam> exams;
                                        if (isEntityTeacher)
                                        {
                                            exams = await _examRepo.GetExamsByTeacherFullNameAsync(searchQueryName);
                                        }
                                        else
                                        {
                                            exams = await _examRepo.GetExamsByGroupNameAsync(searchQueryName);
                                        }
                                        await _messanger.SendExamScheduleAsync(chatId, searchQueryName, isEntityTeacher, exams, cancellationToken);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.ExamSchedule, chatData);
                                    }

                                    break;
                                }

                            case MenuMessages.WATCH_PRACTICE_SCHEDULE:
                                {
                                    string? searchQueryName = _chatController.GetSearchQueryName(chatId);
                                    if (searchQueryName is null)
                                    {
                                        _chatController.UpdateNextMenuById(chatId, MenuType.PracticeSchedule, chatData);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                                        await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
                                    }
                                    else
                                    {
                                        if (chatData.isEntityGroup == true)
                                        {
                                            Group group = _groupRepo.FindByName(searchQueryName);
                                            string? practiceBossName = group.PracticeTeacherFullName;
                                            DateTime? practiceDateStart = group.PracticeDateStart;
                                            DateTime? practiceDateEnd = group.PracticeDateEnd;

                                            if (practiceBossName is null || practiceDateStart is null || practiceDateEnd is null)
                                            {
                                                await _messanger.SendGroupInfoIsNotYetFilledMessageAsync(chatId, cancellationToken);
                                                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                            }
                                            else
                                            {
                                                await _messanger.SendGroupPracticeInfoAsync(chatId, practiceBossName, practiceDateStart, practiceDateEnd, cancellationToken);
                                                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                            }
                                        }
                                    }

                                    break;
                                }
                            case MenuMessages.WATCH_GROUP_INFO:
                                {
                                    string? searchQueryName = _chatController.GetSearchQueryName(chatId);
                                    if (searchQueryName is null)
                                    {
                                        _chatController.UpdateNextMenuById(chatId, MenuType.GroupInfo, chatData);
                                        _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                                        await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
                                    }
                                    else
                                    {
                                        if (chatData.isEntityGroup == true)
                                        {
                                            Group group = _groupRepo.FindByName(searchQueryName);
                                            string groupName = group.Name;
                                            string? groupCode = group.Code;
                                            string? groupSpecialization = group.Specialization;
                                            string? groupOrientation = group.Orientation;

                                            if (groupCode is null || groupSpecialization is null || groupOrientation is null)
                                            {
                                                await _messanger.SendGroupInfoIsNotYetFilledMessageAsync(chatId, cancellationToken);
                                                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                            }
                                            else
                                            {
                                                await _messanger.SendGroupInfoAsync(chatId, groupName, groupCode, groupSpecialization, groupOrientation, cancellationToken);
                                                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);
                                            }

                                            break;
                                        }
                                    }

                                    break;
                                }

                            case MenuMessages.WATCH_CORPUS_INFO:

                                string corpusMessage = await _infoController.GetCorpusInfo();
                                await SendInfoAndGoToMainMenu(chatId, corpusMessage, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);

                                break;

                            case MenuMessages.WATCH_HEAD_INFO:

                                string headMessage = await _infoController.GetHeadInfo();
                                await SendInfoAndGoToMainMenu(chatId, headMessage, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);

                                break;

                            case MenuMessages.WATCH_LINKS_INFO:

                                string linksMessage = await _infoController.GetLinksInfo();
                                await SendInfoAndGoToMainMenu(chatId, linksMessage, chatData, isUserAdmin, isEntityTeacher, isNullEntity, cancellationToken);

                                break;

                            case MenuMessages.ENTER_CHOOSE_MENU:

                                if (isUserAdmin)
                                {
                                    await GoToChooseMenu(chatId, chatData, cancellationToken);
                                }

                                break;
                            case MenuMessages.RESET_SEARCH_QUERY:
                                if (!isNullEntity)
                                {
                                    _chatController.UpdateSearchQueryName(chatId, null, chatData);
                                    _chatController.UpdateIsEntityGroupFlagById(chatId, false, chatData);
                                    await _messanger.SendSearchQueryResetedSuccessfully(chatId, isUserAdmin, isEntityTeacher, !isNullEntity, cancellationToken);
                                }
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

        public async Task LoadOneDaySchedule(long chatId, ChatData chatData, DayOfWeek dayOfWeek, int weekParity, DateTime dateTime, CancellationToken cancellationToken)
        {
            string? searchQueryName = _chatController.GetSearchQueryName(chatId);
            _chatController.UpdateDayScheduleById(chatId, dayOfWeek, chatData);
            _chatController.UpdateWeekParityById(chatId, weekParity, chatData);
            if (searchQueryName is null)
            {
                _chatController.UpdateNextMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
                _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                await _messanger.StartInsertingSearchQueryAsync(chatId, cancellationToken);
            }
            else
            {
                bool isEntityGroup = chatData.isEntityGroup;
                string entityName;
                int workingDaysCount = 0;
                List<Lesson> dayLessons = new List<Lesson>();
                if (isEntityGroup)
                {
                    Group? group = _groupRepo.FindByName(searchQueryName);
                    dayLessons = await _lessonRepo.GetDayLessonsByGroupNameAsync(group.Name, weekParity, dayOfWeek);
                    entityName = group.Name;
                    workingDaysCount = _groupRepo.GetWorkingDays(entityName).Count;
                }
                else
                {
                    Teacher teacher = _teacherRepo.FindByFullName(searchQueryName);
                    string fullName = NameAnalyser.CombineToFullName(teacher.FirstName, teacher.LastName, teacher.SecondName);
                    dayLessons = await _lessonRepo.GetOneDayLessonsByTeacherFullNameAsync(fullName, weekParity, dayOfWeek);
                    entityName = fullName;
                    workingDaysCount = _teacherRepo.GetWorkingDays(entityName).Count;
                }

                await _messanger.SendOneDayScheduleAsync(chatId, entityName, isEntityGroup, workingDaysCount, dayLessons, dateTime, weekParity, cancellationToken);
                _chatController.UpdateCurrentMenuById(chatId, MenuType.LessonScheduleForToday, chatData);
            }
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
        public async Task GoToMainMenu(long chatId, ChatData chatData, bool isUserAdmin, bool isEntityTeacher, bool isNullEntity, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendMainMenuAsync(chatId, isUserAdmin, isEntityTeacher, isNullEntity, ct);
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
        public async Task SendInfoAndGoToMainMenu(long chatId, string infoTextMessage, ChatData chatData, bool isUserAdmin, bool isEntityTeacher, bool isNullEntity, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.MainMenu, chatData);
            await _messanger.SendInfoAsync(chatId, infoTextMessage, isUserAdmin, isEntityTeacher, isNullEntity, ct);
        }

        /// <summary>
        /// Проверяет, если пользователь нажал назад, то отправляет в главное меню
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IfMessageIsBackGoToMainMenu(long chatId, ChatData chatData, string text, bool isUserAdmin, bool isEntityTeacher, bool isNullEntity, CancellationToken ct)
        {
            if (text == MenuMessages.BACK)
            {
                await GoToMainMenu(chatId, chatData, isUserAdmin, isEntityTeacher, isNullEntity, ct);
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
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
            await _messanger.SendCorpusFileLoadedSuccessfullyAsync(chatId, ct);
        }

        public async Task StartLoadingHeadInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadHeadInfo, chatData);
            await _messanger.SendReadyToProcessHeadInfoAsync(chatId, ct);
        }
        public async Task SendHeadFileLoadedSuccessfully(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
            await _messanger.SendHeadFileLoadedSuccessfullyAsync(chatId, ct);
        }

        public async Task StartLoadingLinksInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadLinksInfo, chatData);
            await _messanger.SendReadyToProcessLinksInfoAsync(chatId, ct);
        }

        public async Task SendLinksFileLoadedSuccessfully(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminMainMenu, chatData);
            await _messanger.SendLinksFileLoadedSuccessfullyAsync(chatId, ct);
        }

        public async Task StartLoadingSchedule(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadSchedule, chatData);
            await _messanger.SendReadyToProcessSchedulesAsync(chatId, ct);
        }

        public async Task StartLoadingExams(long chatId, ChatData chatData, CancellationToken ct)
        {
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminLoadExams, chatData);
            await _messanger.SendReadyToProcessExamsAsync(chatId, ct);
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

        public async Task StartInsertingEntityName(long chatId, ChatData chatData, MenuType nextMenuType, CancellationToken ct)
        {
            string? searchQueryName = _chatController.GetSearchQueryName(chatId); // Получаем либо имя группы, либо имя преподавателя
                                                                                  // по которым будет производится поиск расписания

            if (searchQueryName is null)
            {
                _chatController.UpdateNextMenuById(chatId, nextMenuType, chatData);
                _chatController.UpdateCurrentMenuById(chatId, MenuType.InsertingEntityName, chatData);
                await _messanger.StartInsertingSearchQueryAsync(chatId, ct);
            }
            else
            {
                await _messanger.SendWeekParityKeyboard(chatId, ct);
            }
        }

        public async Task StartFillingGroupInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            await _messanger.StartInsertingGroupNameAsync(chatId, ct);
            _chatController.UpdateNextMenuById(chatId, MenuType.AdminInsertingGroupCode, chatData);
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupName, chatData);
        }

        public async Task StartFillingGroupPracticeInfo(long chatId, ChatData chatData, CancellationToken ct)
        {
            await _messanger.StartInsertingGroupNameAsync(chatId, ct);
            _chatController.UpdateNextMenuById(chatId, MenuType.AdminInsertingPracticeTeacherFullName, chatData);
            _chatController.UpdateCurrentMenuById(chatId, MenuType.AdminInsertingGroupName, chatData);
        }


        


    }
}
