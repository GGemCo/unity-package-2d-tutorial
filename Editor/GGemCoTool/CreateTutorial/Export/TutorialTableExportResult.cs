using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 테이블 동기화의 성공 여부와 실제 파일 변경 여부를 표현합니다.
    /// </summary>
    internal sealed class TutorialTableExportResult
    {
        /// <summary>
        /// 테이블 동기화 성공 여부입니다.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 기존 tutorial 테이블 파일의 바이트 내용이 변경되었는지 여부입니다.
        /// </summary>
        public bool Changed { get; }

        /// <summary>
        /// 사용자에게 표시할 결과 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 동기화 대상 테이블의 Unity 프로젝트 상대 경로입니다.
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 동기화된 Tutorial 테이블 에셋입니다.
        /// </summary>
        public Object Asset { get; }

        /// <summary>
        /// Tutorial 테이블 동기화 결과를 생성합니다.
        /// </summary>
        /// <param name="succeeded">동기화 성공 여부입니다.</param>
        /// <param name="changed">실제 파일 변경 여부입니다.</param>
        /// <param name="message">사용자 안내 메시지입니다.</param>
        /// <param name="assetPath">테이블 에셋 경로입니다.</param>
        /// <param name="asset">테이블 에셋입니다.</param>
        private TutorialTableExportResult(
            bool succeeded,
            bool changed,
            string message,
            string assetPath,
            Object asset)
        {
            Succeeded = succeeded;
            Changed = changed;
            Message = message ?? string.Empty;
            AssetPath = assetPath;
            Asset = asset;
        }

        /// <summary>
        /// 성공한 테이블 동기화 결과를 생성합니다.
        /// </summary>
        /// <param name="changed">실제 파일 변경 여부입니다.</param>
        /// <param name="message">사용자 안내 메시지입니다.</param>
        /// <param name="assetPath">테이블 에셋 경로입니다.</param>
        /// <param name="asset">테이블 에셋입니다.</param>
        /// <returns>성공 결과입니다.</returns>
        public static TutorialTableExportResult Success(
            bool changed,
            string message,
            string assetPath,
            Object asset)
        {
            return new TutorialTableExportResult(
                true,
                changed,
                message,
                assetPath,
                asset);
        }

        /// <summary>
        /// 실패한 테이블 동기화 결과를 생성합니다.
        /// </summary>
        /// <param name="message">사용자 안내 메시지입니다.</param>
        /// <returns>실패 결과입니다.</returns>
        public static TutorialTableExportResult Failure(string message)
        {
            return new TutorialTableExportResult(
                false,
                false,
                message,
                null,
                null);
        }
    }
}
