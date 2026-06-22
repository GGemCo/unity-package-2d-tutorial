using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial JSON 로드 및 Authoring Asset Import 결과를 표현합니다.
    /// </summary>
    internal sealed class TutorialImportResult
    {
        /// <summary>
        /// Import 성공 여부입니다.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 사용자에게 표시할 결과 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// JSON에서 역직렬화한 Runtime Tutorial 정의입니다.
        /// </summary>
        public TutorialDefinition Definition { get; }

        /// <summary>
        /// 생성되거나 갱신된 Authoring Asset입니다.
        /// </summary>
        public TutorialAuthoringAsset Asset { get; }

        /// <summary>
        /// Tutorial Import 결과를 생성합니다.
        /// </summary>
        /// <param name="succeeded">Import 성공 여부입니다.</param>
        /// <param name="message">사용자에게 표시할 결과 메시지입니다.</param>
        /// <param name="definition">JSON에서 읽은 Runtime Tutorial 정의입니다.</param>
        /// <param name="asset">생성되거나 갱신된 Authoring Asset입니다.</param>
        private TutorialImportResult(
            bool succeeded,
            string message,
            TutorialDefinition definition,
            TutorialAuthoringAsset asset)
        {
            Succeeded = succeeded;
            Message = message;
            Definition = definition;
            Asset = asset;
        }

        /// <summary>
        /// JSON 로드 성공 결과를 생성합니다.
        /// </summary>
        /// <param name="definition">로드한 Runtime Tutorial 정의입니다.</param>
        /// <returns>정의가 포함된 성공 결과입니다.</returns>
        public static TutorialImportResult Loaded(TutorialDefinition definition)
        {
            return new TutorialImportResult(true, "Tutorial JSON을 읽었습니다.", definition, null);
        }

        /// <summary>
        /// Authoring Asset Import 성공 결과를 생성합니다.
        /// </summary>
        /// <param name="message">사용자에게 표시할 결과 메시지입니다.</param>
        /// <param name="definition">가져온 Runtime Tutorial 정의입니다.</param>
        /// <param name="asset">생성되거나 갱신된 Authoring Asset입니다.</param>
        /// <returns>에셋이 포함된 성공 결과입니다.</returns>
        public static TutorialImportResult Success(
            string message,
            TutorialDefinition definition,
            TutorialAuthoringAsset asset)
        {
            return new TutorialImportResult(true, message, definition, asset);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="message">사용자에게 표시할 오류 메시지입니다.</param>
        /// <returns>실패 결과입니다.</returns>
        public static TutorialImportResult Failure(string message)
        {
            return new TutorialImportResult(false, message, null, null);
        }
    }
}
