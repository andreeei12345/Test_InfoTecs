using Test_InfoTecs.Models.DB;
using FluentValidation;

namespace Test_InfoTecs.Models
{
    public class ValueRecordValidator : AbstractValidator<ValueRecord>
    {
        public ValueRecordValidator()
        {
            RuleFor(x => x.Date)
                .GreaterThan(new DateTime(2000, 1, 1))
                .LessThanOrEqualTo(DateTime.UtcNow);

            RuleFor(x => x.ExecutionTime)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0);   
        }
    }

    public class ResultsFilterValidator : AbstractValidator<ResultsFilter>
    {
        public ResultsFilterValidator()
        {
            RuleFor(x => x)
                .Must(x =>
                    !x.StartDateFrom.HasValue ||
                    !x.StartDateTo.HasValue ||
                    x.StartDateFrom <= x.StartDateTo)
                .WithMessage("StartDateFrom не может быть больше StartDateTo");
        }
    }

}
