using System;
using System.Collections.Generic;
using System.Text;

namespace MotivationUser.Models
{
    public class ChatGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public List<TodoItem> toDos = new List<TodoItem>();
    }
}
