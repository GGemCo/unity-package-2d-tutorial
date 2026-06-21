using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 JSON Export 기능을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 현재 선택된 제작 데이터를 TutorialDefinition JSON으로 내보냅니다.
        /// </summary>
        private void ExportCurrentTutorialJson()
        {
            if (_asset == null)
            {
                ApplyExportResult(TutorialExportResult.Failure("내보낼 TutorialAuthoringAsset을 먼저 선택하십시오."));
                return;
            }

            ApplyModifiedProperties();
            _asset.EnsureDefaults();
            MarkAssetDirty();

            _lastValidationResult = TutorialAuthoringValidator.Validate(_asset);
            if (!_lastValidationResult.IsValid)
            {
                _statusMessage = _lastValidationResult.BuildErrorSummary();
                _statusType = MessageType.Error;
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Tutorial JSON Export",
                TutorialJsonExporter.GetDefaultDefinitionFileName(_asset),
                "json",
                "현재 제작 데이터를 저장할 Tutorial JSON 경로를 선택하십시오.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TutorialExportResult result = TutorialJsonExporter.ExportDefinition(_asset, path);
            ApplyExportResult(result);
        }

        /// <summary>
        /// 현재 제작 데이터 또는 Unity Selection의 제작 데이터 목록을 Tutorial Catalog JSON으로 내보냅니다.
        /// </summary>
        private void ExportCurrentCatalogJson()
        {
            List<TutorialAuthoringAsset> assets = TutorialCatalogExporter.CollectSelectedAuthoringAssets();
            if (assets.Count <= 0 && _asset != null)
            {
                assets.Add(_asset);
            }

            if (assets.Count <= 0)
            {
                ApplyExportResult(TutorialExportResult.Failure("Catalog에 포함할 TutorialAuthoringAsset이 없습니다."));
                return;
            }

            ApplyModifiedProperties();
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i] != null)
                {
                    assets[i].EnsureDefaults();
                    EditorUtility.SetDirty(assets[i]);
                }
            }

            TutorialAuthoringValidationResult catalogValidationResult = TutorialAuthoringValidator.ValidateCatalog(assets);
            if (!catalogValidationResult.IsValid)
            {
                _statusMessage = catalogValidationResult.BuildErrorSummary();
                _statusType = MessageType.Error;
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Tutorial Catalog JSON Export",
                TutorialCatalogExporter.GetDefaultCatalogFileName(),
                "json",
                "선택된 TutorialAuthoringAsset 목록을 저장할 Catalog JSON 경로를 선택하십시오.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TutorialExportResult result = TutorialCatalogExporter.ExportCatalog(assets, path);
            ApplyExportResult(result);
        }

        /// <summary>
        /// Export 결과를 상태 메시지와 프로젝트 창 선택 상태에 반영합니다.
        /// </summary>
        /// <param name="result">반영할 Export 결과입니다.</param>
        private void ApplyExportResult(TutorialExportResult result)
        {
            _statusMessage = result.Message;
            _statusType = result.Succeeded ? MessageType.Info : MessageType.Error;

            if (!result.Succeeded || result.Asset == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(result.Asset);
            Selection.activeObject = result.Asset;
        }
    }
}
