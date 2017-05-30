using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MotivationUser.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace MotivationUser
{
    public class AzureDataService
    {
        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        FacebookUser facebookUser = new FacebookUser();
        static AzureDataService defaultInstance = new AzureDataService();
        SqlDataReader reader;
        private AzureDataService()
        {

        }
        public static AzureDataService DefaultService
        {
            get
            {
                return defaultInstance;
            }
            private set
            {
                defaultInstance = value;
            }
        }
        public void Initialize()
        {
            connection = new SqlConnection(Constants.AzureSQLConnection);
        }
        public User GetUser(string Method)
        {

            User _user = new User();
            string query = "";
            if (Method == "fbId")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE FacebookId = '" + facebookUser.Id + "' AND AdminBool = 0";

            }
            else if (Method == "rId")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE Id = '" + facebookUser.Id + "' AND AdminBool = 0";
            }
            else if (Method == "Name")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE Name = '" + facebookUser.Name + "' AND AdminBool = 0";
            }

            _user = (User)AzureConnect(query, "User");
            return _user;
        }
        public void CompleteTask(User _user)
        {
            string query = "UPDATE Users SET Complete = '"+DateTime.Now+"' WHERE Id = '"+_user.Id+"'";
            AzureConnect(query, "Edit");
        }
        public void RegisterUser(string facebookName, string facebookId)
        {
            string query = "INSERT INTO Users (Name, FacebookId, AdminBool) VALUES ('" + facebookName + "', '" + facebookId + "', 0)";
            AzureConnect(query, "Edit");
        }
        public ChatGroup GetGroup(int _userId)
        {
            string query = "SELECT cg.Id Id, cg.Name Name FROM ChatGroups cg INNER JOIN UserChatGroups ucg"
                                + " ON ucg.ChatGroupId = cg.Id"
                                + " WHERE ucg.UserId ='" + _userId+"'";

            ChatGroup _group = (ChatGroup)AzureConnect(query, "ChatGroup");
            return _group;
        }
        public ChatGroup GetTodo(ChatGroup _chatGroup)
        {
            //ChatGroup cg = new ChatGroup();
            List<TodoItem> toDoList = new List<TodoItem>();

            string query = "SELECT id, text, groupId, complete, sendTime FROM ToDoItem WHERE groupId = '" + _chatGroup.Id + "' AND deleted != 'True' ORDER BY sendTime ASC";
            ChatGroup chatGroup = (ChatGroup)AzureConnect(query, "ToDoList");
            

            return chatGroup;
           
        }
        public DateTime? GetComplete(User _user)
        {

            string query = "SELECT Complete FROM Users WHERE Id = " + _user.Id ;
            var isComplete = (DateTime?)AzureConnect(query, "Complete");

            if (isComplete == DateTime.MinValue)
                return null;

            return isComplete;

        }
        private static readonly int[] RetriableClasses = { 13, 16, 17, 18, 19, 20, 21, 22, 24 };
        public object AzureConnect(string Query, string taskType)
        {
            bool rebuildConnection = true; // First try connection must be open
            object returnValue = null;
            for (int i = 0; i < RetriableClasses[5]; ++i)
            {
                try
                {
                    // (Re)Create connection to SQL Server
                    if (rebuildConnection)
                    {
                        if (connection != null)
                            connection.Dispose();

                        // Create connection and open it...
                        Initialize();
                        cmd.CommandText = Query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        connection.Open();

                    }

                    // inserts information

                    if (taskType == "Edit")
                    {
                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"edited {rows} row(s).");
                        connection.Close();
                        return rows;
                    }
                    else //finds information
                    {
                        reader = cmd.ExecuteReader();
                        if (taskType == "User")
                        {
                            returnValue = readUser(reader);
                        }
                        else if (taskType == "ChatGroup")
                        {
                            returnValue = readChatGroup(reader);
                        }
                        else if (taskType == "Complete")
                        {
                            returnValue = readComplete(reader);
                        }
                        //else if (taskType == "GroupUserList")
                        //{
                        //    returnValue = readGroupUsers(reader);
                        //}
                        //else if (taskType == "UserList")
                        //{
                        //    returnValue = readSingleGroupUsers(reader);
                        //}
                        else if (taskType == "ToDoList")
                        {
                           returnValue = readToDoList(reader);
                        }
                        //else if (taskType == "Schedule")
                        //{
                        //    returnValue = readSchedule(reader);
                        //}
                    }


                    // No exceptions, task has been completed
                    return returnValue;
                }
                catch (SqlException e)
                {
                    if (e.Errors.Cast<SqlError>().All(x => CanRetry(x)))
                    {
                        // What to do? Handle that here, also checking Number property.
                        // For Class < 20 you may simply Thread.Sleep(DelayOnError);
                        Thread.Sleep(2500);
                        rebuildConnection = e.Errors
                            .Cast<SqlError>()
                            .Any(x => x.Class >= 20);

                        continue;
                    }

                    throw;
                }
            }
            return null;
        }
        private ChatGroup readChatGroup(SqlDataReader _reader)
        {
            ChatGroup _group = new ChatGroup();
            if (_reader != null)
            {
                while (reader.Read())
                {                    
                    _group.Id = Convert.ToInt32(String.Format("{0}", reader[0]));
                    _group.GroupName = String.Format("{0}", reader[1]);                   
                }
            }
            connection.Close();
            return _group;
        }
        private DateTime? readComplete(SqlDataReader _reader)
        {

            DateTime compDate = new DateTime();
            if (_reader != null)
            {
                while (reader.Read())
                {
                    if (_reader[0] != DBNull.Value)
                    {
                        Debug.WriteLine("notnull");
                        compDate = Convert.ToDateTime(reader[0].ToString());
                            
                    } else
                    {
                        connection.Close();
                        return DateTime.MinValue;
                    }
                }
            }

            connection.Close();

            return compDate;
        }

        private User readUser(SqlDataReader _reader)
        {
            User _user = new User();
            if (_reader != null)
            {
                while (reader.Read())
                {
                    _user.Id = Convert.ToInt32(reader[0]);
                    _user.Name = String.Format("{0}", reader[1]);
                    _user.FacebookId = String.Format("{0}", reader[2]);
                    _user.Admin = Convert.ToBoolean(reader[3]);
                }
            }
            connection.Close();
            return _user;
        }
        private ChatGroup readToDoList(SqlDataReader _reader)
        {
            List<TodoItem> todoList = new List<TodoItem>();
            ChatGroup _chatGroup = new ChatGroup();
            if (reader != null)
            {
                while (reader.Read())
                {
                    TodoItem toDo = new TodoItem();
                    toDo.Id = String.Format("{0}", reader[0]);
                    toDo.ToDo = String.Format("{0}", reader[1]);
                    toDo.GroupId = String.Format("{0}", reader[2]);
                    toDo.Done = Convert.ToBoolean(reader[3]);
                    toDo.SendTime = String.Format("{0}", reader[4]);
                    todoList.Add(toDo);

                    


                }
            }
            _chatGroup.toDos = todoList;
            //sGroup.ToDoList.Add(toDoList);
            connection.Close();


            //coupons = await GetCouponImages(place);
            return _chatGroup;

        }
        private static bool CanRetry(SqlError error)
        {
            // Use this switch if you want to handle only well-known errors,
            // remove it if you want to always retry. A "blacklist" approach may
            // also work: return false when you're sure you can't recover from one
            // error and rely on Class for anything else.
            switch (error.Number)
            {
                case 4060:
                    Debug.WriteLine("cannot open DB");
                    break;
                case 40197:
                    Debug.WriteLine("error processing request");
                    break;
                case 40501:
                    Debug.WriteLine("services busy - retry in 10 seconds");
                    break;
                case 40613:
                    Debug.WriteLine("database  currently unavailable");
                    break;
                case 49918:
                    Debug.WriteLine("cannot process request - not enough resources");
                    break;
                case 49919:
                    Debug.WriteLine("cannot process create or update request - too many operations");
                    break;
                case 49920:
                    Debug.WriteLine("cannot process request - too many operations");
                    break;

                    // Handle well-known error codes, 
            }

            // Handle unknown errors with severity 21 or less. 22 or more
            // indicates a serious error that need to be manually fixed.
            // 24 indicates media errors. They're serious errors (that should
            // be also notified) but we may retry...
            return RetriableClasses.Contains(error.Class); // LINQ...
        }
        public async Task getFBInfo(string accessToken)
        {
            var requestUrl = "https://graph.facebook.com/v2.8/me/"
                             + "?fields=name,picture,cover,age_range,devices,email,gender,is_verified"
                             + "&access_token=" + accessToken;
            var httpClient = new HttpClient();
            var userJson = await httpClient.GetStringAsync(requestUrl);
            facebookUser = JsonConvert.DeserializeObject<FacebookUser>(userJson);
        }
    }
}
