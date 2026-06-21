using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 Step 목록 패널을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 중앙 Step 목록 패널을 그립니다.
        /// </summary>
        private void DrawStepPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(StepPanelWidth)))
            {
                EditorGUILayout.LabelField("Step 목록", EditorStyles.boldLabel);

                if (_asset == null || _serializedAsset == null)
                {
                    EditorGUILayout.HelpBox("편집할 제작 데이터를 먼저 선택하십시오.", MessageType.Info);
                    return;
                }

                _serializedAsset.Update();
                SerializedProperty stepsProperty = _serializedAsset.FindProperty("steps");
                if (stepsProperty == null)
                {
                    EditorGUILayout.HelpBox("Step 목록 SerializedProperty를 찾을 수 없습니다.", MessageType.Error);
                    return;
                }

                DrawStepToolbar(stepsProperty);
                ClampSelectedStepIndex();

                _stepScrollPosition = EditorGUILayout.BeginScrollView(_stepScrollPosition);
                for (int i = 0; i < stepsProperty.arraySize; i++)
                {
                    SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(i);
                    DrawStepListItem(i, stepProperty);
                }

                EditorGUILayout.EndScrollView();
                ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Step 추가, 삭제, 이동 버튼을 표시합니다.
        /// </summary>
        /// <param name="stepsProperty">Step 목록 SerializedProperty입니다.</param>
        private void DrawStepToolbar(SerializedProperty stepsProperty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("추가"))
                {
                    AddStep();
                }

                using (new EditorGUI.DisabledScope(!HasSelectedStep(stepsProperty)))
                {
                    if (GUILayout.Button("삭제"))
                    {
                        RemoveSelectedStep(stepsProperty);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!CanMoveSelectedStep(stepsProperty, -1)))
                {
                    if (GUILayout.Button("위로"))
                    {
                        MoveSelectedStep(stepsProperty, -1);
                    }
                }

                using (new EditorGUI.DisabledScope(!CanMoveSelectedStep(stepsProperty, 1)))
                {
                    if (GUILayout.Button("아래로"))
                    {
                        MoveSelectedStep(stepsProperty, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Step 목록의 개별 항목 버튼을 그립니다.
        /// </summary>
        /// <param name="index">Step 인덱스입니다.</param>
        /// <param name="stepProperty">Step SerializedProperty입니다.</param>
        private void DrawStepListItem(int index, SerializedProperty stepProperty)
        {
            string label = BuildStepLabel(index, stepProperty);
            GUIStyle style = index == _selectedStepIndex ? EditorStyles.toolbarButton : GUI.skin.button;
            if (GUILayout.Button(label, style))
            {
                _selectedStepIndex = index;
                GUI.FocusControl(null);
            }
        }

        /// <summary>
        /// Step 목록에 표시할 라벨을 생성합니다.
        /// </summary>
        /// <param name="index">Step 인덱스입니다.</param>
        /// <param name="stepProperty">Step SerializedProperty입니다.</param>
        /// <returns>목록에 표시할 Step 라벨입니다.</returns>
        private static string BuildStepLabel(int index, SerializedProperty stepProperty)
        {
            SerializedProperty uidProperty = stepProperty.FindPropertyRelative("uid");
            SerializedProperty displayNameProperty = stepProperty.FindPropertyRelative("displayName");
            SerializedProperty conditionProperty = stepProperty.FindPropertyRelative("conditions");

            int uid = uidProperty != null ? uidProperty.intValue : index + 1;
            string displayName = displayNameProperty != null ? displayNameProperty.stringValue : string.Empty;
            int conditionCount = conditionProperty != null ? conditionProperty.arraySize : 0;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = $"Step {uid}";
            }

            return $"{index + 1}. [{uid}] {displayName}  조건:{conditionCount}";
        }

        /// <summary>
        /// 제작 데이터에 새 Step을 추가합니다.
        /// </summary>
        private void AddStep()
        {
            if (_asset == null)
            {
                return;
            }

            Undo.RecordObject(_asset, "Add Tutorial Step");
            _asset.AddStep();
            _asset.EnsureDefaults();
            _serializedAsset = new SerializedObject(_asset);
            _selectedStepIndex = _asset.Steps.Count - 1;
            MarkAssetDirty();
            _statusMessage = "새 Step을 추가했습니다.";
            _statusType = MessageType.Info;
        }

        /// <summary>
        /// 선택된 Step을 삭제합니다.
        /// </summary>
        /// <param name="stepsProperty">Step 목록 SerializedProperty입니다.</param>
        private void RemoveSelectedStep(SerializedProperty stepsProperty)
        {
            if (!HasSelectedStep(stepsProperty))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog("Step 삭제", "선택한 Step을 삭제하시겠습니까?", "삭제", "취소"))
            {
                return;
            }

            Undo.RecordObject(_asset, "Remove Tutorial Step");
            stepsProperty.DeleteArrayElementAtIndex(_selectedStepIndex);
            ApplyModifiedProperties();
            _asset.EnsureDefaults();
            _serializedAsset = new SerializedObject(_asset);
            ClampSelectedStepIndex();
            MarkAssetDirty();
            _statusMessage = "선택한 Step을 삭제했습니다.";
            _statusType = MessageType.Info;
        }

        /// <summary>
        /// 선택된 Step의 순서를 이동합니다.
        /// </summary>
        /// <param name="stepsProperty">Step 목록 SerializedProperty입니다.</param>
        /// <param name="direction">이동 방향입니다. -1은 위, 1은 아래입니다.</param>
        private void MoveSelectedStep(SerializedProperty stepsProperty, int direction)
        {
            if (!CanMoveSelectedStep(stepsProperty, direction))
            {
                return;
            }

            Undo.RecordObject(_asset, "Move Tutorial Step");
            int targetIndex = _selectedStepIndex + direction;
            stepsProperty.MoveArrayElement(_selectedStepIndex, targetIndex);
            _selectedStepIndex = targetIndex;
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        /// <summary>
        /// 현재 Step 선택 상태가 유효한지 확인합니다.
        /// </summary>
        /// <param name="stepsProperty">Step 목록 SerializedProperty입니다.</param>
        /// <returns>선택된 Step이 있으면 true입니다.</returns>
        private bool HasSelectedStep(SerializedProperty stepsProperty)
        {
            return stepsProperty != null && _selectedStepIndex >= 0 && _selectedStepIndex < stepsProperty.arraySize;
        }

        /// <summary>
        /// 선택된 Step을 지정 방향으로 이동할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="stepsProperty">Step 목록 SerializedProperty입니다.</param>
        /// <param name="direction">이동 방향입니다.</param>
        /// <returns>이동 가능하면 true입니다.</returns>
        private bool CanMoveSelectedStep(SerializedProperty stepsProperty, int direction)
        {
            if (!HasSelectedStep(stepsProperty))
            {
                return false;
            }

            int targetIndex = _selectedStepIndex + direction;
            return targetIndex >= 0 && targetIndex < stepsProperty.arraySize;
        }

        /// <summary>
        /// 선택된 Step 인덱스가 현재 Step 목록 범위를 벗어나지 않도록 보정합니다.
        /// </summary>
        private void ClampSelectedStepIndex()
        {
            if (_asset == null || _asset.Steps == null || _asset.Steps.Count <= 0)
            {
                _selectedStepIndex = -1;
                return;
            }

            if (_selectedStepIndex < 0)
            {
                _selectedStepIndex = 0;
                return;
            }

            if (_selectedStepIndex >= _asset.Steps.Count)
            {
                _selectedStepIndex = _asset.Steps.Count - 1;
            }
        }
    }
}
