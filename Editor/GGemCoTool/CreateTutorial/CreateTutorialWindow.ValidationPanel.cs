using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 강한 제작 데이터 검증 UI를 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 현재 선택된 제작 데이터를 검증하고 결과를 상태 메시지에 반영합니다.
        /// </summary>
        private void ValidateCurrentAsset()
        {
            if (_asset == null)
            {
                _lastValidationResult = null;
                _statusMessage = "검증할 TutorialAuthoringAsset을 먼저 선택하십시오.";
                _statusType = MessageType.Warning;
                return;
            }

            ApplyModifiedProperties();
            _asset.EnsureDefaults();
            MarkAssetDirty();

            _lastValidationResult = TutorialAuthoringValidator.Validate(_asset);
            _statusMessage = _lastValidationResult.BuildSummary();
            _statusType = GetMessageType(_lastValidationResult);
        }

        /// <summary>
        /// 마지막 검증 결과를 왼쪽 기본 정보 패널에 표시합니다.
        /// </summary>
        private void DrawValidationPanel()
        {
            EditorGUILayout.LabelField("검증 결과", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("현재 제작 데이터 검증"))
                {
                    ValidateCurrentAsset();
                }

                using (new EditorGUI.DisabledScope(_lastValidationResult == null))
                {
                    if (GUILayout.Button("결과 초기화", GUILayout.Width(90f)))
                    {
                        _lastValidationResult = null;
                        _statusMessage = "검증 결과를 초기화했습니다.";
                        _statusType = MessageType.Info;
                    }
                }
            }

            if (_lastValidationResult == null)
            {
                EditorGUILayout.HelpBox("검증 버튼을 누르면 UID, Step, 조건, 액션, 입력 차단 해제 누락 등을 검사합니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(_lastValidationResult.BuildSummary(), GetMessageType(_lastValidationResult));
            if (!_lastValidationResult.HasIssues)
            {
                return;
            }

            for (int i = 0; i < _lastValidationResult.Issues.Count; i++)
            {
                TutorialAuthoringValidationIssue issue = _lastValidationResult.Issues[i];
                if (issue == null)
                {
                    continue;
                }

                DrawValidationIssue(issue);
            }
        }

        /// <summary>
        /// 단일 검증 항목을 HelpBox로 표시합니다.
        /// </summary>
        /// <param name="issue">표시할 검증 항목입니다.</param>
        private static void DrawValidationIssue(TutorialAuthoringValidationIssue issue)
        {
            string prefix;
            switch (issue.Severity)
            {
                case TutorialAuthoringValidationSeverity.Error:
                    prefix = "오류";
                    break;
                case TutorialAuthoringValidationSeverity.Warning:
                    prefix = "경고";
                    break;
                default:
                    prefix = "정보";
                    break;
            }

            EditorGUILayout.HelpBox($"[{prefix}] {issue.Path}\n{issue.Message}", GetMessageType(issue.Severity));
        }

        /// <summary>
        /// 검증 결과의 최고 심각도를 Unity MessageType으로 변환합니다.
        /// </summary>
        /// <param name="result">변환할 검증 결과입니다.</param>
        /// <returns>Unity HelpBox에 사용할 MessageType입니다.</returns>
        private static MessageType GetMessageType(TutorialAuthoringValidationResult result)
        {
            if (result == null)
            {
                return MessageType.Info;
            }

            if (result.ErrorCount > 0)
            {
                return MessageType.Error;
            }

            return result.WarningCount > 0 ? MessageType.Warning : MessageType.Info;
        }

        /// <summary>
        /// 검증 심각도를 Unity MessageType으로 변환합니다.
        /// </summary>
        /// <param name="severity">변환할 검증 심각도입니다.</param>
        /// <returns>Unity HelpBox에 사용할 MessageType입니다.</returns>
        private static MessageType GetMessageType(TutorialAuthoringValidationSeverity severity)
        {
            switch (severity)
            {
                case TutorialAuthoringValidationSeverity.Error:
                    return MessageType.Error;
                case TutorialAuthoringValidationSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
