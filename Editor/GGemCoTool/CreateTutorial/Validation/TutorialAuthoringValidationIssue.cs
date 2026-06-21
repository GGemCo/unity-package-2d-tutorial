namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터 검증에서 발견된 단일 항목입니다.
    /// </summary>
    internal sealed class TutorialAuthoringValidationIssue
    {
        /// <summary>
        /// 검증 항목의 심각도입니다.
        /// </summary>
        public TutorialAuthoringValidationSeverity Severity { get; }

        /// <summary>
        /// 문제가 발생한 제작 데이터 경로입니다.
        /// 예: Tutorial, StartCondition, Step[1].Condition[0]
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 사용자에게 표시할 검증 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 새 검증 항목을 생성합니다.
        /// </summary>
        /// <param name="severity">검증 항목 심각도입니다.</param>
        /// <param name="path">문제가 발생한 제작 데이터 경로입니다.</param>
        /// <param name="message">사용자에게 표시할 메시지입니다.</param>
        public TutorialAuthoringValidationIssue(
            TutorialAuthoringValidationSeverity severity,
            string path,
            string message)
        {
            Severity = severity;
            Path = string.IsNullOrWhiteSpace(path) ? "Tutorial" : path;
            Message = message ?? string.Empty;
        }
    }
}
