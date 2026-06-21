using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// TutorialAuthoringAsset 목록을 TableTutorial txt 파일로 내보냅니다.
    /// </summary>
    internal static class TutorialTableExporter
    {
        private const string DefaultTableFileName = "tutorial.txt";
        private const string Header =
            "Uid\tName\tEnabled\tRepeatable\tPriority\tAddressableKey\tStartEventType\tStartKey\tStartIntValue\tStartRequiredCount\tPreloadPolicy\tMemo";

        /// <summary>
        /// 기본 Tutorial 테이블 파일명을 반환합니다.
        /// </summary>
        /// <returns>기본 Tutorial 테이블 파일명입니다.</returns>
        public static string GetDefaultTableFileName()
        {
            return DefaultTableFileName;
        }

        /// <summary>
        /// 제작 데이터 목록으로 Tutorial 테이블 txt 파일을 생성합니다.
        /// </summary>
        /// <param name="assets">테이블에 포함할 제작 데이터 목록입니다.</param>
        /// <param name="assetPath">Unity 프로젝트 상대 저장 경로입니다.</param>
        /// <returns>Export 결과입니다.</returns>
        public static TutorialExportResult ExportTable(
            IReadOnlyList<TutorialAuthoringAsset> assets,
            string assetPath)
        {
            TutorialAuthoringValidationResult validationResult = TutorialAuthoringValidator.ValidateCatalog(assets);
            if (!validationResult.IsValid)
            {
                return TutorialExportResult.Failure(validationResult.BuildErrorSummary());
            }

            if (!TryBuildContent(assets, out string content, out string error))
            {
                return TutorialExportResult.Failure(error);
            }

            try
            {
                if (!TryNormalizeAssetPath(assetPath, out string normalizedPath, out string pathError))
                {
                    return TutorialExportResult.Failure(pathError);
                }

                string fullPath = Path.GetFullPath(normalizedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);
                File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(normalizedPath);
                AssetDatabase.Refresh();

                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(normalizedPath);
                return TutorialExportResult.Success(
                    $"Tutorial 테이블을 저장했습니다. count: {assets.Count}",
                    normalizedPath,
                    asset);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure($"Tutorial 테이블 Export 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 제작 데이터 목록을 Tutorial 테이블 원문으로 변환합니다.
        /// </summary>
        /// <param name="assets">테이블 행으로 변환할 제작 데이터 목록입니다.</param>
        /// <param name="content">생성된 테이블 원문입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>변환에 성공하면 true입니다.</returns>
        private static bool TryBuildContent(
            IReadOnlyList<TutorialAuthoringAsset> assets,
            out string content,
            out string error)
        {
            content = null;
            error = null;

            if (assets == null || assets.Count <= 0)
            {
                error = "TableTutorial에 포함할 TutorialAuthoringAsset이 없습니다.";
                return false;
            }

            HashSet<int> usedUids = new HashSet<int>();
            StringBuilder builder = new StringBuilder(assets.Count * 96);
            builder.AppendLine(Header);
            for (int i = 0; i < assets.Count; i++)
            {
                TutorialAuthoringAsset asset = assets[i];
                if (asset == null)
                {
                    continue;
                }

                asset.EnsureDefaults();
                if (asset.Uid <= 0)
                {
                    error = $"Tutorial UID가 유효하지 않습니다. asset: {asset.name}";
                    return false;
                }

                if (!usedUids.Add(asset.Uid))
                {
                    error = $"TableTutorial에 중복 Tutorial UID가 있습니다. uid: {asset.Uid}";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(asset.AddressableKey))
                {
                    error = $"Addressables Key가 비어 있습니다. uid: {asset.Uid}, asset: {asset.name}";
                    return false;
                }

                AppendRow(builder, asset, i);
            }

            content = builder.ToString();
            return true;
        }

        /// <summary>
        /// 제작 데이터 1건을 Tutorial 테이블 행으로 추가합니다.
        /// </summary>
        /// <param name="builder">테이블 원문을 누적할 StringBuilder입니다.</param>
        /// <param name="asset">행으로 변환할 제작 데이터입니다.</param>
        /// <param name="index">선택 목록 내 순서입니다. 기본 우선순위에 사용합니다.</param>
        private static void AppendRow(StringBuilder builder, TutorialAuthoringAsset asset, int index)
        {
            TutorialAuthoringCondition startCondition = asset.StartCondition;
            TutorialEventType startEventType = startCondition != null
                ? startCondition.Type
                : TutorialEventType.None;
            string startKey = startCondition != null ? startCondition.Key : string.Empty;
            int startIntValue = startCondition != null ? startCondition.IntValue : 0;
            int requiredCount = startCondition != null ? Math.Max(1, startCondition.RequiredCount) : 1;

            builder.Append(asset.Uid).Append('\t')
                .Append(Escape(asset.Title)).Append('\t')
                .Append('Y').Append('\t')
                .Append(asset.Repeatable ? 'Y' : 'N').Append('\t')
                .Append(index).Append('\t')
                .Append(Escape(asset.AddressableKey)).Append('\t')
                .Append(startEventType).Append('\t')
                .Append(Escape(startKey)).Append('\t')
                .Append(startIntValue).Append('\t')
                .Append(requiredCount).Append('\t')
                .Append(TutorialPreloadPolicy.None).Append('\t')
                .Append(Escape(asset.Memo))
                .AppendLine();
        }

        /// <summary>
        /// 테이블 셀에서 탭/줄바꿈 문자가 행 구조를 깨지 않도록 보정합니다.
        /// </summary>
        /// <param name="value">보정할 문자열입니다.</param>
        /// <returns>테이블 셀에 쓸 수 있는 문자열입니다.</returns>
        private static string Escape(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim()
                .Replace("\t", " ")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\n");
        }

        /// <summary>
        /// Unity 프로젝트 상대 경로를 검증하고 정규화합니다.
        /// </summary>
        /// <param name="assetPath">검증할 에셋 경로입니다.</param>
        /// <param name="normalizedPath">정규화된 에셋 경로입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>유효한 프로젝트 에셋 경로이면 true입니다.</returns>
        private static bool TryNormalizeAssetPath(
            string assetPath,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = null;
            error = null;

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                error = "저장할 Tutorial 테이블 경로가 비어 있습니다.";
                return false;
            }

            string path = assetPath.Trim().Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) ||
                !string.Equals(Path.GetExtension(path), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                error = "Tutorial 테이블은 Assets 폴더 하위의 .txt 파일로 저장해야 합니다.";
                return false;
            }

            normalizedPath = path;
            return true;
        }
    }
}
