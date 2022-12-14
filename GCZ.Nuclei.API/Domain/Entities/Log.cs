
namespace Domain.Entities
{
    [Table("Log")]
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        //LogLevel is type byte due to its limited enumerations.
        //A byte compared to int would shave computation time for efficiency.
        public byte LogLevel { get; set; }        
        public string Operation { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
    
}
