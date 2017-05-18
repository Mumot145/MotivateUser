using System;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MotivationUser
{
	public class TodoItem
	{
        string id;
        string group;
        string todo;
        bool done;
        bool deleted;
        string sendTime;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "text")]
        public string ToDo
        {
            get { return todo; }
            set { todo = value; }
        }
        [JsonProperty(PropertyName = "groupId")]
        public string GroupId
        {
            get { return group; }
            set { group = value; }
        }
        [JsonProperty(PropertyName = "sendTime")]
        public string SendTime
        {
            get { return sendTime; }
            set { sendTime = value; }
        }
        [JsonProperty(PropertyName = "complete")]
        public bool Done
        {
            get { return done; }
            set { done = value; }
        }
        [JsonProperty(PropertyName = "deleted")]
        public bool Deleted
        {
            get { return deleted; }
            set { deleted = value; }
        }
        [Version]
        public string Version { get; set; }
    }
}

