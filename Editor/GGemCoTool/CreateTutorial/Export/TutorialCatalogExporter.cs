using System;
using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// TutorialAuthoringAsset 목록을 런타임 Tutorial Catalog JSON으로 내보냅니다.
    /// </summary>
    internal static class TutorialCatalogExporter
    {
        private const string DefaultCatalogFileName = "tutorial_catalog.json";

        /// <summary>
        /// 기본 Tutorial Catalog JSON 파일명을 반환합니다.
        /// </summary>
        /// <returns>기본 Catalog JSON 파일명입니다.</returns>
        public static string GetDefaultCatalogFileName()
        {
            return DefaultCatalogFileName;
        }

        /// <summary>
        /// 제작 데이터 목록으로 Tutorial Catalog JSON을 생성합니다.
        /// </summary>
        /// <param name="assets">Catalog에 포함할 제작 데이터 목록입니다.</param>
        /// <param name="assetPath">Unity 프로젝트 상대 저장 경로입니다.</param>
        /// <returns>Export 결과입니다.</returns>
        public static TutorialExportResult ExportCatalog(
            IReadOnlyList<TutorialAuthoringAsset> assets,
            string assetPath)
        {
            TutorialAuthoringValidationResult validationResult = TutorialAuthoringValidator.ValidateCatalog(assets);
            if (!validationResult.IsValid)
            {
                return TutorialExportResult.Failure(validationResult.BuildErrorSummary());
            }

            if (!TryBuildCatalog(assets, out TutorialCatalog catalog, out string error))
            {
                return TutorialExportResult.Failure(error);
            }

            try
            {
                string json = TutorialJsonExporter.Serialize(catalog);
                return TutorialJsonExporter.WriteJsonAsset(
                    assetPath,
                    json,
                    $"Tutorial Catalog JSON을 저장했습니다. count: {catalog.tutorials.Count}");
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure($"Tutorial Catalog Export 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// Unity Selection에서 TutorialAuthoringAsset 목록을 수집합니다.
        /// </summary>
        /// <returns>중복이 제거된 제작 데이터 목록입니다.</returns>
        public static List<TutorialAuthoringAsset> CollectSelectedAuthoringAssets()
        {
            List<TutorialAuthoringAsset> result = new List<TutorialAuthoringAsset>();
            UnityEngine.Object[] selectedObjects = Selection.objects;
            if (selectedObjects == null || selectedObjects.Length <= 0)
            {
                return result;
            }

            HashSet<TutorialAuthoringAsset> uniqueAssets = new HashSet<TutorialAuthoringAsset>();
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i] is TutorialAuthoringAsset asset && asset != null && uniqueAssets.Add(asset))
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        /// <summary>
        /// 제작 데이터 목록을 Catalog DTO로 변환합니다.
        /// </summary>
        /// <param name="assets">Catalog에 포함할 제작 데이터 목록입니다.</param>
        /// <param name="catalog">생성된 Catalog DTO입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>Catalog 생성에 성공하면 true입니다.</returns>
        private static bool TryBuildCatalog(
            IReadOnlyList<TutorialAuthoringAsset> assets,
            out TutorialCatalog catalog,
            out string error)
        {
            catalog = new TutorialCatalog();
            error = null;

            if (assets == null || assets.Count <= 0)
            {
                error = "Catalog에 포함할 TutorialAuthoringAsset이 없습니다.";
                return false;
            }

            HashSet<int> usedUids = new HashSet<int>();
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
                    error = $"Catalog에 중복 Tutorial UID가 있습니다. uid: {asset.Uid}";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(asset.AddressableKey))
                {
                    error = $"Addressables Key가 비어 있습니다. uid: {asset.Uid}, asset: {asset.name}";
                    return false;
                }

                catalog.tutorials.Add(asset.ToCatalogEntry());
            }

            if (catalog.tutorials.Count <= 0)
            {
                error = "Catalog에 저장할 유효한 튜토리얼 항목이 없습니다.";
                return false;
            }

            return true;
        }
    }
}
