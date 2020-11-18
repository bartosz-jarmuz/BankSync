// ReSharper disable InconsistentNaming
namespace BankSync.Exporters.Ipko.DTO
{

    internal class LoginStep2Request
    {
        public LoginStep2Request(string flowId, string token, string password, Sequence sequence)
        {
            this.flow_id = flowId;
            this.token = token;
            this.data = new Data()
            {
                password = password
            };
            this.seq = sequence.GetValue();
        }

        public int version { get; set; } = 3;
        public int seq { get; set; }
        public string location { get; set; } = "";
        public string state_id { get; set; } = "password";
        public string flow_id { get; set; }
        public string token { get; set; }
        public Data data { get; set; }
        public string action { get; set; } = "submit";
        public class Data
        {
            public string password { get; set; }
        }
    }

   

}
