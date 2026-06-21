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
        private const string DefaultDirectory = "Assets";
        private const string AuthoringFilePrefix = "TutorialAuthoring_";
        private const string AuthoringFileExtension = ".asset";

        /// <summary>
        /// 프로젝트에 존재하는 제작 데이터 중 가장 큰 UID 다음 값을 계산합니다.
        /// </summary>
        /// <returns>새 제작 데이터에 사용할 수 있는 UID입니다.</returns>
        public static int GetNextAvailableUid()
        {
            int maxUid = 0;
            string[] guids = AssetDatabase.FindAssets("t:TutorialAuthoringAsset");
            if (guids != null)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    TutorialAuthoringAsset asset = AssetDatabase.LoadAssetAtPath<TutorialAuthoringAsset>(path);
                    if (asset != null && asset.Uid > maxUid)
                    {
                        maxUid = asset.Uid;
                    }
                }
            }

            return maxUid + 1;
        }

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
            return TutorialAddressableKeyUtility.GetDefinitionFileName(uid);
        }

        /// <summary>
        /// 지정한 UID에 대응되는 Tutorial JSON Addressables 주소를 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Addressables 주소입니다.</returns>
        public static string GetDefinitionAddressableKey(int uid)
        {
            return TutorialAddressableKeyUtility.GetDefinitionAddressableKey(uid);
        }

        /// <summary>
        /// 현재 프로젝트 창 선택을 기준으로 새 제작 데이터 저장 폴더를 계산합니다.
        /// </summary>
        /// <returns>Unity 프로젝트 상대 폴더 경로입니다.</returns>
        public static string GetSelectedFolderOrDefault()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return DefaultDirectory;
            }

            string normalizedPath = selectedPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalizedPath))
            {
                return normalizedPath;
            }

            string directory = Path.GetDirectoryName(normalizedPath)?.Replace('\\', '/');
            return !string.IsNullOrWhiteSpace(directory) && AssetDatabase.IsValidFolder(directory)
                ? directory
                : DefaultDirectory;
        }

        /// <summary>
        /// 새 TutorialAuthoringAsset을 저장할 프로젝트 상대 경로를 생성합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>중복이 보정된 Unity 프로젝트 상대 에셋 경로입니다.</returns>
        public static string BuildUniqueAuthoringAssetPath(int uid)
        {
            string directory = GetSelectedFolderOrDefault();
            string fileName = GetAuthoringAssetFileName(uid);
            return AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}");
        }
    }
}
