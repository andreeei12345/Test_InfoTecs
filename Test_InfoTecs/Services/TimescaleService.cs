using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Test_InfoTecs.Models.DB;

namespace Test_InfoTecs.Services
{
    public class TimescaleService : ITimescaleService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<ValueRecord> _validator;

        public TimescaleService(
            AppDbContext context,
            IValidator<ValueRecord> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task ProcessFileAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null
            });

            csv.Context.RegisterClassMap<ValueRecordMap>();

            var records = csv.GetRecords<ValueRecord>().ToList();

            if (records.Count < 1 || records.Count > 10000)
                throw new Exception("Количество строк должно быть от 1 до 10000.");

            foreach (var record in records)
            {
                var result = _validator.Validate(record);
                if (!result.IsValid)
                    throw new Exception(string.Join(",", result.Errors.Select(e => e.ErrorMessage)));

                record.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
                record.FileName = file.FileName;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var existingValues = _context.Values.Where(v => v.FileName == file.FileName);
            var existingResults = _context.Results.Where(r => r.FileName == file.FileName);

            _context.Values.RemoveRange(existingValues);
            _context.Results.RemoveRange(existingResults);

            await _context.Values.AddRangeAsync(records);
            await _context.SaveChangesAsync();

            var delta = (records.Max(r => r.Date) - records.Min(r => r.Date)).TotalSeconds;

            var resultRecord = new ResultRecord
            {
                FileName = file.FileName,
                DeltaSeconds = delta,
                StartDate = records.Min(r => r.Date),
                AvgExecutionTime = records.Average(r => r.ExecutionTime),
                AvgValue = records.Average(r => r.Value),
                MedianValue = GetMedian(records.Select(r => r.Value).ToList()),
                MaxValue = records.Max(r => r.Value),
                MinValue = records.Min(r => r.Value)
            };

            await _context.Results.AddAsync(resultRecord);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        public async Task<ResultRecord?> GetResultAsync(string fileName)
        {
            return await _context.Results
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.FileName == fileName);
        }
        public async Task<List<ResultRecord>> GetFilteredResultsAsync(ResultsFilter filter)
        {
            var query = _context.Results.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FileName))
                query = query.Where(r => r.FileName == filter.FileName);

            if (filter.AvgValueFrom.HasValue)
                query = query.Where(r => r.AvgValue >= filter.AvgValueFrom.Value);

            if (filter.AvgValueTo.HasValue)
                query = query.Where(r => r.AvgValue <= filter.AvgValueTo.Value);

            if (filter.AvgExecutionTimeFrom.HasValue)
                query = query.Where(r => r.AvgExecutionTime >= filter.AvgExecutionTimeFrom.Value);

            if (filter.AvgExecutionTimeTo.HasValue)
                query = query.Where(r => r.AvgExecutionTime <= filter.AvgExecutionTimeTo.Value);

            if (filter.StartDateFrom.HasValue)
            {
                var utc = DateTime.SpecifyKind(
                    filter.StartDateFrom.Value.UtcDateTime,
                    DateTimeKind.Utc);

                query = query.Where(r => r.StartDate >= utc);
            }

            if (filter.StartDateTo.HasValue)
            {
                var utc = DateTime.SpecifyKind(
                    filter.StartDateTo.Value.UtcDateTime,
                    DateTimeKind.Utc);

                query = query.Where(r => r.StartDate <= utc);
            }

            return await query.OrderBy(r => r.StartDate).ToListAsync();
        }

        public async Task<List<ValueRecord>> GetLastValuesAsync(string fileName)
        {
            return await _context.Values
                .Where(v => v.FileName == fileName)
                .OrderByDescending(v => v.Date)
                .Take(10)
                .OrderBy(v => v.Date)
                .ToListAsync();
        }
        public async Task<List<ValueRecord>> GetValuesAsync(string fileName)
        {
            return await _context.Values
                .AsNoTracking()
                .Where(v => v.FileName == fileName)
                .OrderBy(v => v.Date)
                .ToListAsync();
        }

        private static double GetMedian(List<double> values)
        {
            values.Sort();
            int count = values.Count;
            if (count % 2 == 0)
            {
                return (values[count / 2 - 1] + values[count / 2]) / 2.0;
            }
            return values[count / 2];
        }
        public sealed class ValueRecordMap : ClassMap<ValueRecord>
        {
            public ValueRecordMap()
            {
                Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd'T'HH-mm-ss.ffff'Z'");
                Map(m => m.ExecutionTime).Name("ExecutionTime");
                Map(m => m.Value).Name("Value");

            }
        }
    }
}