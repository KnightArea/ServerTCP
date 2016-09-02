using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.Linq.Mapping;

//فعلا از linq استفاده نمیکنیم و در صورتی که نیاز شد این کار را انجام می دهیم
using System.Data;
using System.Data.SqlClient;

namespace ServerTCPService
{
    
    public struct RequestType
    {
        public string FuncName{ get; set; }
        public object Params { get; set; }
    }

    public struct ResponseType
    {
        public object retVal { get; set; }
        public bool state { get; set; }
    }

    struct Register_Params
    {
        public string Name { get; set; }
        public string TeamTitle { get; set; }
        public string EmailAdress { get; set; }
        public string PhoneNumber { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string TeamColor { get; set; }
        public bool isGuest { get; set; }
    }

    class ServerServices
    {
        
        public static string test(RequestType request,out bool HasResponse)
        {
            string result = "";
            ResponseType rt = new ResponseType();
            rt.retVal = request.FuncName;
            //rt.functions = System.IO.File.ReadAllText(System.IO.Directory.GetCurrentDirectory()+"\\t.txt");
            result = JsonConvert.SerializeObject(rt);
            HasResponse = true;
            return result;
        }

        public static string Start(RequestType request, out bool HasResponse)
        {
            string result = "";
            switch(request.FuncName)
            {
                case "Register":
                    Register_Params par = 
                        (Register_Params)JsonConvert.DeserializeObject(request.Params.ToString(), typeof(Register_Params));
                    result = Register(par);
                    break;
                default:
                    break;
            }
            HasResponse = true;
            return result;
        }

        private static string Register(Register_Params rp)
        {
            string result = "";
            ResponseType rt = new ResponseType();
            //DataContext db = new DataContext(Properties.Settings.Default.__connection);
            SqlConnection cnn = new SqlConnection(Properties.Settings.Default.__connection);
            SqlCommand cmm = new SqlCommand();
            cmm.Connection = cnn;
            try
            {
                if (rp.isGuest == true)
                {
                    cmm.CommandText = "INSERT INTO UserIdentifier (Latitude,Longitude,TeamColor,TeamTitle) output inserted.id"+
                        " Values (" +
                        "@Latitude,@Longitude,@TeamColor,@TeamTitle)";
                    cmm.Parameters.AddWithValue("Latitude", rp.Latitude);
                    cmm.Parameters.AddWithValue("Longitude", rp.Longitude);
                    cmm.Parameters.AddWithValue("TeamColor", rp.TeamColor);
                    cmm.Parameters.AddWithValue("TeamTitle", rp.TeamTitle);
                }
                else if (rp.isGuest == false)
                {
                    cmm.CommandText = "INSERT INTO UserIdentifier (Latitude,Longitude,TeamColor,TeamTitle,Name,EmailAdress"+
                    ",Phonenumber) output inserted.id Values (" +
                        "@Latitude,@Longitude,@TeamColor,@TeamTitle,@Name,@EmailAdress,@Phonenumber)";
                    cmm.Parameters.AddWithValue("Latitude", rp.Latitude);
                    cmm.Parameters.AddWithValue("Longitude", rp.Longitude);
                    cmm.Parameters.AddWithValue("TeamColor", rp.TeamColor);
                    cmm.Parameters.AddWithValue("TeamTitle", rp.TeamTitle);
                    cmm.Parameters.AddWithValue("Name", rp.Name);
                    cmm.Parameters.AddWithValue("EmailAdress", rp.EmailAdress);
                    cmm.Parameters.AddWithValue("Phonenumber", rp.PhoneNumber);
                }
                try
                {
                    if (cnn.State != ConnectionState.Open)
                    {
                        cnn.Open();
                    }
                }
                catch { }
                object insertedId = cmm.ExecuteScalar();
                try
                {
                    if (cnn.State != ConnectionState.Closed)
                    {
                        cnn.Close();
                    }
                }
                catch { }
                rt.state = true;
                rt.retVal = insertedId.ToString() ;
            }
            catch(Exception e)
            {
                rt.state = false;
                rt.retVal = e.Message;
            }
            try
            {
                result = JsonConvert.SerializeObject(rt, typeof(ResponseType), new JsonSerializerSettings());
            }
            catch (Exception e)
            {
                result = "error : " + e.Message;
            }
            return result;
        }
    }
}
