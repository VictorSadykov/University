using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.Common
{
    public static class MenuMessages
    {
        public const string INSERT_ENTITY_NAME_MENU_ITEM = "Ввести название группы.";
        public const string START_SKIP = "Пропустить.";
        public const string BACK = "Назад.";
        public const string WATCH_TODAY_SCHEDULE = "📅 Посмотреть расписание на сегодня.";
        public const string WATCH_WEEK_SCHEDULE = "🗓 Посмотреть расписание на неделю.";
        public const string WATCH_PRACTICE_SCHEDULE = "🛠 Посмотреть расписание практики.";
        public const string WATCH_EXAM_SCHEDULE = "🎓 Посмотреть расписание сессии.";
        public const string WATCH_CORPUS_INFO = "🏫 Посмотреть информацию о корпусах.";
        public const string WATCH_LINKS_INFO = "🔗 Посмотреть ссылки на полезные источники.";
        public const string WATCH_HEAD_INFO = "👨‍🏫 Посмотреть информацию о кафедре.";
        public const string ENTER_ADMIN_MENU = "⚙ Войти в меню админа.";
        public const string ENTER_ORD_MENU = "🔑 Войти в обычное меню.";
        public const string ADMIN_LOAD_SCHEDULE = "🗂 Загрузить расписание группы.";
        public const string ENTER_CHOOSE_MENU = "🚫 Выйти к выбору меню.";
        public const string ADMIN_LOAD_HEAD_INFO = "👩‍💻 Загрузить информацию о кафедре.";
        public const string ADMIN_LOAD_CORPUS_INFO = "🧱 Загрузить информацию о корпусах.";
        public const string SELECT_MENU_ITEM = "Выберите пункт меню";
        public const string GROUP_IS_NOT_FOUND = "Данного преподавателя/группы не существует. Попробуйте ввести имя преподавателя/группы правильно.\n\nДля выхода в главное меню нажмите кнопку.";
        public const string SEND_GROUP_SCHEDULE_LINK = "Отправьте ссылку на расписание.";
        public const string WRONG_LINK_FORMAT = "Неверный формат ссылки. Ссылка должна начинаться на \"https://timetable.pallada.sibsau.ru/timetable/\"";
        public const string NEW_GROUP_ADDED = "Новая группа добавлена:";
        public const string NEW_SCHEDULE_ADDED = "Добавлено расписание для группы";
        public const string SEND_HEAD_FILE = "Отправьте txt файл с информацией о кафедре."; 
        public const string SEND_CORPUS_FILE = "Отправьте txt файл с информацией о корпусах.";
        public const string SEND_LINKS_FILE = "Отправьте txt файл с полезными ссылками.";
        public const string CORPUS_FILE_LOADED_SUCCESSFULLY = "Информация о корпусах загружена.";
        public const string HEAD_FILE_LOADED_SUCCESSFULLY = "Информация о кафедре загружена.";
        public const string LINKS_FILE_LOADED_SUCCESSFULLY = "Ссылки на полезные источники загружены.";
        public const string INSERT_ENTITY_NAME = "Введите название учебной группы или инициалы преподавателя.\n\nПример:\nА01-02Б (для поиска по группе)\nИванов (для поиска по преподавателю только по фамилии)\nИванов И. О. (для поиска по преподавателю полностью по инициалам)\n\nДля выхода в главное меню нажмите на кнопку.";
        public const string INSERT_GROUP_NAME = "Введите название учебной группы для которой необходимо заполнить информацию.";
        public const string INSERT_GROUP_CODE = "Введите код специальности группы.";
        public const string INSERT_GROUP_SPECIALIZATION = "Введите направленность группы.";
        public const string INSERT_GROUP_ORIENTATION = "Введите направление группы.";
        public const string INSERT_GROUP_PRACTICE_TEACHER_FULLNAME = "Введите фамимилию и инициалы руководителя практики.";
        public const string INSERT_PRACTICE_START_DATE = "Введите дату начала практики в формате дд.мм.гггг\n(Например 31.12.2000).";
        public const string INSERT_PRACTICE_END_DATE = "Введите дату конца практики в формате дд.мм.гггг\n(Например 31.12.2000).";
        public const string ADMIN_FILL_GROUP_INFO = "📋 Заполнить информацию о группе";
        public const string ADMIN_FILL_GROUP_PRACTICE_INFO = "🔨 Заполнить информацию о практике группы";
        public const string ADMIN_LOAD_LINKS_INFO = "⛓ Загрузить ссылки на полезные источники";
        public const string RESET_SEARCH_QUERY = "❌ Сбросить группу/преподавателя по которым производится поиск расписания";
        public const string RESET_SEARCH_QUERY_SUCCESSFULLY = "Поисковый запрос расписаний сброшен.";


    }
}
