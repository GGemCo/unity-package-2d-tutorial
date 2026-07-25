using System;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 선택한 튜토리얼 단계의 조건과 액션 상세 편집 영역입니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        private void DrawStepDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Step 상세", EditorStyles.boldLabel);
                if (_asset == null || _serializedAsset == null)
                {
                    EditorGUILayout.HelpBox("제작 에셋을 선택하십시오.", MessageType.Info);
                    return;
                }

                _serializedAsset.Update();
                SerializedProperty steps = _serializedAsset.FindProperty("steps");
                if (!HasSelectedStep(steps))
                {
                    EditorGUILayout.HelpBox("편집할 Step을 선택하십시오.", MessageType.Info);
                    return;
                }

                SerializedProperty step =
                    steps.GetArrayElementAtIndex(_selectedStepIndex);
                _detailScrollPosition =
                    EditorGUILayout.BeginScrollView(_detailScrollPosition);
                DrawStepBasicProperties(step);
                DrawConditionList(step.FindPropertyRelative("conditions"));
                DrawActionList(
                    step.FindPropertyRelative("actionsOnEnter"),
                    "진입 액션");
                DrawActionList(
                    step.FindPropertyRelative("actionsOnExit"),
                    "종료 액션");
                EditorGUILayout.EndScrollView();
                ApplyModifiedProperties();
            }
        }

        private static void DrawStepBasicProperties(SerializedProperty step)
        {
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("uid"),
                new GUIContent("Step UID"));
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("displayName"),
                new GUIContent("표시 이름"));
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("messageUid"),
                new GUIContent("안내 문구 UID"));
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("guideTextPreview"),
                new GUIContent("안내 문구 미리보기"));
        }

        private void DrawConditionList(SerializedProperty conditions)
        {
            DrawListHeader("완료 조건", conditions, AddCondition);
            if (conditions == null)
            {
                return;
            }

            for (int i = 0; i < conditions.arraySize; i++)
            {
                SerializedProperty condition =
                    conditions.GetArrayElementAtIndex(i);
                DrawArrayElement(
                    conditions,
                    condition,
                    i,
                    TutorialConditionDrawerFactory.BuildTitle(condition, i),
                    ResetConditionProperty,
                    TutorialConditionDrawerFactory.Draw);
            }
        }

        private void DrawActionList(SerializedProperty actions, string title)
        {
            DrawListHeader(title, actions, () => AddAction(actions));
            if (actions == null)
            {
                return;
            }

            for (int i = 0; i < actions.arraySize; i++)
            {
                SerializedProperty action = actions.GetArrayElementAtIndex(i);
                DrawArrayElement(
                    actions,
                    action,
                    i,
                    TutorialActionDrawerFactory.BuildTitle(action, i),
                    ResetActionProperty,
                    TutorialActionDrawerFactory.Draw);
            }
        }

        private static void DrawListHeader(
            string title,
            SerializedProperty array,
            Action onAdd)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"{title} ({array?.arraySize ?? 0})",
                    EditorStyles.boldLabel);
                if (GUILayout.Button("추가", GUILayout.Width(60f)))
                {
                    onAdd?.Invoke();
                }
            }
        }

        private void DrawArrayElement(
            SerializedProperty array,
            SerializedProperty element,
            int index,
            string label,
            Action<SerializedProperty> reset,
            Action<SerializedProperty> draw)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    element.isExpanded =
                        EditorGUILayout.Foldout(element.isExpanded, label, true);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("복제", GUILayout.Width(50f)))
                    {
                        DuplicateArrayElement(array, index);
                        return;
                    }

                    if (GUILayout.Button("초기화", GUILayout.Width(60f)))
                    {
                        reset(element);
                        ScheduleGuideSynchronizationIfAction(element);
                    }

                    if (GUILayout.Button("삭제", GUILayout.Width(50f)))
                    {
                        ScheduleGuideSynchronizationIfAction(element);
                        DeleteArrayElement(array, index);
                        return;
                    }
                }

                if (element.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    draw(element);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void AddCondition()
        {
            SerializedProperty list = GetSelectedStepChildArray("conditions");
            AddArrayElement(list, ResetConditionProperty, "Add Tutorial Condition");
        }

        private void AddAction(SerializedProperty list)
        {
            AddArrayElement(list, ResetActionProperty, "Add Tutorial Action");
        }

        private void AddArrayElement(
            SerializedProperty list,
            Action<SerializedProperty> reset,
            string undoName)
        {
            if (list == null)
            {
                return;
            }

            Undo.RecordObject(_asset, undoName);
            int index = list.arraySize;
            list.InsertArrayElementAtIndex(index);
            reset(list.GetArrayElementAtIndex(index));
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        private void DuplicateArrayElement(SerializedProperty array, int index)
        {
            if (array == null || index < 0 || index >= array.arraySize)
            {
                return;
            }

            Undo.RecordObject(_asset, "Duplicate Tutorial List Element");
            array.InsertArrayElementAtIndex(index);
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        private void DeleteArrayElement(SerializedProperty array, int index)
        {
            if (array == null || index < 0 || index >= array.arraySize)
            {
                return;
            }

            Undo.RecordObject(_asset, "Delete Tutorial List Element");
            array.DeleteArrayElementAtIndex(index);
            ApplyModifiedProperties();
            MarkAssetDirty();
        }

        private SerializedProperty GetSelectedStepChildArray(string childName)
        {
            SerializedProperty steps = _serializedAsset?.FindProperty("steps");
            if (!HasSelectedStep(steps))
            {
                return null;
            }

            return steps.GetArrayElementAtIndex(_selectedStepIndex)
                .FindPropertyRelative(childName);
        }

        private static void ResetConditionProperty(SerializedProperty property)
        {
            SetEnum(
                property,
                "startSource",
                (int)TutorialStartConditionSource.Event);
            SetEnum(
                property,
                "startStateType",
                (int)TutorialStartStateType.None);
            SetEnum(property, "type", 0);
            SetInt(property, "targetUid", 0);
            SetEnum(property, "inputAction", 0);
            SetInt(property, "intValue", 0);
            SetFloat(property, "floatValue", 0f);
            SetInt(property, "requiredCount", 1);
            SetString(property, "memo", string.Empty);
            property.isExpanded = true;
        }

        private static void ResetActionProperty(SerializedProperty property)
        {
            SetEnum(property, "type", 0);
            SetInt(property, "targetUid", 0);
            SetInt(property, "intValue", 0);
            SetEnum(property, "inputMask", 0);
            SetEnum(property, "gameplayState", 0);
            SerializedProperty guidePages =
                property.FindPropertyRelative("guidePages");
            if (guidePages != null)
            {
                guidePages.arraySize = 0;
            }

            SerializedProperty guideSprites =
                property.FindPropertyRelative("guideSprites");
            if (guideSprites != null)
            {
                guideSprites.arraySize = 0;
            }

            SerializedProperty guideSpriteAddresses =
                property.FindPropertyRelative("guideSpriteAddresses");
            if (guideSpriteAddresses != null)
            {
                guideSpriteAddresses.arraySize = 0;
            }

            SetObject(property, "guideSprite", null);
            SetString(property, "guideSpriteAddress", string.Empty);
            SetString(property, "memo", string.Empty);
            property.isExpanded = true;
        }

        private static void SetEnum(
            SerializedProperty parent,
            string name,
            int value)
        {
            SerializedProperty property = parent?.FindPropertyRelative(name);
            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        private static void SetInt(
            SerializedProperty parent,
            string name,
            int value)
        {
            SerializedProperty property = parent?.FindPropertyRelative(name);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetFloat(
            SerializedProperty parent,
            string name,
            float value)
        {
            SerializedProperty property = parent?.FindPropertyRelative(name);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetString(
            SerializedProperty parent,
            string name,
            string value)
        {
            SerializedProperty property = parent?.FindPropertyRelative(name);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetObject(
            SerializedProperty parent,
            string name,
            UnityEngine.Object value)
        {
            SerializedProperty property = parent?.FindPropertyRelative(name);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        /// <summary>
        /// 대상 SerializedProperty가 액션이면 가이드 Addressables 지연 동기화를 예약합니다.
        /// </summary>
        /// <param name="property">검사할 목록 요소입니다.</param>
        private static void ScheduleGuideSynchronizationIfAction(
            SerializedProperty property)
        {
            if (property?.FindPropertyRelative("guideSprite") != null)
            {
                TutorialGuideSpriteAddressableSynchronizer.ScheduleSynchronize();
            }
        }
    }
}
