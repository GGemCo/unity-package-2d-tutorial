using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터 검증 기능을 제공합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        private void ValidateCurrentAsset()
        {
            if (_asset == null)
            {
                _statusMessage = "검증할 TutorialAuthoringAsset을 선택하십시오.";
                _statusType = MessageType.Warning;
                return;
            }

            ApplyModifiedProperties();
            _asset.EnsureDefaults();
            MarkAssetDirty();
            _lastValidationResult = TutorialAuthoringValidator.Validate(_asset);
            _statusMessage = _lastValidationResult.BuildSummary();
            _statusType = _lastValidationResult.IsValid
                ? MessageType.Info
                : MessageType.Error;
        }

        private void DrawValidationPanel()
        {
            // 현재 기본 레이아웃에서는 숨기지만 검증 기능 자체는 툴바에서 사용합니다.
        }
    }
}
