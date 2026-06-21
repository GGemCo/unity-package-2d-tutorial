using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 Step 상세 편집 패널을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 오른쪽 Step 상세 편집 패널을 그립니다.
        /// </summary>
        private void DrawStepDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Step 상세", EditorStyles.boldLabel);

                if (_asset == null || _serializedAsset == null)
                {
                    EditorGUILayout.HelpBox("편집할 제작 데이터를 먼저 선택하십시오.", MessageType.Info);
                    return;
                }

                _serializedAsset.Update();
                SerializedProperty stepsProperty = _serializedAsset.FindProperty("steps");
                if (!HasSelectedStep(stepsProperty))
                {
                    EditorGUILayout.HelpBox("편집할 Step을 선택하십시오.", MessageType.Info);
                    return;
                }

                SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(_selectedStepIndex);
                _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

                DrawStepBasicProperties(stepProperty);
                EditorGUILayout.Space(8f);
                DrawConditionList(stepProperty.FindPropertyRelative("conditions"));
                EditorGUILayout.Space(8f);
                DrawActionList(stepProperty.FindPropertyRelative("actionsOnEnter"), "진입 액션");
                EditorGUILayout.Space(8f);
                DrawActionList(stepProperty.FindPropertyRelative("actionsOnExit"), "종료 액션");

                EditorGUILayout.EndScrollView();
                ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Step 기본 필드를 그립니다.
        /// </summary>
        /// <param name="stepProperty">편집할 Step SerializedProperty입니다.</param>
        private static void DrawStepBasicProperties(SerializedProperty stepProperty)
        {
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("uid"), new GUIContent("Step UID"));
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("displayName"), new GUIContent("표시 이름"));
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("messageKey"), new GUIContent("안내 문구 Key"));
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("guideTextPreview"), new GUIContent("안내 문구 미리보기"));
        }

        /// <summary>
        /// Step 완료 조건 목록을 그립니다.
        /// </summary>
        /// <param name="conditionsProperty">조건 목록 SerializedProperty입니다.</param>
        private void DrawConditionList(SerializedProperty conditionsProperty)
        {
            DrawListHeader("완료 조건", conditionsProperty, AddCondition);
            if (conditionsProperty == null)
            {
                return;
            }

            for (int i = 0; i < conditionsProperty.arraySize; i++)
            {
                SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
                DrawArrayElement(
                    conditionsProperty,
                    conditionProperty,
                    i,
                    $"조건 {i + 1}",
                    ResetConditionProperty);
            }
        }

        /// <summary>
        /// Step 액션 목록을 그립니다.
        /// </summary>
        /// <param name="actionsProperty">액션 목록 SerializedProperty입니다.</param>
        /// <param name="title">목록 제목입니다.</param>
        private void DrawActionList(SerializedProperty actionsProperty, string title)
        {
            DrawListHeader(title, actionsProperty, () => AddAction(actionsProperty));
            if (actionsProperty == null)
            {
                return;
            }

            for (int i = 0; i < actionsProperty.arraySize; i++)
            {
                SerializedProperty actionProperty = actionsProperty.GetArrayElementAtIndex(i);
                DrawArrayElement(
                    actionsProperty,
                    actionProperty,
                    i,
                    $"액션 {i + 1}",
                    ResetActionProperty);
            }
        }

        /// <summary>
        /// 배열형 목록의 제목과 추가 버튼을 그립니다.
        /// </summary>
        /// <param name="title">목록 제목입니다.</param>
        /// <param name="arrayProperty">배열 SerializedProperty입니다.</param>
        /// <param name="onAdd">추가 버튼을 눌렀을 때 실행할 동작입니다.</param>
        private static void DrawListHeader(string title, SerializedProperty arrayProperty, System.Action onAdd)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                int count = arrayProperty != null ? arrayProperty.arraySize : 0;
                EditorGUILayout.LabelField($"{title} ({count})", EditorStyles.boldLabel);
                if (GUILayout.Button("추가", GUILayout.Width(60f)))
                {
                    onAdd?.Invoke();
                }
            }
        }

        /// <summary>
        /// 조건 또는 액션 배열의 개별 항목을 접이식으로 표시합니다.
        /// </summary>
        /// <param name="arrayProperty">항목이 포함된 배열 SerializedProperty입니다.</param>
        /// <param name="elementProperty">표시할 항목 SerializedProperty입니다.</param>
        /// <param name="index">항목 인덱스입니다.</param>
        /// <param name="label">항목 제목입니다.</param>
        /// <param name="resetAction">항목 초기화 함수입니다.</param>
        private void DrawArrayElement(
            SerializedProperty arrayProperty,
            SerializedProperty elementProperty,
            int index,
            string label,
            System.Action<SerializedProperty> resetAction)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    elementProperty.isExpanded = EditorGUILayout.Foldout(elementProperty.isExpanded, label, true);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("복제", GUILayout.Width(50f)))
                    {
                        DuplicateArrayElement(arrayProperty, index);
                        return;
                    }

                    if (GUILayout.Button("초기화", GUILayout.Width(60f)))
                    {
                        resetAction?.Invoke(elementProperty);
                    }

                    if (GUILayout.Button("삭제", GUILayout.Width(50f)))
                    {
                        DeleteArrayElement(arrayProperty, index);
                        return;
                    }
                }

                if (elementProperty.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(elementProperty, GUIContent.none, true);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// 선택된 Step에 조건을 추가합니다.
        /// </summary>
        private void AddCondition()
        {
            SerializedProperty listProperty = GetSelectedStepChildArray("conditions");
            if (listProperty == null)
            {
                return;
            }

            Undo.RecordObject(_asset, "Add Tutorial Condition");
            int index = listProperty.arraySize;
            listProperty.InsertArrayElementAtIndex(index);
            ResetConditionProperty(listProperty.GetArrayElementAtIndex(index));
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        /// <summary>
        /// 지정 액션 목록에 액션을 추가합니다.
        /// </summary>
        /// <param name="actionsProperty">액션 목록 SerializedProperty입니다.</param>
        private void AddAction(SerializedProperty actionsProperty)
        {
            if (actionsProperty == null)
            {
                return;
            }

            Undo.RecordObject(_asset, "Add Tutorial Action");
            int index = actionsProperty.arraySize;
            actionsProperty.InsertArrayElementAtIndex(index);
            ResetActionProperty(actionsProperty.GetArrayElementAtIndex(index));
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        /// <summary>
        /// 배열 항목을 복제합니다.
        /// </summary>
        /// <param name="arrayProperty">복제 대상 배열입니다.</param>
        /// <param name="index">복제할 항목 인덱스입니다.</param>
        private void DuplicateArrayElement(SerializedProperty arrayProperty, int index)
        {
            if (arrayProperty == null || index < 0 || index >= arrayProperty.arraySize)
            {
                return;
            }

            Undo.RecordObject(_asset, "Duplicate Tutorial List Element");
            arrayProperty.InsertArrayElementAtIndex(index);
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        /// <summary>
        /// 배열 항목을 삭제합니다.
        /// </summary>
        /// <param name="arrayProperty">삭제 대상 배열입니다.</param>
        /// <param name="index">삭제할 항목 인덱스입니다.</param>
        private void DeleteArrayElement(SerializedProperty arrayProperty, int index)
        {
            if (arrayProperty == null || index < 0 || index >= arrayProperty.arraySize)
            {
                return;
            }

            Undo.RecordObject(_asset, "Delete Tutorial List Element");
            arrayProperty.DeleteArrayElementAtIndex(index);
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        /// <summary>
        /// 선택된 Step 하위의 배열 SerializedProperty를 반환합니다.
        /// </summary>
        /// <param name="childName">Step 하위 배열 필드명입니다.</param>
        /// <returns>배열 SerializedProperty입니다.</returns>
        private SerializedProperty GetSelectedStepChildArray(string childName)
        {
            if (_serializedAsset == null)
            {
                return null;
            }

            SerializedProperty stepsProperty = _serializedAsset.FindProperty("steps");
            if (!HasSelectedStep(stepsProperty))
            {
                return null;
            }

            SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(_selectedStepIndex);
            return stepProperty.FindPropertyRelative(childName);
        }

        /// <summary>
        /// 조건 항목을 기본값으로 초기화합니다.
        /// </summary>
        /// <param name="property">초기화할 조건 SerializedProperty입니다.</param>
        private static void ResetConditionProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return;
            }

            SetEnumProperty(property, "type", 0);
            SetStringProperty(property, "key", string.Empty);
            SetIntProperty(property, "intValue", 0);
            SetIntProperty(property, "requiredCount", 1);
            SetStringProperty(property, "memo", string.Empty);
            property.isExpanded = true;
        }

        /// <summary>
        /// 액션 항목을 기본값으로 초기화합니다.
        /// </summary>
        /// <param name="property">초기화할 액션 SerializedProperty입니다.</param>
        private static void ResetActionProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return;
            }

            SetEnumProperty(property, "type", 0);
            SetStringProperty(property, "key", string.Empty);
            SetIntProperty(property, "intValue", 0);
            SetStringProperty(property, "memo", string.Empty);

            SerializedProperty stringValuesProperty = property.FindPropertyRelative("stringValues");
            if (stringValuesProperty != null)
            {
                stringValuesProperty.ClearArray();
            }

            property.isExpanded = true;
        }

        /// <summary>
        /// 하위 enum 프로퍼티 값을 설정합니다.
        /// </summary>
        /// <param name="parent">부모 SerializedProperty입니다.</param>
        /// <param name="name">하위 필드명입니다.</param>
        /// <param name="value">설정할 enum 인덱스입니다.</param>
        private static void SetEnumProperty(SerializedProperty parent, string name, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        /// <summary>
        /// 하위 문자열 프로퍼티 값을 설정합니다.
        /// </summary>
        /// <param name="parent">부모 SerializedProperty입니다.</param>
        /// <param name="name">하위 필드명입니다.</param>
        /// <param name="value">설정할 문자열입니다.</param>
        private static void SetStringProperty(SerializedProperty parent, string name, string value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        /// <summary>
        /// 하위 정수 프로퍼티 값을 설정합니다.
        /// </summary>
        /// <param name="parent">부모 SerializedProperty입니다.</param>
        /// <param name="name">하위 필드명입니다.</param>
        /// <param name="value">설정할 정수입니다.</param>
        private static void SetIntProperty(SerializedProperty parent, string name, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null)
            {
                property.intValue = value;
            }
        }
    }
}
