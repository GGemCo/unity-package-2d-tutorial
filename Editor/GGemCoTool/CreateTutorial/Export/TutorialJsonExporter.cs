using System;
using System.IO;
using System.Text;
using GGemCo2DTutorial;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// TutorialAuthoringAsset을 런타임 Tutorial JSON으로 내보냅니다.
    /// </summary>
    internal static class TutorialJsonExporter
    {
        private const string JsonExtension = ".json";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// 제작 데이터의 기본 Tutorial JSON 파일명을 반환합니다.
        /// </summary>
        /// <param name="asset">파일명을 계산할 제작 데이터입니다.</param>
        /// <returns>확장자가 포함된 JSON 파일명입니다.</returns>
        public static string GetDefaultDefinitionFileName(TutorialAuthoringAsset asset)
        {
            if (asset == null)
            {
                return "tutorial.json";
            }

            string fileName = asset.ExportFileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = asset.Uid > 0 ? $"tutorial_{asset.Uid}.json" : "tutorial.json";
            }

            return EnsureJsonFileName(fileName.Trim());
        }

        /// <summary>
        /// 현재 제작 데이터를 Runtime TutorialDefinition JSON 파일로 저장합니다.
        /// </summary>
        /// <param name="asset">내보낼 제작 데이터입니다.</param>
        /// <param name="assetPath">Unity 프로젝트 상대 저장 경로입니다.</param>
        /// <returns>Export 결과입니다.</returns>
        public static TutorialExportResult ExportDefinition(
            TutorialAuthoringAsset asset,
            string assetPath)
        {
            if (asset == null)
            {
                return TutorialExportResult.Failure("내보낼 TutorialAuthoringAsset이 없습니다.");
            }

            try
            {
                asset.EnsureDefaults();
                TutorialDefinition definition = asset.ToRuntimeDefinition();
                if (!TutorialDefinitionValidator.ValidateRuntime(asset.Uid, definition, out string error))
                {
                    return TutorialExportResult.Failure(error);
                }

                string json = Serialize(definition);
                return WriteJsonAsset(
                    assetPath,
                    json,
                    $"Tutorial JSON을 저장했습니다. uid: {definition.uid}, steps: {definition.steps.Count}");
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure($"Tutorial JSON Export 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 지정 DTO를 사람이 읽기 쉬운 JSON 문자열로 직렬화합니다.
        /// </summary>
        /// <param name="value">직렬화할 DTO입니다.</param>
        /// <returns>들여쓰기가 적용된 JSON 문자열입니다.</returns>
        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

        /// <summary>
        /// JSON 문자열을 Unity 프로젝트 에셋으로 저장하고 AssetDatabase에 반영합니다.
        /// </summary>
        /// <param name="assetPath">Unity 프로젝트 상대 저장 경로입니다.</param>
        /// <param name="json">저장할 JSON 문자열입니다.</param>
        /// <param name="successMessage">성공 시 표시할 메시지입니다.</param>
        /// <returns>Export 결과입니다.</returns>
        public static TutorialExportResult WriteJsonAsset(
            string assetPath,
            string json,
            string successMessage)
        {
            if (!TryNormalizeAssetPath(assetPath, out string normalizedPath, out string error))
            {
                return TutorialExportResult.Failure(error);
            }

            try
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), normalizedPath);
                string directoryPath = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(absolutePath, json ?? string.Empty, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(normalizedPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(normalizedPath);
                return TutorialExportResult.Success(successMessage, normalizedPath, textAsset);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure($"JSON 파일 저장 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 파일명에 JSON 확장자가 없으면 확장자를 추가합니다.
        /// </summary>
        /// <param name="fileName">보정할 파일명입니다.</param>
        /// <returns>JSON 확장자가 포함된 파일명입니다.</returns>
        public static string EnsureJsonFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "tutorial.json";
            }

            return string.Equals(Path.GetExtension(fileName), JsonExtension, StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + JsonExtension;
        }

        /// <summary>
        /// Unity 프로젝트 상대 에셋 경로를 검증하고 JSON 확장자를 보정합니다.
        /// </summary>
        /// <param name="assetPath">검증할 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="normalizedPath">보정된 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="error">검증 실패 시 오류 메시지입니다.</param>
        /// <returns>유효한 경로이면 true입니다.</returns>
        private static bool TryNormalizeAssetPath(
            string assetPath,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = null;
            error = null;

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                error = "저장 경로가 비어 있습니다.";
                return false;
            }

            string path = assetPath.Trim().Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) && path != "Assets")
            {
                error = "Unity 프로젝트의 Assets 폴더 하위 경로만 사용할 수 있습니다.";
                return false;
            }

            if (path == "Assets")
            {
                error = "저장 파일명이 없습니다.";
                return false;
            }

            string directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                error = "저장 파일명이 없습니다.";
                return false;
            }

            fileName = EnsureJsonFileName(fileName);
            normalizedPath = string.IsNullOrWhiteSpace(directory) ? fileName : $"{directory}/{fileName}";
            return true;
        }
    }
}
