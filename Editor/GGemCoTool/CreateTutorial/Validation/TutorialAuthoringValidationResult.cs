using System.Collections.Generic;
using System.Text;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터의 전체 검증 결과입니다.
    /// </summary>
    internal sealed class TutorialAuthoringValidationResult
    {
        private readonly List<TutorialAuthoringValidationIssue> _issues =
            new List<TutorialAuthoringValidationIssue>();

        public IReadOnlyList<TutorialAuthoringValidationIssue> Issues => _issues;
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public int InfoCount { get; private set; }
        public bool IsValid => ErrorCount == 0;
        public bool HasIssues => _issues.Count > 0;

        public void AddError(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Error, path, message);
        }

        public void AddWarning(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Warning, path, message);
        }

        public void AddInfo(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Info, path, message);
        }

        /// <summary>
        /// 심각도별 항목 수를 요약한 메시지를 생성합니다.
        /// </summary>
        public string BuildSummary()
        {
            return _issues.Count == 0
                ? "검증 결과: 문제가 없습니다."
                : $"검증 결과: 오류 {ErrorCount}개, 경고 {WarningCount}개, 정보 {InfoCount}개";
        }

        /// <summary>
        /// Export를 차단한 오류 항목을 제한된 개수로 요약합니다.
        /// </summary>
        public string BuildErrorSummary(int maxItems = 5)
        {
            if (ErrorCount == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(BuildSummary());
            int count = 0;
            for (int i = 0; i < _issues.Count && count < maxItems; i++)
            {
                TutorialAuthoringValidationIssue issue = _issues[i];
                if (issue.Severity != TutorialAuthoringValidationSeverity.Error)
                {
                    continue;
                }

                builder.AppendLine();
                builder.Append("- ");
                builder.Append(issue.Path);
                builder.Append(": ");
                builder.Append(issue.Message);
                count++;
            }

            if (ErrorCount > count)
            {
                builder.AppendLine();
                builder.Append($"- 추가 오류 {ErrorCount - count}개가 있습니다.");
            }

            return builder.ToString();
        }

        private void Add(
            TutorialAuthoringValidationSeverity severity,
            string path,
            string message)
        {
            _issues.Add(new TutorialAuthoringValidationIssue(severity, path, message));
            switch (severity)
            {
                case TutorialAuthoringValidationSeverity.Error:
                    ErrorCount++;
                    break;
                case TutorialAuthoringValidationSeverity.Warning:
                    WarningCount++;
                    break;
                default:
                    InfoCount++;
                    break;
            }
        }
    }
}
