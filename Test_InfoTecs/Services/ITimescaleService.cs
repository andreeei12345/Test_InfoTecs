using Test_InfoTecs.Models.DB;

namespace Test_InfoTecs.Services
{
    public interface ITimescaleService
    {
        Task ProcessFileAsync(IFormFile file);

        Task<ResultRecord?> GetResultAsync(string fileName);

        Task<List<ValueRecord>> GetValuesAsync(string fileName);

        Task<List<ResultRecord>> GetFilteredResultsAsync(ResultsFilter filter);

        Task<List<ValueRecord>> GetLastValuesAsync(string fileName);
    }
}