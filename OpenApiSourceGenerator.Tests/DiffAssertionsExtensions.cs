using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace NatMarchand.OpenApiSourceGenerator.Tests;

public static class DiffAssertionsExtensions
{
    public static AndConstraint<StringAssertions> HaveNoDifferencesWith(this StringAssertions assertions, string expected, string because = "", params object[] becauseArgs)
    {
        var validator = new DiffValidator(assertions.Subject, expected, because, becauseArgs);
        validator.Validate();
        return new AndConstraint<StringAssertions>(assertions);
    }

    public class DiffValidator
    {
        private static readonly char[] TrimChars = { ' ', '\r', '\n', '\t' };
        private readonly string _subject;
        private readonly string _expected;

        public IAssertionScope Assertion { get; }

        public DiffValidator(string subject, string expected, string because, object[] becauseArgs)
        {
            _subject = subject;
            _expected = expected;
            Assertion = Execute.Assertion.BecauseOf(because, becauseArgs);
        }

        public void Validate()
        {
            var r = new Regex("(\r\n{2,})", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            var expected = r.Replace(_expected, "\r\n").Trim(TrimChars);
            var subject = r.Replace(_subject, "\r\n").Trim(TrimChars);
            var diff = InlineDiffBuilder.Instance.BuildDiffModel(expected, subject, true);

            string? message = null;
            if (diff.HasDifferences)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Expected {context} to match but found the following differences :");

                foreach (var d in diff.Lines.Where(d => d.Type != ChangeType.Unchanged))
                {
                    var text = d.Text.Replace("{", "{{").Replace("}", "}}");
                    switch (d.Type)
                    {
                        case ChangeType.Deleted:
                            sb.AppendLine($"     - {text}");
                            break;
                        case ChangeType.Inserted:
                            sb.AppendLine($"{d.Position,4} + {text}");
                            break;
                        case ChangeType.Modified:
                            sb.AppendLine($"{d.Position,4} ~ {text}");
                            break;
                    }

                }
                message = sb.ToString();
            }

            Assertion.ForCondition(!diff.HasDifferences)
                .FailWith(message);
        }
    }
}