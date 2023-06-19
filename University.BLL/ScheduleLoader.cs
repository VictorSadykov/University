﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using University.Common;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;
using University.MiniMethods;

namespace University.BLL
{
    /// <summary>
    /// Класс, используя PDF файлы расписаний, загружает в базу данных информацию о группах и их расписание.
    /// </summary>
    public class ScheduleLoader
    {

        private IGroupRepository _groupRepo;
        private ILessonRepository _lessonRepo;
        private ITeacherRepository _teacherRepo;
        private IExamRepository _examRepo;

        public ScheduleLoader(IGroupRepository groupRepo, ILessonRepository lessonRepo, ITeacherRepository teacherRepo, IExamRepository examRepo)
        {
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
            _teacherRepo = teacherRepo;
            _examRepo = examRepo;
        }

        public async Task<(string entityName, bool isEntityNew)> AddExamScheduleAsync(string link)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(link);

            bool isScheduleForGroup = link.Contains("group");

            bool isEntityNew = false;
            string entityName = null;

            var examContainer = htmlDoc.GetElementbyId("session_tab");
            var dayContainers = examContainer.SelectNodes("./div[@class=\"day\"]");

            if (isScheduleForGroup)
            {
                entityName = ExtractGroupName(htmlDoc);
                if (_groupRepo.FindByName(entityName) is null) // Проверка есть ли сущность группы в базе данных
                {
                    isEntityNew = true;
                }
            }
            else
            {
                entityName = ExtractTeacherFullName(htmlDoc);
                if (_teacherRepo.FindByFullName(entityName) is null)
                {
                    isEntityNew = true;
                }
            }

            var examList = new List<Exam>[dayContainers.Count];

            for (int i = 0; i < dayContainers.Count; i++)
            {
                examList[i] = await ExtractDayExamsFromNode(dayContainers[i], isScheduleForGroup);
            }

            if (isScheduleForGroup)
            {
                Group? foundGroup = _groupRepo.FindByName(entityName);

                if (foundGroup is null)
                {
                    foundGroup = new Group() { Name = entityName };
                }
                else
                {
                    await _groupRepo.ResetExamScheduleAsync(foundGroup);
                }

                foreach (var day in examList)
                {
                    foreach (var exam in day)
                    {
                        exam.Group = foundGroup;
                        await _examRepo.AddAsync(exam);
                    }
                }
            }
            else
            {
                Teacher? foundTeacher = _teacherRepo.FindByFullName(entityName);

                if (foundTeacher is null)
                {
                    (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(entityName);

                    foundTeacher = new Teacher()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        SecondName = secondName
                    };
                }
                else
                {
                    await _teacherRepo.ResetExamScheduleAsync(foundTeacher);
                }

                foreach (var day in examList)
                {
                    foreach (var exam in day)
                    {
                        exam.Teacher = foundTeacher;
                        await _examRepo.AddAsync(exam);
                    }
                }
            }

            return (entityName, isEntityNew);
        }

