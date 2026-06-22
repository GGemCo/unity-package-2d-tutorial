using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 단계 목록과 순서 편집 영역입니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        private void DrawStepPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(StepPanelWidth)))
            {
                EditorGUILayout.LabelField("Step 목록", EditorStyles.boldLabel);
                if (_asset == null || _serializedAsset == null)
                {
                    return;
                }

                _serializedAsset.Update();
                SerializedProperty steps = _serializedAsset.FindProperty("steps");
                DrawStepToolbar(steps);
                ClampSelectedStepIndex();

                _stepScrollPosition =
                    EditorGUILayout.BeginScrollView(_stepScrollPosition);
                for (int i = 0; i < steps.arraySize; i++)
                {
                    DrawStepListItem(i, steps.GetArrayElementAtIndex(i));
                }

                EditorGUILayout.EndScrollView();
                ApplyModifiedProperties();
            }
        }

        private void DrawStepToolbar(SerializedProperty steps)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("추가"))
                {
                    AddStep();
                }

                using (new EditorGUI.DisabledScope(!HasSelectedStep(steps)))
                {
                    if (GUILayout.Button("삭제"))
                    {
                        RemoveSelectedStep(steps);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!CanMoveSelectedStep(steps, -1)))
                {
                    if (GUILayout.Button("위로"))
                    {
                        MoveSelectedStep(steps, -1);
                    }
                }

                using (new EditorGUI.DisabledScope(!CanMoveSelectedStep(steps, 1)))
                {
                    if (GUILayout.Button("아래로"))
                    {
                        MoveSelectedStep(steps, 1);
                    }
                }
            }
        }

        private void DrawStepListItem(
            int index,
            SerializedProperty step)
        {
            GUIStyle style = index == _selectedStepIndex
                ? EditorStyles.toolbarButton
                : GUI.skin.button;
            if (GUILayout.Button(BuildStepLabel(index, step), style))
            {
                _selectedStepIndex = index;
                GUI.FocusControl(null);
            }
        }

        private static string BuildStepLabel(
            int index,
            SerializedProperty step)
        {
            int uid = step.FindPropertyRelative("uid")?.intValue ?? index + 1;
            string displayName =
                step.FindPropertyRelative("displayName")?.stringValue;
            int conditionCount =
                step.FindPropertyRelative("conditions")?.arraySize ?? 0;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = $"Step {uid}";
            }

            return $"{index + 1}. [{uid}] {displayName}  조건:{conditionCount}";
        }

        private void AddStep()
        {
            Undo.RecordObject(_asset, "Add Tutorial Step");
            _asset.AddStep();
            _asset.EnsureDefaults();
            _serializedAsset = new SerializedObject(_asset);
            _selectedStepIndex = _asset.Steps.Count - 1;
            MarkAssetDirty();
        }

        private void RemoveSelectedStep(SerializedProperty steps)
        {
            if (!HasSelectedStep(steps) ||
                !EditorUtility.DisplayDialog(
                    "Step 삭제",
                    "선택한 Step을 삭제하시겠습니까?",
                    "삭제",
                    "취소"))
            {
                return;
            }

            Undo.RecordObject(_asset, "Remove Tutorial Step");
            steps.DeleteArrayElementAtIndex(_selectedStepIndex);
            ApplyModifiedProperties();
            _asset.EnsureDefaults();
            _serializedAsset = new SerializedObject(_asset);
            ClampSelectedStepIndex();
            MarkAssetDirty();
        }

        private void MoveSelectedStep(
            SerializedProperty steps,
            int direction)
        {
            if (!CanMoveSelectedStep(steps, direction))
            {
                return;
            }

            Undo.RecordObject(_asset, "Move Tutorial Step");
            int target = _selectedStepIndex + direction;
            steps.MoveArrayElement(_selectedStepIndex, target);
            _selectedStepIndex = target;
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        private bool HasSelectedStep(SerializedProperty steps)
        {
            return steps != null &&
                   _selectedStepIndex >= 0 &&
                   _selectedStepIndex < steps.arraySize;
        }

        private bool CanMoveSelectedStep(
            SerializedProperty steps,
            int direction)
        {
            if (!HasSelectedStep(steps))
            {
                return false;
            }

            int target = _selectedStepIndex + direction;
            return target >= 0 && target < steps.arraySize;
        }

        private void ClampSelectedStepIndex()
        {
            int count = _asset?.Steps?.Count ?? 0;
            _selectedStepIndex = count == 0
                ? -1
                : Mathf.Clamp(_selectedStepIndex, 0, count - 1);
        }
    }
}
