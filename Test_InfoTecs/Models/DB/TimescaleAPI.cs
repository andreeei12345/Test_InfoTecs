using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test_InfoTecs.Models.DB
{
    public class ValueRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double ExecutionTime { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Value { get; set; }

        [Required]
        public string FileName { get; set; }
    }

    public class ResultRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public double DeltaSeconds { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public double AvgExecutionTime { get; set; }

        [Required]
        public double AvgValue { get; set; }

        [Required]
        public double MedianValue { get; set; }

        [Required]
        public double MaxValue { get; set; }

        [Required]
        public double MinValue { get; set; }
    }
    public class ResultsFilter
    {
        public string? FileName { get; set; }
        [Column(TypeName = "timestamptz")]
        public DateTimeOffset? StartDateFrom { get; set; }
        [Column(TypeName = "timestamptz")]
        public DateTimeOffset? StartDateTo { get; set; }

        public double? AvgValueFrom { get; set; }
        public double? AvgValueTo { get; set; }

        public double? AvgExecutionTimeFrom { get; set; }
        public double? AvgExecutionTimeTo { get; set; }
    }
}