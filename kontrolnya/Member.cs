using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public class Member
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Group {  get; set; }
        public string Status { get; set; } = "active";
        public Member(string name, int id, string group, string status)
        {
            Name = name;
            Id = id;
            Group = group;
            Status = status;
        }
    }
}