        public async Task<(string entityName, bool isEntityNew)> AddScheduleAsync(string link)
        {

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(link);

            /* var path = @"C:\Users\Витя\YandexDisk\C#\Расписание БВТ22-01.html";
             var htmlDoc = new HtmlDocument();
             htmlDoc.Load(path);
 */

            bool isScheduleForGroup = link.Contains("group"); // Флаг чьё расписание мы проверяем. Проверка на основе ссылки - в расписании для группы есть подстрока "group",
                                                              // в расписании для преподавателя - "professor"

            bool isEntityNew = false; // Флаг новая ли сущность учителя или урока (не существует ли она уже в БД)
            string entityName = null;

            var scheduleСontainerNode = htmlDoc.GetElementbyId("timetable_tab"); // Элемент расписания

            var scheduleWeekNodes = scheduleСontainerNode.SelectNodes("./div[@class=\"tab-content\"]/div");

           

            if (isScheduleForGroup)
            {
                entityName = ExtractGroupName(htmlDoc);
                if (_groupRepo.FindByName(entityName) is null) // Проверка есть ли сущность группы в базе данных
                {
                    isEntityNew = true;
                }
            }
            else
            {
                entityName = ExtractTeacherFullName(htmlDoc);
                if (_teacherRepo.FindByFullName(entityName) is null)
                {
                    isEntityNew = true;
                }
            }


            var weekSchedules = new List<List<List<Lesson>>>[scheduleWeekNodes.Count];

            for (int i = 0; i < scheduleWeekNodes.Count; i++)
            {
                weekSchedules[i] = ExtractWeekScheduleFromNode(scheduleWeekNodes[i], isScheduleForGroup);
            }

            /* if (isScheduleForGroup)
             {
                 Group? group = await _groupRepo.FindByNameAsync(entityName);
                 if (group is null)
                 {
                     group = new Group()
                     {
                         Name = entityName
                     };

                     await _groupRepo.AddAsync(group);
                 }

                 await _groupRepo.ResetLessonScheduleAsync(group);
             }
             else
             {
                 Teacher? teacher = await _teacherRepo.FindByFullNameAsync(entityName);

                 if (teacher is null)
                 {
                     (string firstName, string lastName, string secondName) = FullNameParser.Parse(entityName);
                     teacher = new Teacher()
                     {
                         FirstName = firstName,
                         LastName = lastName,
                         SecondName = secondName
                     };

                     await _teacherRepo.AddAsync(teacher);
                 }

                 await _teacherRepo.ResetLessonScheduleAsync(teacher);
             }*/

            if (isScheduleForGroup)
            {
                Group? foundGroup = _groupRepo.FindByName(entityName);

                if (foundGroup is null)
                {
                    foundGroup = new Group() { Name = entityName };
                }
                else
                {
                    await _groupRepo.ResetLessonScheduleAsync(foundGroup);
                }

                foreach (var week in weekSchedules)
                {
                    foreach (var day in week)
                    {
                        foreach (var time in day)
                        {
                            foreach (var lesson in time)
                            {
                                if (!lesson.Groups.Contains(foundGroup))
                                {
                                    lesson.Groups.Add(foundGroup);
                                }

                                await _lessonRepo.AddAsync(lesson);
                            }
                        }
                    }
                }
            }
            else
            {
                Teacher? foundTeacher = _teacherRepo.FindByFullName(entityName);

                if (foundTeacher is null)
                {
                    (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(entityName);

                    foundTeacher = new Teacher()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        SecondName = secondName
                    };
                }
                else
                {
                    await _teacherRepo.ResetLessonScheduleAsync(foundTeacher);
                }

                foreach (var week in weekSchedules)
                {
                    foreach (var day in week)
                    {
                        foreach (var time in day)
                        {
                            foreach (var lesson in time)
                            {
                                lesson.Teacher = foundTeacher;
                                await _lessonRepo.AddAsync(lesson);
                            }
                        }
                    }
                }
            }

           /* if (isScheduleForGroup)
            {
                Group? foundGroup = _groupRepo.FindByName(entityName);

                if (foundGroup is not null)
                {
                    await _groupRepo.ResetLessonScheduleAsync(foundGroup);
                }
            }
            else
            {
                Teacher? foundTeacher = _teacherRepo.FindByFullName(entityName);

                if (foundTeacher is not null)
                {

                    await _teacherRepo.ResetLessonScheduleAsync(foundTeacher);
                }
            }


            foreach (var week in weekSchedules)
            {
                foreach (var day in week)
                {
                    foreach (var time in day)
                    {
                        foreach (var lesson in time)
                        {

                            if (isScheduleForGroup)
                            {
                                Group? foundGroup = _groupRepo.FindByName(entityName);
                                if (foundGroup is null)
                                {
                                    foundGroup = new Group() { Name = entityName };
                                }

                                if (!lesson.Groups.Contains(foundGroup))
                                {
                                    lesson.Groups.Add(foundGroup);
                                }
                            }
                            else
                            {
                                Teacher? foundTeacher = _teacherRepo.FindByFullName(entityName);

                                if (foundTeacher is null)
                                {
                                    (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(entityName);

                                    foundTeacher = new Teacher()
                                    {
                                        FirstName = firstName,
                                        LastName = lastName,
                                        SecondName = secondName
                                    };
                                }

                                lesson.Teacher = foundTeacher;
                            }

                            await _lessonRepo.AddAsync(lesson);
                        }
                    }
                }
            }*/



            return (entityName, isEntityNew);
        }

