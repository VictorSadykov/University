using Newtonsoft.Json;

namespace University.BLL
{
    public class Class1
    {
        public void Go()
        {
            string text = JsonConvert.SerializeObject()

        }
    }

    public class ChatData
    {
        public long ChatId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}