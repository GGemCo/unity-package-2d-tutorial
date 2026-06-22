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

            _lastValidationResult = TutorialAuthoringValidator.Validate(_asset);
            if (!_lastValidationResult.IsValid)
            {
                _statusMessage = _lastValidationResult.BuildErrorSummary();
                _statusType = MessageType.Error;
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
                result = TutorialTableExporter.Export(_asset);
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