        private List<List<List<Lesson>>> ExtractWeekScheduleFromNode(HtmlNode weekNode, bool isScheduleForGroup)
        {
            var weekSchedule = new List<List<List<Lesson>>>();

            string idValue = weekNode.Attributes
                .Where(atr => atr.Name == "id")
                .FirstOrDefault()
                .Value; // поиск номера недели из атрибута id. Пример id="week_2_tab"

            int weekNumber = int.Parse(idValue.Split("_")[1]);
            var dayNodes = weekNode.ChildNodes.Where(cn => cn.Name != "#text").ToList(); // Извлечение узлов дней расписания. Их количество равно количеству учебным дням в недели
            foreach (var dayNode in dayNodes)
            {
                weekSchedule.Add(ExtractDayScheduleFromNode(dayNode, weekNumber, isScheduleForGroup));
            }

            return weekSchedule;
        }

        private async Task<List<Exam>> ExtractDayExamsFromNode(HtmlNode dayNode, bool IsScheduleForGroup)
        {
            var daySchedule = new List<Exam>();

            DateTime examsStartDate = ExtractExamDateFromDayNode(dayNode);
            var examsInOneDayNodes = dayNode
                .SelectSingleNode("./div[@class=\"body\"]")
                .SelectNodes("./div[@class=\"line\"]");

            foreach (var examNode in examsInOneDayNodes)
            {
                daySchedule.Add(await ExtractSingleExam(examNode, IsScheduleForGroup, examsStartDate));
            }

            return daySchedule;
        }

        private async Task<Exam> ExtractSingleExam(HtmlNode examLineNode, bool IsScheduleForGroup, DateTime examDateTime)
        {
            var hourTime = ExtractExamHourTime(examLineNode);

            DateTime examStartTime = examDateTime
                .AddHours(hourTime.Hour)
                .AddMinutes(hourTime.Minute);

            var examUlNode = examLineNode.SelectSingleNode("./div[@class=\"discipline\"]/div/div/ul");

            (string examName, ExaminationType examType) = ExtractExamNameAndType(examUlNode);
            (string corpusLetter, string cabNumber) = ExtractCorpusLetterAndCabNumber(examUlNode);

            Exam exam = new Exam
            {
                Name = examName,
                ExaminationType = examType,
                StartDateTime = examStartTime,
                CabNumber = cabNumber,
                CorpusLetter = corpusLetter
            };

            string teacherName;
            string groupName;
            if (IsScheduleForGroup)
            {
                teacherName = ExtractTeacherNameFromSingleLesson(examUlNode);
                Teacher? teacher = _teacherRepo.FindByFullName(teacherName);

                if (teacher is null)
                {
                    (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(teacherName);
                    teacher = new Teacher()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        SecondName = secondName,
                    };

                    await _teacherRepo.AddAsync(teacher);
                }

                exam.Teacher = teacher;
            }
            else
            {
                groupName = ExtractGroupNamesFromSingleLesson(examUlNode)[0];
                Group? group = _groupRepo.FindByName(groupName);

                if (group is null)
                {
                    group = new Group
                    {
                        Name = groupName
                    };
                    await _groupRepo.AddAsync(group);
                }

                exam.Group = group;
            }

            return exam;
        }

        private (string lessonName, ExaminationType examType) ExtractExamNameAndType(HtmlNode examUlNode)
        {
            var liWithExamNameAndType = examUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-bookmark\")]]");
            var spanWithExamName = liWithExamNameAndType.SelectSingleNode("span");
            string examName = spanWithExamName.InnerText;

            string ExamTypeName = liWithExamNameAndType.InnerHtml
                .Replace(liWithExamNameAndType.SelectSingleNode("i").OuterHtml, "")
                .Replace(spanWithExamName.OuterHtml, "")
                .Replace("<br>", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();

            ExaminationType examType = ExamTypeName switch
            {
                "Экзамен" => ExaminationType.Exam,
                "Зачёт" => ExaminationType.Offset,
                "Зачет" => ExaminationType.Offset,
                _ => throw new Exception("Parsing exam type error")
            };


            return (examName, examType);
        }

