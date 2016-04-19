using System;
using System.ComponentModel.DataAnnotations;

namespace CustomExceptionHandler.Models
{
    public class ExceptionLog
    {
        [Key]
        [Required]
        [StringLength(450)]
        public string Id { get; set; }

        [Required]
        public DateTime ExDate { get; set; }

        [Required]
        [StringLength(510)]
        public string Message { get; set; }

        [Required]
        [StringLength(510)]
        public string FileName { get; set; }

        [Required]
        [StringLength(255)]
        public string Method { get; set; }

        [Required]
        public int LineNum { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(20)]
        public string ClientIp { get; set; }
    }
}
