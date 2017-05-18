using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoAzure
{
    class AzureDataService
    {
        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;
        public void Initialize()
        {
            connection = new SqlConnection(Constants.AzureSQLConnection);
        }

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
                        else if (taskType == "ChatGroups")
                        {
                            returnValue = readChatGroups(reader);
                        }
                        else if (taskType == "CheckId")
                        {
                            returnValue = readLastId(reader);
                        }
                        else if (taskType == "GroupUserList")
                        {
                            returnValue = readGroupUsers(reader);
                        }
                        else if (taskType == "UserList")
                        {
                            returnValue = readSingleGroupUsers(reader);
                        }
                        else if (taskType == "ToDoList")
                        {
                            returnValue = readToDoList(reader);
                        }
                        else if (taskType == "Schedule")
                        {
                            returnValue = readSchedule(reader);
                        }
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
    }
}