        private DateTime ExtractExamHourTime(HtmlNode examNode)
        {
            var hourTime = examNode.SelectSingleNode("./div[contains(@class, time)]/div").InnerText;
            hourTime = hourTime
                .Replace("\n", "")
                .Trim();

            return DateTime.ParseExact(hourTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }

        private DateTime ExtractExamDateFromDayNode(HtmlNode dayNode)
        {
            var dateTime = dayNode
                .SelectSingleNode("./div[@class=\"header\"]/div/div").InnerText;

            dateTime = dateTime
                .Replace("\n", "")
                .Trim();

            return DateTime.ParseExact(dateTime, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }


        private List<List<Lesson>> ExtractDayScheduleFromNode(HtmlNode dayNode, int weekNumber, bool isScheduleForGroup)
        {
            var daySchedule = new List<List<Lesson>>();

            string classValue = dayNode.Attributes
                .Where(atr => atr.Name == "class")
                .FirstOrDefault()
                .Value; // Значение атрибута class из которого можно извлечь название дня недели

            string dayOfWeekName = classValue.Split(" ")[1];
            int dayOfWeekNumber = 0;

            switch (dayOfWeekName)
            {
                case "monday": 
                    dayOfWeekNumber = 1;
                    break;
                case "tuesday": 
                    dayOfWeekNumber = 2;
                    break;
                case "wednesday":
                    dayOfWeekNumber = 3;
                    break;
                case "thursday": 
                    dayOfWeekNumber = 4;
                    break;
                case "friday": 
                    dayOfWeekNumber = 5;
                    break;
                case "saturday": 
                    dayOfWeekNumber = 6;
                    break;
                default:
                    break;
            }

            var lessonNodes = dayNode.SelectNodes(".//div[@class=\"line\"]");

            foreach (var lessonNode in lessonNodes)
            {
                daySchedule.Add(ExtractLessonListFromOneTimeZone(lessonNode, weekNumber, dayOfWeekNumber, isScheduleForGroup));
            }

            return daySchedule;
        }

        private List<Lesson> ExtractLessonListFromOneTimeZone(HtmlNode lessonNode, int weekNumber, int dayOfWeekNumber, bool isScheduleForGroup)
        {
            var lessonInfoInOneTimeZone = new List<Lesson>();
            var timeNode = lessonNode.SelectSingleNode("./div[contains(@class, \"time\")]/div[2]");
            int timeNumber = ExtractTimeNumber(timeNode);

            var lessonInfoNodes = lessonNode.SelectNodes("./div[@class=\"discipline\"]//ul"); // Информация о занятиях множества подгрупп группы.
            // Может быть листом с 1 элементом, если все подгруппы идут на одно занятие
            // Может быть листом с несколькими элементами, если каждая подгруппа идёт на 1 занятие
            // Может быть листом с несколькими элементами, если каждая подгруппа идёт на множество занятий
                    

            foreach (var lessonUl in lessonInfoNodes)
            {
                lessonInfoInOneTimeZone.Add(ExtractSingleLesson(lessonUl, weekNumber, dayOfWeekNumber, timeNumber, isScheduleForGroup).Result);
            }


            return lessonInfoInOneTimeZone;
        }

        private async Task<Lesson> ExtractSingleLesson(HtmlNode lessonUlNode, int weekNumber, int dayOfWeekNumber, int timeNumber, bool isScheduleForGroup)
        {
            (string lessonName, LessonType lessonType) = ExtractLessonNameAndType(lessonUlNode);
            (string corpusLetter, string cabNumber) = ExtractCorpusLetterAndCabNumber(lessonUlNode);
            string subGroupName = ExtractSubGroupName(lessonUlNode);

            string teacherName;
            List<string> groupNames; // Если мы рассматриваем расписание преподавателя, он может вести занятие у нескольких групп,
                                     // поэтому названия этих групп записываем в список


            Lesson lesson = new Lesson()
            {
                Name = lessonName,
                LessonType = lessonType,
                CorpusLetter = corpusLetter,
                CabNumber = cabNumber,
                WeekNumber = weekNumber,
                DayNumber = (DayOfWeek)dayOfWeekNumber,
                TimeNumber = timeNumber,
                SubGroup = subGroupName
            };

            if (isScheduleForGroup)
            {
                teacherName = ExtractTeacherNameFromSingleLesson(lessonUlNode); // Полное имя учителя, которое пишется внутри элемента урока
                

                Teacher? teacher = _teacherRepo.FindByFullName(teacherName); // Проверяем есть ли учитель в базе данных

                if (teacher is null)
                {
                    (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(teacherName);
                    teacher = new Teacher()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        SecondName = secondName,
                    };

                    await _teacherRepo.AddAsync(teacher);
                }

                lesson.Teacher = teacher;

            }
            else
            {
                groupNames = ExtractGroupNamesFromSingleLesson(lessonUlNode);

                foreach (var groupName in groupNames)
                {
                    Group? group = _groupRepo.FindByName(groupName);

                    if (group is null)
                    {
                        group = new Group
                        {
                            Name = groupName
                        };
                        await _groupRepo.AddAsync(group);
                    }

                    lesson.Groups.Add(group);
                }
            }

            return lesson;            
        }

        private string ExtractSubGroupName(HtmlNode lessonUlNode)
        {
            /*var liWithCorpusInfo = lessonUlNode.ChildNodes
                .Where(cn =>
                cn.Attributes
                    .Any(atr => atr.Name == "class" && atr.Value.Contains("num_pdgrp"))
                    ||
                cn.ChildNodes
                    .Any(gcn => gcn.Name == "i" && gcn.Attributes
                        .Any(atr => atr.Name == "class" && atr.Value.Contains("fa-paperclip"))                    
                    );*/

            var subGroupLi = lessonUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-paperclip\")]]");

            if (subGroupLi is null)
            {
                subGroupLi = lessonUlNode.SelectSingleNode("./li[contains(@class, \"num_pdgrp\")]");
                if (subGroupLi is null)
                {
                    return 0.ToString();
                }
            }

            string subGroupName = subGroupLi.InnerText.Split(" ")[0];

            return subGroupName;
        }

        private (string corpusLetter, string cabNumber) ExtractCorpusLetterAndCabNumber(HtmlNode lessonUlNode)
        {
            var liWithCorpusInfo = lessonUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-compass\")]]");
            string[] strings = liWithCorpusInfo.InnerText.Split(" ");
            string corpusLetter = strings[1].Replace("\"", "");
            string cabNumber = strings[3].Replace("\"", "");

            return (corpusLetter, cabNumber);
        }

