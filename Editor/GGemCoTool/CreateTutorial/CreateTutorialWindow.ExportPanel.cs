using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
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

            TutorialExportResult result =
                TutorialGuideSpriteAddressableSynchronizer.Synchronize();
            if (!result.Succeeded)
            {
                ApplyExportResult(result);
                return;
            }

            Undo.RecordObject(
                _asset,
                "Generate Tutorial Localization Keys");
            if (TutorialGuideLocalizationKeyUtility.PrepareKeys(_asset))
            {
                MarkAssetDirty();
            }

            _lastValidationResult = TutorialAuthoringValidator.Validate(_asset);
            if (!_lastValidationResult.IsValid)
            {
                _statusMessage = _lastValidationResult.BuildErrorSummary();
                _statusType = MessageType.Error;
                return;
            }

            TutorialExportResult localizationResult =
                TutorialGuideLocalizationExportService.Export(_asset);
            if (!localizationResult.Succeeded)
            {
                ApplyExportResult(localizationResult);
                return;
            }

            string path = TutorialAuthoringNamingUtility.GetDefinitionJsonAssetPath(_asset.Uid);
            result = TutorialJsonExporter.ExportDefinition(_asset, path);
            if (result.Succeeded)
            {
                result = TutorialDefinitionAddressableRegistrar.Register(
                    result.AssetPath,
                    _asset.Uid);
            }

            if (result.Succeeded)
            {
                TutorialTableExportResult tableResult =
                    TutorialTableExporter.Export(_asset);
                if (!tableResult.Succeeded)
                {
                    result = TutorialExportResult.Failure(
                        tableResult.Message);
                }
                else
                {
                    // TableEditor와 같은 공유 설정을 사용하되, tutorial.txt 바이트가 달라진 경우에만 pack을 요청합니다.
                    TableEditorAutoPackResult autoPackResult =
                        TableEditorAutoPackService.TryBuildIfEnabled(
                            ConfigAddressableTableTutorial.Tutorial,
                            tableResult.Changed);
                    bool packFailed =
                        autoPackResult.Status ==
                        TableEditorAutoPackStatus.Failed;
                    string autoPackMessage =
                        autoPackResult.Status ==
                        TableEditorAutoPackStatus.SkippedUnchanged
                            ? string.Empty
                            : $"\n{autoPackResult.Message}";
                    string message =
                        $"{tableResult.Message}\n{localizationResult.Message}{autoPackMessage}";

                    if (packFailed)
                    {
                        // JSON과 테이블 저장은 완료되었으므로 롤백하지 않고 pack 불일치 가능성을 경고로 전달합니다.
                        Debug.LogWarning(
                            $"[CreateTutorial] {autoPackResult.Message}");
                        result = TutorialExportResult.Warning(
                            message,
                            tableResult.AssetPath,
                            tableResult.Asset);
                    }
                    else
                    {
                        result = TutorialExportResult.Success(
                            message,
                            tableResult.AssetPath,
                            tableResult.Asset);
                    }
                }
            }

            ApplyExportResult(result);
        }

        /// <summary>
        /// Export 결과를 상태 메시지와 프로젝트 창 선택 상태에 반영합니다.
        /// </summary>
        /// <param name="result">반영할 Export 결과입니다.</param>
        private void ApplyExportResult(TutorialExportResult result)
        {
            _statusMessage = result.Message;
            _statusType = !result.Succeeded
                ? MessageType.Error
                : result.HasWarning
                    ? MessageType.Warning
                    : MessageType.Info;

            if (!result.Succeeded || result.Asset == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(result.Asset);
            Selection.activeObject = result.Asset;
        }
    }
}
