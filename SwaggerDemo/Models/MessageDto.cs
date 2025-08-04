using System;

namespace SwaggerDemo.Models
{
    public class MessageDto
    {
        public string Id { get; set; }
        public string Type { get; set; }       // e.g., "Email", "SMS"
        public string Recipient { get; set; }  // e.g., email or phone number
        public string Content { get; set; }    // message body
        public string Status { get; set; }     // e.g., "Pending", "Sent", "Failed"
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
    }
}