        private string ExtractTeacherNameFromSingleLesson(HtmlNode lessonUlNode)
        {
            var liWithTeacherName = lessonUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-user\")]]");

            return liWithTeacherName.InnerText;
        }

        private List<string> ExtractGroupNamesFromSingleLesson(HtmlNode lessonUlNode)
        {
            List<string> output = new List<string>();
            var lisWithGroupName = lessonUlNode.SelectNodes("./li[i[contains(@class, \"fa-group\")]]");

            foreach (var li in lisWithGroupName)
            {
                output.Add(li.InnerText);
            }

            return output;
        }

        private (string lessonName, LessonType lessonType) ExtractLessonNameAndType(HtmlNode lessonUlNode)
        {
            var liWithLessonNameAndType = lessonUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-bookmark\")]]");
            var spanWithLessonName = liWithLessonNameAndType.SelectSingleNode("span");
            string lessonName = spanWithLessonName.InnerText;

            string lessonTypeName = liWithLessonNameAndType.InnerHtml
                .Replace(liWithLessonNameAndType.SelectSingleNode("i").OuterHtml, "")
                .Replace(spanWithLessonName.OuterHtml, "")
                .Replace("<br>", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();

            LessonType lessonType = lessonTypeName switch
            {
                "Практика" => LessonType.Practice,
                "Лекция" => LessonType.Lecture,
                "Лабораторная работа" => LessonType.LabWork,
                _ => throw new Exception("Parsing lesson type error")
            };
            

            return (lessonName, lessonType);
        }

        private int ExtractTimeNumber(HtmlNode timeNode)
        {
            var timeString = timeNode.InnerHtml.Replace("\n", " ").Trim();
            string[] times = timeString.Split("<br>");
            string timeStart = times[0];

            int timeNumber = ScheduleTimer.GetLessonNumberFromTimeStart(timeStart);
            return timeNumber;
        }

        private string ExtractGroupName(HtmlDocument htmlDocument)
        {
            var node = htmlDocument.DocumentNode.SelectSingleNode("//body//div[@id=\"wrapwrap\"]//main//div//h3");

            /*innertext = &quot;А20-01&quot;
            2 семестр 2022 - 2023г.*/
            string sep;
            if (node.InnerText.Contains("&quot;"))
            {
                sep = "&quot;";
            }
            else
            {
                sep = "\"";
            }

            string[] strings = node.InnerText.Split(sep);

            return strings[1];
                        
        }

        private string ExtractTeacherFullName(HtmlDocument htmlDocument)
        {
            var node = htmlDocument.DocumentNode.SelectSingleNode("//body//div[@id=\"wrapwrap\"]//main//div//h3");
            string[] strings = node.InnerText.Split("-");
            string output = strings[0]
                .Replace("\n", "")
                .Trim();
            return output;
        }

    }
}
