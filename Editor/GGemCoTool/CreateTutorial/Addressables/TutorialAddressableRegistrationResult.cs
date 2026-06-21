using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 JSON 에셋의 Addressables 등록 결과를 표현합니다.
    /// </summary>
    internal readonly struct TutorialAddressableRegistrationResult
    {
        /// <summary>
        /// Addressables 등록 성공 여부입니다.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 사용자에게 표시할 결과 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 등록된 Unity 프로젝트 상대 에셋 경로입니다.
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 등록된 Addressables 주소입니다.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 등록된 에셋 오브젝트입니다.
        /// </summary>
        public Object Asset { get; }

        /// <summary>
        /// 등록 결과를 생성합니다.
        /// </summary>
        /// <param name="succeeded">등록 성공 여부입니다.</param>
        /// <param name="message">사용자 표시 메시지입니다.</param>
        /// <param name="assetPath">등록된 에셋 경로입니다.</param>
        /// <param name="address">등록된 Addressables 주소입니다.</param>
        /// <param name="asset">등록된 에셋 오브젝트입니다.</param>
        private TutorialAddressableRegistrationResult(
            bool succeeded,
            string message,
            string assetPath,
            string address,
            Object asset)
        {
            Succeeded = succeeded;
            Message = message;
            AssetPath = assetPath;
            Address = address;
            Asset = asset;
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        /// <param name="message">성공 메시지입니다.</param>
        /// <param name="assetPath">등록된 에셋 경로입니다.</param>
        /// <param name="address">등록된 Addressables 주소입니다.</param>
        /// <param name="asset">등록된 에셋 오브젝트입니다.</param>
        /// <returns>성공 결과입니다.</returns>
        public static TutorialAddressableRegistrationResult Success(
            string message,
            string assetPath,
            string address,
            Object asset)
        {
            return new TutorialAddressableRegistrationResult(true, message, assetPath, address, asset);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="message">실패 메시지입니다.</param>
        /// <returns>실패 결과입니다.</returns>
        public static TutorialAddressableRegistrationResult Failure(string message)
        {
            return new TutorialAddressableRegistrationResult(false, message, null, null, null);
        }
    }
}
