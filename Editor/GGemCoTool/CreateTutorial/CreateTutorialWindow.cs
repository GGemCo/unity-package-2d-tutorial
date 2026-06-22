using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 Authoring Asset을 선택하고 단계와 상세 데이터를 편집하는 창입니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow : EditorWindow
    {
        private const string WindowTitle = "Create Tutorial";
        private const float StepPanelWidth = 260f;
        private const string LastAssetGuidEditorPrefsKey =
            "GGemCo2DTutorialEditor.CreateTutorialWindow.LastAssetGuid";

        private TutorialAuthoringAsset _asset;
        private SerializedObject _serializedAsset;
        private Vector2 _stepScrollPosition;
        private Vector2 _detailScrollPosition;
        private int _selectedStepIndex = -1;
        private int _playModeStartStepIndex;
        private TutorialEventType _playModeEventType = TutorialEventType.InputAction;
        private int _playModeEventTargetUid;
        private TutorialInputActionType _playModeInputAction;
        private int _playModeEventIntValue;
        private float _playModeEventFloatValue;
        private int _playModeEventAmount = 1;
        private Vector2 _playModeLogScrollPosition;
        private string _statusMessage = "Tutorial Authoring Asset을 선택하거나 생성하십시오.";
        private MessageType _statusType = MessageType.Info;
        private TutorialAuthoringValidationResult _lastValidationResult;

        [MenuItem(
            ConfigEditorTutorial.NameToolTutorial,
            false,
            (int)ConfigEditorTutorial.ToolOrdering.CreateTutorial)]
        public static void Open()
        {
            GetWindow<CreateTutorialWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            ReloadTutorialTable();
            RestoreLastAsset();
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is TutorialAuthoringAsset selected &&
                selected != _asset)
            {
                SetAsset(selected);
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawTutorialSelectionArea();
            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            EditorGUILayout.BeginHorizontal();
            DrawStepPanel();
            DrawStepDetailPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(
                    WindowTitle,
                    EditorStyles.boldLabel,
                    GUILayout.Width(160f));

                if (GUILayout.Button(
                        "JSON Import",
                        EditorStyles.toolbarButton,
                        GUILayout.Width(90f)))
                {
                    ImportTutorialJson();
                }

                using (new EditorGUI.DisabledScope(_asset == null))
                {
                    if (GUILayout.Button(
                            "기본값 보정",
                            EditorStyles.toolbarButton,
                            GUILayout.Width(90f)))
                    {
                        NormalizeAsset();
                    }

                    if (GUILayout.Button(
                            "검증",
                            EditorStyles.toolbarButton,
                            GUILayout.Width(55f)))
                    {
                        ValidateCurrentAsset();
                    }

                    if (GUILayout.Button(
                            "JSON Export",
                            EditorStyles.toolbarButton,
                            GUILayout.Width(90f)))
                    {
                        ExportCurrentTutorialJson();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawTutorialSelectionArea()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawTutorialTableSelectionPanel();
                EditorGUILayout.Space(6f);

                EditorGUI.BeginChangeCheck();
                TutorialAuthoringAsset selected =
                    (TutorialAuthoringAsset)EditorGUILayout.ObjectField(
                        "Authoring Asset",
                        _asset,
                        typeof(TutorialAuthoringAsset),
                        false);
                if (EditorGUI.EndChangeCheck())
                {
                    SetAsset(selected);
                }

                if (_asset == null || _serializedAsset == null)
                {
                    return;
                }

                _serializedAsset.Update();
                DrawProperty("enabled", "활성화");
                DrawProperty("priority", "우선순위");
                DrawProperty("repeatable", "반복 가능");
                DrawProperty("preloadPolicy", "사전 로드 정책");
                DrawStartConditionProperty();

                // 검증 및 플레이 모드 테스트 기능은 유지하되 현재 기본 레이아웃에서는 숨깁니다.
                ApplyModifiedProperties();
            }
        }

        private void DrawStartConditionProperty()
        {
            SerializedProperty property =
                _serializedAsset.FindProperty("startCondition");
            if (property == null)
            {
                return;
            }

            EditorGUILayout.LabelField("자동 시작 조건", EditorStyles.boldLabel);
            TutorialConditionDrawerFactory.Draw(property);
        }

        private void DrawProperty(string propertyName, string label)
        {
            SerializedProperty property =
                _serializedAsset.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(
                    property,
                    new GUIContent(label),
                    true);
            }
        }

        private void ApplyModifiedProperties()
        {
            if (_serializedAsset != null &&
                _serializedAsset.ApplyModifiedProperties())
            {
                MarkAssetDirty();
            }
        }

        private void SetAsset(TutorialAuthoringAsset asset)
        {
            _asset = asset;
            if (_asset != null)
            {
                _asset.EnsureDefaults();
                _selectedTutorialUid = _asset.Uid;
                _serializedAsset = new SerializedObject(_asset);
                _selectedStepIndex = _asset.Steps.Count > 0 ? 0 : -1;
                SaveLastAsset();
            }
            else
            {
                _serializedAsset = null;
                _selectedStepIndex = -1;
            }

            _lastValidationResult = null;
        }

        private void NormalizeAsset()
        {
            if (_asset == null)
            {
                return;
            }

            Undo.RecordObject(_asset, "Normalize Tutorial Authoring Asset");
            _asset.EnsureDefaults();
            MarkAssetDirty();
            _serializedAsset = new SerializedObject(_asset);
            _statusMessage = "기본값을 보정했습니다.";
            _statusType = MessageType.Info;
        }

        private void MarkAssetDirty()
        {
            if (_asset != null)
            {
                EditorUtility.SetDirty(_asset);
            }
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            Repaint();
        }

        private void SaveLastAsset()
        {
            string path = AssetDatabase.GetAssetPath(_asset);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrWhiteSpace(guid))
            {
                EditorPrefs.SetString(LastAssetGuidEditorPrefsKey, guid);
            }
        }

        private void RestoreLastAsset()
        {
            string guid = EditorPrefs.GetString(
                LastAssetGuidEditorPrefsKey,
                string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
            {
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            TutorialAuthoringAsset asset =
                AssetDatabase.LoadAssetAtPath<TutorialAuthoringAsset>(path);
            if (asset != null)
            {
                SetAsset(asset);
            }
        }
    }
}
