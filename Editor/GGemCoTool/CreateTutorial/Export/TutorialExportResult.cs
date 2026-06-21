using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터 Export 결과를 표현합니다.
    /// </summary>
    internal sealed class TutorialExportResult
    {
        /// <summary>
        /// Export 성공 여부입니다.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 사용자에게 표시할 결과 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 생성되거나 갱신된 Unity 프로젝트 상대 경로입니다.
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 생성되거나 갱신된 에셋입니다.
        /// </summary>
        public Object Asset { get; }

        /// <summary>
        /// Export 결과를 생성합니다.
        /// </summary>
        /// <param name="succeeded">Export 성공 여부입니다.</param>
        /// <param name="message">사용자에게 표시할 결과 메시지입니다.</param>
        /// <param name="assetPath">생성되거나 갱신된 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="asset">생성되거나 갱신된 에셋입니다.</param>
        public TutorialExportResult(bool succeeded, string message, string assetPath, Object asset)
        {
            Succeeded = succeeded;
            Message = message;
            AssetPath = assetPath;
            Asset = asset;
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        /// <param name="message">사용자에게 표시할 결과 메시지입니다.</param>
        /// <param name="assetPath">생성되거나 갱신된 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="asset">생성되거나 갱신된 에셋입니다.</param>
        /// <returns>성공 Export 결과입니다.</returns>
        public static TutorialExportResult Success(string message, string assetPath, Object asset)
        {
            return new TutorialExportResult(true, message, assetPath, asset);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="message">사용자에게 표시할 결과 메시지입니다.</param>
        /// <returns>실패 Export 결과입니다.</returns>
        public static TutorialExportResult Failure(string message)
        {
            return new TutorialExportResult(false, message, null, null);
        }
    }
}
