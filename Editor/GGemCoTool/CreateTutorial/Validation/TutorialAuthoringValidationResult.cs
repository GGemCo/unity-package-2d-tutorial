using System.Collections.Generic;
using System.Text;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터 전체 검증 결과입니다.
    /// </summary>
    internal sealed class TutorialAuthoringValidationResult
    {
        private readonly List<TutorialAuthoringValidationIssue> _issues = new List<TutorialAuthoringValidationIssue>();

        /// <summary>
        /// 발견된 검증 항목 목록입니다.
        /// </summary>
        public IReadOnlyList<TutorialAuthoringValidationIssue> Issues => _issues;

        /// <summary>
        /// 오류 항목 수입니다.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// 경고 항목 수입니다.
        /// </summary>
        public int WarningCount { get; private set; }

        /// <summary>
        /// 정보 항목 수입니다.
        /// </summary>
        public int InfoCount { get; private set; }

        /// <summary>
        /// Export를 중단해야 하는 오류가 없는지 여부입니다.
        /// </summary>
        public bool IsValid => ErrorCount <= 0;

        /// <summary>
        /// 표시할 항목이 있는지 여부입니다.
        /// </summary>
        public bool HasIssues => _issues.Count > 0;

        /// <summary>
        /// 검증 결과에 오류 항목을 추가합니다.
        /// </summary>
        /// <param name="path">문제가 발생한 제작 데이터 경로입니다.</param>
        /// <param name="message">사용자에게 표시할 메시지입니다.</param>
        public void AddError(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Error, path, message);
        }

        /// <summary>
        /// 검증 결과에 경고 항목을 추가합니다.
        /// </summary>
        /// <param name="path">문제가 발생한 제작 데이터 경로입니다.</param>
        /// <param name="message">사용자에게 표시할 메시지입니다.</param>
        public void AddWarning(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Warning, path, message);
        }

        /// <summary>
        /// 검증 결과에 정보 항목을 추가합니다.
        /// </summary>
        /// <param name="path">문제가 발생한 제작 데이터 경로입니다.</param>
        /// <param name="message">사용자에게 표시할 메시지입니다.</param>
        public void AddInfo(string path, string message)
        {
            Add(TutorialAuthoringValidationSeverity.Info, path, message);
        }

        /// <summary>
        /// 사용자에게 표시할 요약 메시지를 생성합니다.
        /// </summary>
        /// <returns>검증 결과 요약 문자열입니다.</returns>
        public string BuildSummary()
        {
            if (_issues.Count <= 0)
            {
                return "검증 결과: 문제가 없습니다.";
            }

            return $"검증 결과: 오류 {ErrorCount}개, 경고 {WarningCount}개, 정보 {InfoCount}개";
        }

        /// <summary>
        /// Export 차단 메시지에 사용할 오류 요약을 생성합니다.
        /// </summary>
        /// <param name="maxItems">표시할 최대 오류 수입니다.</param>
        /// <returns>오류 요약 문자열입니다.</returns>
        public string BuildErrorSummary(int maxItems = 5)
        {
            if (ErrorCount <= 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(BuildSummary());

            int count = 0;
            for (int i = 0; i < _issues.Count; i++)
            {
                TutorialAuthoringValidationIssue issue = _issues[i];
                if (issue == null || issue.Severity != TutorialAuthoringValidationSeverity.Error)
                {
                    continue;
                }

                builder.AppendLine();
                builder.Append("- ");
                builder.Append(issue.Path);
                builder.Append(": ");
                builder.Append(issue.Message);
                count++;

                if (count >= maxItems)
                {
                    break;
                }
            }

            if (ErrorCount > count)
            {
                builder.AppendLine();
                builder.Append("- 추가 오류 ");
                builder.Append(ErrorCount - count);
                builder.Append("개가 있습니다.");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 검증 항목을 추가하고 심각도별 카운트를 갱신합니다.
        /// </summary>
        /// <param name="severity">검증 항목 심각도입니다.</param>
        /// <param name="path">문제가 발생한 제작 데이터 경로입니다.</param>
        /// <param name="message">사용자에게 표시할 메시지입니다.</param>
        private void Add(TutorialAuthoringValidationSeverity severity, string path, string message)
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
