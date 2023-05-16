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

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(link);

            var timeTableNode = htmlDoc.GetElementbyId("timetable_tab"); // Элемент расписания
            
            


            /*string excelPath = ConvertPdfToExcel(sourcePath);

            (string groupName,
             string groupCode,
             string groupSpecialization,
             string groupOrientation
             ) groupInfo = GetGroupInfoFromExcel(excelPath);

            *//*(string groupName,
             string groupCode,
             string groupSpecialization,
             string groupOrientation
             ) groupInfo = GetGroupInfoFromExcel(@"C:\Users\Витя\YandexDisk\C#\ПРОЕКТЫ\University\Data\schedules\EXCEL\d.xlsx");*/

            /*(string groupName,
             string groupCode,
             string groupSpecialization,
             string groupOrientation
             ) groupInfo = ("А20-01", "24.05.01", "Проектирование, производство и эксплуатация ракет и ракетно-космических", "Ракетные транспортные системы");*//*

            int groupOperationStatusCode = await _groupRepo.WriteOrEditAsync(groupInfo);

            string message = groupOperationStatusCode == 0 ?
                $"Обновлена информация существующей группы {groupInfo.groupName}" :
                $"Добавлена информация о новой группе {groupInfo.groupName}";


            await Test(excelPath, groupInfo.groupName);

            Console.WriteLine(message);*/
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
