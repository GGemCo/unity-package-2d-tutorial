using System;
using GGemCo2DTutorial;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial JSON TextAsset을 선택하여 파싱 결과와 필수 데이터 오류를 검증하는 도구입니다.
    /// </summary>
    public sealed class TutorialEditorWindow : EditorWindow
    {
        private TextAsset _tutorialJson;
        private Vector2 _scrollPosition;
        private string _validationMessage = "검증할 Tutorial JSON을 선택하십시오.";
        private MessageType _messageType = MessageType.Info;

        /// <summary>
        /// Tutorial JSON 검증 창을 엽니다.
        /// </summary>
        [MenuItem("GGemCo/Tutorial/Tutorial JSON Validator")]
        public static void Open()
        {
            GetWindow<TutorialEditorWindow>("Tutorial Validator");
        }

        /// <summary>
        /// Tutorial JSON 선택과 검증 결과 UI를 그립니다.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tutorial JSON Validator", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _tutorialJson = (TextAsset)EditorGUILayout.ObjectField(
                "Tutorial JSON",
                _tutorialJson,
                typeof(TextAsset),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateSelectedJson();
            }

            using (new EditorGUI.DisabledScope(_tutorialJson == null))
            {
                if (GUILayout.Button("검증"))
                {
                    ValidateSelectedJson();
                }
            }

            EditorGUILayout.HelpBox(_validationMessage, _messageType);
            if (_tutorialJson == null)
            {
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.TextArea(_tutorialJson.text, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 선택한 TextAsset을 Tutorial 정의로 역직렬화하고 공통 검증 규칙을 실행합니다.
        /// </summary>
        private void ValidateSelectedJson()
        {
            if (_tutorialJson == null)
            {
                _validationMessage = "검증할 Tutorial JSON을 선택하십시오.";
                _messageType = MessageType.Info;
                return;
            }

            try
            {
                TutorialDefinition definition =
                    JsonConvert.DeserializeObject<TutorialDefinition>(_tutorialJson.text);
                if (definition == null)
                {
                    _validationMessage = "Tutorial JSON을 해석할 수 없습니다.";
                    _messageType = MessageType.Error;
                    return;
                }

                if (TutorialDefinitionValidator.ValidateRuntime(
                        definition.uid,
                        definition,
                        out string error))
                {
                    _validationMessage =
                        $"검증에 성공했습니다. uid: {definition.uid}, steps: {definition.steps.Count}";
                    _messageType = MessageType.Info;
                    return;
                }

                _validationMessage = error;
                _messageType = MessageType.Error;
            }
            catch (Exception exception)
            {
                _validationMessage = $"JSON 파싱 오류: {exception.Message}";
                _messageType = MessageType.Error;
            }
        }
    }
}
