using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow 플레이 모드 테스트 명령의 실행 결과입니다.
    /// </summary>
    public readonly struct TutorialPlayModeTestResult
    {
        /// <summary>
        /// 명령이 성공했는지 여부입니다.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 사용자에게 표시할 결과 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 결과 메시지의 표시 종류입니다.
        /// </summary>
        public MessageType DisplayType => Succeeded ? MessageType.Info : MessageType.Error;

        /// <summary>
        /// 플레이 모드 테스트 결과를 생성합니다.
        /// </summary>
        /// <param name="succeeded">명령 성공 여부입니다.</param>
        /// <param name="message">표시할 결과 메시지입니다.</param>
        private TutorialPlayModeTestResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = string.IsNullOrWhiteSpace(message) ? "결과 메시지가 없습니다." : message;
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        /// <param name="message">표시할 성공 메시지입니다.</param>
        /// <returns>성공 결과입니다.</returns>
        public static TutorialPlayModeTestResult Success(string message)
        {
            return new TutorialPlayModeTestResult(true, message);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="message">표시할 실패 메시지입니다.</param>
        /// <returns>실패 결과입니다.</returns>
        public static TutorialPlayModeTestResult Failure(string message)
        {
            return new TutorialPlayModeTestResult(false, message);
        }
    }
}
