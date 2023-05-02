using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.BLL
{
    public class JsonService
    {
        public void WriteObjectToJson<T>(T objectToSerialize, string path)
        {
            string serializedObject = JsonConvert.SerializeObject(objectToSerialize);

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(serializedObject);
            }
        }

        public string GetChatDataListInJson()
        {
            string output = null;

            using (StreamReader sr = new StreamReader(_chatDataListJsonStoragePath))
            {
                output = sr.ReadToEnd();
            }

            return output;
        }

        public List<ChatData>? GetChatDataList()
        {
            List<ChatData>? chatDataList = JsonConvert
                .DeserializeObject<List<ChatData>?>(
                    GetChatDataListInJson()
                );

            if (chatDataList is null) chatDataList = new List<ChatData>();

            return chatDataList;
        }
    }
}
