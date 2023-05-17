using Aspose.Pdf;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using University.Common;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.BLL
{
    /// <summary>
    /// Класс, используя PDF файлы расписаний, загружает в базу данных информацию о группах и их расписание.
    /// </summary>
    public class ScheduleLoader
    {

        private string excelDestPath = DataConfig.DATA_FOLDER_PATH + "schedules/EXCEL/";
        private IGroupRepository _groupRepo;
        private ILessonRepository _lessonRepo;

        public ScheduleLoader(IGroupRepository groupRepo, ILessonRepository lessonRepo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _groupRepo = groupRepo;
            _lessonRepo = lessonRepo;
        }

        public async Task AddScheduleAsync(string link)
        {

            /*HtmlWeb web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(link);*/

            var path = @"C:\Users\Витя\YandexDisk\C#\Расписание БВТ22-01.html";
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(path);

            string groupName = ExtractGroupName(htmlDoc); // Получение имени группы

            var scheduleСontainerNode = htmlDoc.GetElementbyId("timetable_tab"); // Элемент расписания

            var scheduleWeekNodes = scheduleСontainerNode.SelectNodes("./div/div");

            var weekSchedules = new List<List<List<Lesson>>>[scheduleWeekNodes.Count];

            for (int i = 0; i < scheduleWeekNodes.Count; i++)
            {
                weekSchedules[i] = ExtractWeekScheduleFromNode(scheduleWeekNodes[i]);
            }

            DLL.Sqlite.Entities.Group group = await _groupRepo.FindByName(groupName);

            if (group is null)
            {
                await _groupRepo.Add(groupName);
                group = await _groupRepo.FindByName(groupName);
            }

            await _groupRepo.ResetLessonSchedule(group);

            foreach (var week in weekSchedules)
            {
                foreach (var day in week)
                {
                    foreach (var time in day)
                    {
                        foreach (var lesson in time)
                        {
                            lesson.Group = group;
                            await _groupRepo.AddLesson(group, lesson);                            
                        }
                    }
                }
            }

        }

        private List<List<List<Lesson>>> ExtractWeekScheduleFromNode(HtmlNode weekNode)
        {
            var weekSchedule = new List<List<List<Lesson>>>();

            string idValue = weekNode.Attributes
                .Where(atr => atr.Name == "id")
                .FirstOrDefault()
                .Value; // week_2_tab

            int weekNumber = int.Parse(idValue.Split("_")[1]);
            var dayNodes = weekNode.ChildNodes.Where(cn => cn.Name != "#text").ToList();
            foreach (var dayNode in dayNodes)
            {
                weekSchedule.Add(ExtractDayScheduleFromNode(dayNode, weekNumber));
            }

            return weekSchedule;
        }

        private List<List<Lesson>> ExtractDayScheduleFromNode(HtmlNode dayNode, int weekNumber)
        {
            var daySchedule = new List<List<Lesson>>();

            string classValue = dayNode.Attributes
                .Where(atr => atr.Name == "class")
                .FirstOrDefault()
                .Value;

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
                daySchedule.Add(ExtractLessonListFromOneTimeZone(lessonNode, weekNumber, dayOfWeekNumber));
            }

            return daySchedule;
        }

        private List<Lesson> ExtractLessonListFromOneTimeZone(HtmlNode lessonNode, int weekNumber, int dayOfWeekNumber)
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
                lessonInfoInOneTimeZone.Add(ExtractSingleLesson(lessonUl, weekNumber, dayOfWeekNumber, timeNumber));
            }


            return lessonInfoInOneTimeZone;
        }

        private Lesson ExtractSingleLesson(HtmlNode lessonUlNode, int weekNumber, int dayOfWeekNumber, int timeNumber)
        {
            (string lessonName, LessonType lessonType) = ExtractLessonNameAndType(lessonUlNode);
            string teacherName = ExtractTeacherName(lessonUlNode);
            (string corpusLetter, string cabNumber) = ExtractCorpusLetterAndCabNumber(lessonUlNode);
            string subGroupName = ExtractSubGroupName(lessonUlNode);

            return new Lesson()
            {
                Name = lessonName,
                LessonType = lessonType,
                CorpusLetter = corpusLetter,
                CabNumber = cabNumber,
                TeacherFullName = teacherName,
                WeekNumber = weekNumber,
                DayNumber = (DayOfWeek)dayOfWeekNumber,
                TimeNumber = timeNumber,
                SubGroup = subGroupName
            };
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

        private string ExtractTeacherName(HtmlNode lessonUlNode)
        {
            var liWithTeacherName = lessonUlNode.SelectSingleNode("./li[i[contains(@class, \"fa-user\")]]");

            return liWithTeacherName.InnerText;
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

            string[] strings = node.InnerText.Split("\"");

            return strings[1];
        }

        private async Task Test(string path, string groupName)
        {
            string[,] strings;
            using (var package = new ExcelPackage(path))
            {
                var a = package.Workbook.Worksheets[0];
                int x = 0;
                int y = 0;
                string ad = null;
                foreach (var item in a.Cells)
                {
                    if (item.Value?.ToString() == "I НЕДЕЛЯ")
                    {
                        y = item.Start.Row;
                    }
                }


                strings = new string[a.Dimension.Rows - y, a.Dimension.Columns];

                for (int i = 1; i <= strings.GetLength(0); i++)
                {
                    for (int j = 1; j <= strings.GetLength(1); j++)
                    {
                        strings[i - 1, j - 1] = a.Cells[i + y, j].Value?.ToString();
                    }
                }


            }
            using (var p = new ExcelPackage(@$"C:\\Users\\Витя\\YandexDisk\\C#\\ПРОЕКТЫ\\University\\Data\\schedules\\EXCEL\test\{groupName}.xlsx"))
            {
                var sheet = p.Workbook.Worksheets.Add(DateTime.Now.ToString());
                for (int z = 0; z < strings.GetLength(0); z++)
                {
                    for (int c = 0; c < strings.GetLength(1); c++)
                    {
                        sheet.Cells[z + 1, c + 1].Value = strings[z, c];
                    }
                }

                await p.SaveAsync();
            }
        }
        

        private (string, string,  string, string) GetGroupInfoFromExcel(string excelPath)
        {
            (string groupName, string groupCode, string groupSpecialization, string groupOrientation) output = (null, null, null, null);

            

            using (var package = new ExcelPackage(excelPath))
            {
                var sheet = package.Workbook.Worksheets[0];

                foreach (var item in sheet.Cells)
                {

                    if (item.Value?.ToString() == "ГРУППА:")
                    {
                        output.groupName = sheet.Cells[item.Start.Row, item.Start.Column + 1].Value.ToString();
                        output.groupName = output.groupName.Replace("\n", " ").Trim();
                    }

                    if (item.Value?.ToString() == "НАПРАВЛЕНИЕ:")
                    {
                        string[] words = sheet.Cells[item.Start.Row, item.Start.Column + 1].Value.ToString().Split(" ");

                        output.groupCode = words[0];
                        output.groupCode = output.groupCode.Replace("\n", " ").Trim();

                        StringBuilder specialization = new StringBuilder();

                        for (int i = 1; i < words.Length; i++)
                        {
                            specialization.Append(words[i] + " ");
                        }

                        output.groupSpecialization = specialization.ToString();
                        output.groupSpecialization = output.groupSpecialization.Replace("\n", " ").Trim();
                    }

                    if (item.Value?.ToString() == "НАПРАВЛЕННОСТЬ:")
                    {
                        output.groupOrientation = sheet.Cells[item.Start.Row, item.Start.Column + 1].Value.ToString();
                        output.groupOrientation = output.groupOrientation.Replace("\n", " ").Trim();
                    }
                }
            }

            return output;
        }

        private string ConvertPdfToExcel(string pdfFilePath)
        {
            Document pdfDocument = new Document(pdfFilePath);
            ExcelSaveOptions excelSaveOptions = new ExcelSaveOptions();
            string fileName = Path.GetFileName(pdfDocument.FileName);
            string newFileName = fileName.Replace("pdf", "xlsx");
            string newFilePath = Path.Combine(excelDestPath, newFileName);

            pdfDocument.Save(newFilePath, excelSaveOptions);

            return newFilePath;
        }
    }
}
