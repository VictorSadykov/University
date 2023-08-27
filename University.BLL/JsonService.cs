using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.BLL
{
    /// <summary>
    /// Класс работы с json
    /// </summary>
    public class JsonService
    {
        private string _storagePath;

        public JsonService(string storagePath)
        {
            _storagePath = storagePath;
        }

        /// <summary>
        /// Записывает объект в JSON хранилище
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize">Объект для записи</param>
        public void WriteObjectToJson<T>(T objectToSerialize)
        {
            string serializedObject = JsonConvert.SerializeObject(objectToSerialize);

            using (StreamWriter sw = new StreamWriter(_storagePath))
            {
                sw.WriteLine(serializedObject);
            }
        }

        /// <summary>
        /// Получает объект в формате JSON строки
        /// </summary>
        /// <returns></returns>
        public string GetObjectInJson()
        {
            string output = null;

            using (StreamReader sr = new StreamReader(_storagePath))
            {
                output = sr.ReadToEnd();
            }

            return output;
        }

        /// <summary>
        /// Возвращает десириализованный объект из JSON хранилища
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetObject<T>() where T : new()
        {
            string objectJsonString = GetObjectInJson();

            T? objectFromJsonStorage = JsonConvert.DeserializeObject<T?>(objectJsonString);

            if (objectFromJsonStorage is null) objectFromJsonStorage = new T();

            return objectFromJsonStorage;
        }
    }
}
