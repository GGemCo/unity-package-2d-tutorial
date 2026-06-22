namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터에서 발견한 단일 검증 항목입니다.
    /// </summary>
    internal sealed class TutorialAuthoringValidationIssue
    {
        public TutorialAuthoringValidationSeverity Severity { get; }
        public string Path { get; }
        public string Message { get; }

        /// <summary>
        /// 검증 심각도와 제작 데이터 경로를 포함한 항목을 생성합니다.
        /// </summary>
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
