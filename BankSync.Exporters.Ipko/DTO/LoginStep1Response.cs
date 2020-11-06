namespace BankSync.Exporters.Ipko.DTO
{

    public class LoginStep1Response
    {
        public int httpStatus { get; set; }
        public string flow_id { get; set; }
        public string token { get; set; }
        public Response response { get; set; }
        public string state_id { get; set; }
        public class Response
        {
            public Data data { get; set; }
            public Fields fields { get; set; }

            public class Fields
            {
                public object errors { get; set; }
                public Password password { get; set; }

                public class Password
                {
                    public object errors { get; set; }
                    public object value { get; set; }
                    public Widget widget { get; set; }

                    public class Widget
                    {
                        public string field_type { get; set; }
                        public bool required { get; set; }
                        public int max_len { get; set; }
                    }
                }
            }


            public class Data
            {
                public Image image { get; set; }
                public string tracking_pixel { get; set; }
                public class Image
                {
                    public string src { get; set; }
                }

            }
        }
    }

  



 
   

   



}
