using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ComplaintDTO
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string TargetName { get; set; } 
        public int TargetId { get; set; }     
        public string TargetType { get; set; } 
        public string Reason { get; set; }    
        public DateTime CreatedAt { get; set; }
    }
}
