using System.IO;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// TutorialAuthoringAsset과 Export 파일의 이름 규칙을 계산합니다.
    /// </summary>
    internal static class TutorialAuthoringNamingUtility
    {
        private const string AuthoringAssetDirectory = "Assets/Editor/Tutorials";
        private const string ExportJsonDirectory = "Assets/GGemCo/DataAddressable/Tutorials";
        private const string AuthoringFilePrefix = "TutorialAuthoring_";
        private const string AuthoringFileExtension = ".asset";

        /// <summary>
        /// 지정한 UID에 대응되는 TutorialAuthoringAsset 파일명을 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 제작 데이터 파일명입니다.</returns>
        public static string GetAuthoringAssetFileName(int uid)
        {
            int normalizedUid = Mathf.Max(0, uid);
            return $"{AuthoringFilePrefix}{normalizedUid}{AuthoringFileExtension}";
        }

        /// <summary>
        /// 지정한 UID에 대응되는 Tutorial JSON 파일명을 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Tutorial JSON 파일명입니다.</returns>
        public static string GetDefinitionFileName(int uid)
        {
            return ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
        }

        /// <summary>
        /// 지정한 UID에 대응되는 Tutorial JSON Addressables 주소를 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Addressables 주소입니다.</returns>
        public static string GetDefinitionAddressableKey(int uid)
        {
            return ConfigAddressableKeyTutorial.GetDefinitionAddressableKey(uid);
        }

        /// <summary>
        /// 지정한 UID에 대응되는 Tutorial JSON의 프로젝트 상대 경로를 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>고정 Export 폴더가 적용된 JSON 에셋 경로입니다.</returns>
        public static string GetDefinitionJsonAssetPath(int uid)
        {
            return $"{ExportJsonDirectory}/{GetDefinitionFileName(uid)}";
        }

        /// <summary>
        /// 고정 제작 폴더에 새 TutorialAuthoringAsset을 저장할 프로젝트 상대 경로를 생성합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>중복이 보정된 Unity 프로젝트 상대 에셋 경로입니다.</returns>
        public static string BuildUniqueAuthoringAssetPath(int uid)
        {
            EnsureAssetFolder(AuthoringAssetDirectory);
            string fileName = GetAuthoringAssetFileName(uid);
            return AssetDatabase.GenerateUniqueAssetPath($"{AuthoringAssetDirectory}/{fileName}");
        }

        /// <summary>
        /// Unity AssetDatabase에 지정한 폴더와 누락된 상위 폴더를 순서대로 생성합니다.
        /// </summary>
        /// <param name="folderPath">생성할 Unity 프로젝트 상대 폴더 경로입니다.</param>
        private static void EnsureAssetFolder(string folderPath)
        {
            string normalizedPath = folderPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalizedPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(normalizedPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(normalizedPath);
            if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(folderName))
            {
                return;
            }

            EnsureAssetFolder(parentPath);
            if (!AssetDatabase.IsValidFolder(normalizedPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}
