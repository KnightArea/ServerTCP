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
    #region Global Request and Response Class definition
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
    #endregion

    #region Register Parameters Def
    struct Register_Params
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    #endregion

    #region UpdateUserInformation Parameters Def
    struct UpdateUserInformation_Params
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string TeamTitle { get; set; }
        public string EmailAdress { get; set; }
        public string PhoneNumber { get; set; }
        public string TeamColor { get; set; }
    }

    #endregion
    class ServerServices
    {
        public static string Start(RequestType request, out bool HasResponse)
        {
            string result = "";
            try
            {                
                switch (request.FuncName)
                {
                    case "Register":
                        Register_Params Rpar =
                            (Register_Params)JsonConvert.DeserializeObject(request.Params.ToString(), typeof(Register_Params));
                        result = Register(Rpar);
                        break;
                    case "UpdateUserInformation":
                        UpdateUserInformation_Params UUIpar =
                            (UpdateUserInformation_Params)JsonConvert.DeserializeObject(request.Params.ToString(),
                                                                                         typeof(UpdateUserInformation_Params));
                        result = UpdateUserInformation(UUIpar);
                        break;
                    default:
                        break;
                }
            }
            catch 
            {
                result = "error in parameter format";
            }

            HasResponse = true;
            return result;
        }

        private static string UpdateUserInformation(UpdateUserInformation_Params uuip)
        {
            string result = "";

            ResponseType rt = new ResponseType();
            SqlConnection cnn = new SqlConnection(Properties.Settings.Default.__connection);
            SqlCommand cmm = new SqlCommand();
            cmm.Connection = cnn;
            try
            {

                cmm.CommandText = "UPDATE UserIdentifier SET TeamColor=@TeamColor,TeamTitle=@TeamTitle,"+
                    "Name=@Name,EmailAdress=@EmailAdress,Phonenumber=@Phonenumber"+
                    " WHERE id=@id";
                cmm.Parameters.AddWithValue("TeamColor", uuip.TeamColor);
                cmm.Parameters.AddWithValue("TeamTitle", uuip.TeamTitle);
                cmm.Parameters.AddWithValue("Name", uuip.Name);
                cmm.Parameters.AddWithValue("EmailAdress", uuip.EmailAdress);
                cmm.Parameters.AddWithValue("Phonenumber", uuip.PhoneNumber);
                cmm.Parameters.AddWithValue("id", uuip.ID);



                try
                {
                    if (cnn.State != ConnectionState.Open)
                    {
                        cnn.Open();
                    }
                }
                catch { }
                int rowEffected = cmm.ExecuteNonQuery();
                try
                {
                    if (cnn.State != ConnectionState.Closed)
                    {
                        cnn.Close();
                    }
                }
                catch { }
                if (rowEffected > 0)
                {
                    rt.state = true;
                    rt.retVal = "done";
                }
                else
                {
                    rt.state = false;
                    rt.retVal = "no user founded";
                }
            }
            catch (Exception e)
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

                cmm.CommandText = "INSERT INTO UserIdentifier (Latitude,Longitude) output inserted.id" +
                    " Values (" +
                    "@Latitude,@Longitude)";
                cmm.Parameters.AddWithValue("Latitude", rp.Latitude);
                cmm.Parameters.AddWithValue("Longitude", rp.Longitude);

               

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
                rt.retVal = insertedId.ToString();
            }
            catch (Exception e)
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
