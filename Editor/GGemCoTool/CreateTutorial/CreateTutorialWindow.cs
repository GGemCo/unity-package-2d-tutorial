using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작용 Authoring Asset을 편집하는 기본 EditorWindow입니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow : EditorWindow
    {
        private const string WindowTitle = "Create Tutorial";
        private const float LeftPanelWidth = 300f;
        private const float StepPanelWidth = 260f;
        private const string LastAssetGuidEditorPrefsKey = "GGemCo2DTutorialEditor.CreateTutorialWindow.LastAssetGuid";

        private TutorialAuthoringAsset _asset;
        private SerializedObject _serializedAsset;
        private Vector2 _leftScrollPosition;
        private Vector2 _stepScrollPosition;
        private Vector2 _detailScrollPosition;
        private int _selectedStepIndex = -1;
        private string _statusMessage = "Tutorial Authoring Asset을 선택하거나 새로 생성하십시오.";
        private MessageType _statusType = MessageType.Info;
        private TutorialAuthoringValidationResult _lastValidationResult;
        private int _playModeStartStepIndex;
        private TutorialEventType _playModeEventType = TutorialEventType.InputAction;
        private string _playModeEventKey = string.Empty;
        private int _playModeEventIntValue;
        private int _playModeEventAmount = 1;
        private string _playModeInputActionId = string.Empty;
        private Vector2 _playModeLogScrollPosition;

        /// <summary>
        /// 튜토리얼 제작 창을 엽니다.
        /// </summary>
        [MenuItem(ConfigEditorTutorial.NameToolTutorial, false, (int)ConfigEditorTutorial.ToolOrdering.CreateTutorial)]
        public static void Open()
        {
            GetWindow<CreateTutorialWindow>(WindowTitle);
        }

        /// <summary>
        /// EditorWindow가 활성화될 때 마지막으로 사용한 제작 데이터를 복원합니다.
        /// </summary>
        private void OnEnable()
        {
            ReloadTutorialTable();
            RestoreLastAsset();
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        /// <summary>
        /// EditorWindow가 비활성화될 때 플레이 모드 상태 변경 구독을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        /// <summary>
        /// Unity Selection이 변경되면 선택된 TutorialAuthoringAsset을 즉시 편집 대상으로 반영합니다.
        /// </summary>
        private void OnSelectionChange()
        {
            if (Selection.activeObject is TutorialAuthoringAsset selectedAsset && selectedAsset != _asset)
            {
                SetAsset(selectedAsset);
                Repaint();
            }
        }

        /// <summary>
        /// 튜토리얼 제작 창 전체 UI를 그립니다.
        /// </summary>
        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            EditorGUILayout.BeginHorizontal();
            DrawAssetPanel();
            DrawStepPanel();
            DrawStepDetailPanel();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 제작 창 상단 도구 모음을 그립니다.
        /// </summary>
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel, GUILayout.Width(160f));

                using (new EditorGUI.DisabledScope(_asset == null))
                {
                    if (GUILayout.Button("기본값 보정", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        NormalizeAsset();
                    }

                    if (GUILayout.Button("검증", EditorStyles.toolbarButton, GUILayout.Width(55f)))
                    {
                        ValidateCurrentAsset();
                    }

                    if (GUILayout.Button("JSON Export", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        ExportCurrentTutorialJson();
                    }

                }

                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// 왼쪽 제작 데이터 선택 및 기본 정보 패널을 그립니다.
        /// </summary>
        private void DrawAssetPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftPanelWidth)))
            {
                EditorGUILayout.LabelField("Tutorial 기본 정보", EditorStyles.boldLabel);
                DrawTutorialTableSelectionPanel();
                EditorGUILayout.Space(6f);

                EditorGUI.BeginChangeCheck();
                TutorialAuthoringAsset selectedAsset = (TutorialAuthoringAsset)EditorGUILayout.ObjectField(
                    "Authoring Asset",
                    _asset,
                    typeof(TutorialAuthoringAsset),
                    false);
                if (EditorGUI.EndChangeCheck())
                {
                    SetAsset(selectedAsset);
                }

                if (_asset == null || _serializedAsset == null)
                {
                    EditorGUILayout.HelpBox("튜토리얼 제작 데이터를 선택하면 기본 정보와 Step 목록을 편집할 수 있습니다.", MessageType.Info);
                    return;
                }

                _serializedAsset.Update();
                _leftScrollPosition = EditorGUILayout.BeginScrollView(_leftScrollPosition);

                // DrawProperty("uid", "UID");
                // DrawProperty("title", "제목");
                // DrawProperty("category", "카테고리");
                // DrawProperty("memo", "제작 메모");
                // DrawProperty("repeatable", "반복 실행");
                // DrawGeneratedNamingInfo();

                EditorGUILayout.Space(8f);
                DrawStartConditionProperty();
                EditorGUILayout.Space(8f);
                DrawValidationPanel();
                EditorGUILayout.Space(8f);
                DrawPlayModeTestPanel();

                EditorGUILayout.EndScrollView();
                ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// 시작 조건 SerializedProperty를 접이식 형태로 표시합니다.
        /// </summary>
        private void DrawStartConditionProperty()
        {
            SerializedProperty property = _serializedAsset.FindProperty("startCondition");
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent("자동 시작 조건"), true);
        }

        /// <summary>
        /// 지정한 직렬화 필드를 표시합니다.
        /// </summary>
        /// <param name="propertyName">표시할 SerializedProperty 이름입니다.</param>
        /// <param name="label">화면에 표시할 라벨입니다.</param>
        private void DrawProperty(string propertyName, string label)
        {
            SerializedProperty property = _serializedAsset.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        /// <summary>
        /// UID 규칙으로 자동 계산되는 파일명과 Addressables 주소를 표시합니다.
        /// </summary>
        private void DrawGeneratedNamingInfo()
        {
            if (_asset == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("자동 생성 이름", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(
                    $"Authoring Asset: {TutorialAuthoringNamingUtility.GetAuthoringAssetFileName(_asset.Uid)}",
                    EditorStyles.wordWrappedMiniLabel,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(
                    $"Tutorial JSON: {TutorialAuthoringNamingUtility.GetDefinitionFileName(_asset.Uid)}",
                    EditorStyles.wordWrappedMiniLabel,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(
                    $"Addressables Key: {TutorialAuthoringNamingUtility.GetDefinitionAddressableKey(_asset.Uid) ?? "UID 필요"}",
                    EditorStyles.wordWrappedMiniLabel,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        /// <summary>
        /// 현재 SerializedObject 변경 사항을 적용하고 제작 데이터를 Dirty 처리합니다.
        /// </summary>
        private void ApplyModifiedProperties()
        {
            if (_serializedAsset == null)
            {
                return;
            }

            if (_serializedAsset.ApplyModifiedProperties())
            {
                MarkAssetDirty();
            }
        }

        /// <summary>
        /// 현재 편집할 제작 데이터를 교체합니다.
        /// </summary>
        /// <param name="asset">새로 편집할 제작 데이터입니다.</param>
        private void SetAsset(TutorialAuthoringAsset asset)
        {
            if (asset != null)
            {
                asset.EnsureDefaults();
                _selectedTutorialUid = asset.Uid;
            }

            _asset = asset;
            _serializedAsset = asset != null ? new SerializedObject(asset) : null;
            _selectedStepIndex = asset != null && asset.Steps != null && asset.Steps.Count > 0 ? 0 : -1;

            if (asset == null)
            {
                _statusMessage = "Tutorial Authoring Asset을 선택하거나 새로 생성하십시오.";
                _statusType = MessageType.Info;
                _lastValidationResult = null;
                return;
            }

            _lastValidationResult = null;
            SaveLastAsset(asset);
            _statusMessage = $"편집 대상: {asset.name}";
            _statusType = MessageType.Info;
        }

        /// <summary>
        /// 현재 제작 데이터의 기본값을 보정하고 저장 대상으로 표시합니다.
        /// </summary>
        private void NormalizeAsset()
        {
            if (_asset == null)
            {
                return;
            }

            Undo.RecordObject(_asset, "Normalize Tutorial Authoring Asset");
            _asset.EnsureDefaults();
            _serializedAsset = new SerializedObject(_asset);
            ClampSelectedStepIndex();
            MarkAssetDirty();
            _statusMessage = "제작 데이터 기본값을 보정했습니다.";
            _statusType = MessageType.Info;
        }

        /// <summary>
        /// 현재 제작 데이터를 Dirty 처리하여 Unity가 저장 대상으로 인식하게 합니다.
        /// </summary>
        private void MarkAssetDirty()
        {
            if (_asset == null)
            {
                return;
            }

            EditorUtility.SetDirty(_asset);
        }

        /// <summary>
        /// 마지막으로 사용한 제작 데이터 GUID를 EditorPrefs에 저장합니다.
        /// </summary>
        /// <param name="asset">저장할 제작 데이터입니다.</param>
        private static void SaveLastAsset(TutorialAuthoringAsset asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                EditorPrefs.SetString(LastAssetGuidEditorPrefsKey, guid);
            }
        }

        /// <summary>
        /// 플레이 모드 상태가 바뀔 때 테스트 패널 상태를 다시 그립니다.
        /// </summary>
        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            Repaint();
        }

        /// <summary>
        /// 마지막으로 사용한 제작 데이터를 EditorPrefs에서 복원합니다.
        /// </summary>
        private void RestoreLastAsset()
        {
            string guid = EditorPrefs.GetString(LastAssetGuidEditorPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TutorialAuthoringAsset savedAsset = AssetDatabase.LoadAssetAtPath<TutorialAuthoringAsset>(path);
            if (savedAsset != null)
            {
                SetAsset(savedAsset);
            }
        }
    }
}
