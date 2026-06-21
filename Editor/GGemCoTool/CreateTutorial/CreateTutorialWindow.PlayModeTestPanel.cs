using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 플레이 모드 테스트 UI를 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 선택한 제작 데이터와 Tutorial Runtime을 플레이 모드에서 테스트하는 패널을 그립니다.
        /// </summary>
        private void DrawPlayModeTestPanel()
        {
            EditorGUILayout.LabelField("플레이 모드 테스트", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawPlayModeRuntimeStatus();
                DrawPlayModeStartControls();
                DrawPlayModeEventControls();
                DrawPlayModeInputBlockControls();
                DrawPlayModeActionLogControls();
            }
        }

        /// <summary>
        /// 플레이 모드 Tutorial Runtime 상태 요약을 표시합니다.
        /// </summary>
        private void DrawPlayModeRuntimeStatus()
        {
            bool isPlaying = EditorApplication.isPlaying;
            EditorGUILayout.LabelField("상태", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Play Mode", isPlaying ? "실행 중" : "Edit Mode");

            if (!isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드에 진입하면 튜토리얼 시작, 이벤트 발행, 입력 차단 확인을 테스트할 수 있습니다.", MessageType.Info);
                return;
            }

            if (!TutorialPlayModeTestUtility.TryGetStatus(out TutorialPlayModeRuntimeStatus status))
            {
                EditorGUILayout.HelpBox("TutorialPackageManager 또는 TutorialManager가 아직 준비되지 않았습니다.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Runtime Ready", status.IsReady ? "Yes" : "No");
            EditorGUILayout.LabelField("Running", status.IsRunning ? "Yes" : "No");
            EditorGUILayout.LabelField("Active UID", status.ActiveTutorialUid.ToString());
            EditorGUILayout.LabelField("Step Index", status.StepIndex.ToString());
            EditorGUILayout.LabelField("Input Block", status.IsInputBlockActive ? "Active" : "Inactive");
        }

        /// <summary>
        /// 선택한 튜토리얼을 직접 또는 Catalog 경유로 시작하는 테스트 UI를 그립니다.
        /// </summary>
        private void DrawPlayModeStartControls()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("시작 테스트", EditorStyles.boldLabel);
            _playModeStartStepIndex = EditorGUILayout.IntField("시작 Step Index", _playModeStartStepIndex);
            if (_playModeStartStepIndex < 0)
            {
                _playModeStartStepIndex = 0;
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || _asset == null))
            {
                if (GUILayout.Button("현재 Authoring Asset 직접 시작"))
                {
                    ApplyPlayModeResult(TutorialPlayModeTestUtility.StartAuthoringAsset(_asset, _playModeStartStepIndex));
                }
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || _asset == null || _asset.Uid <= 0))
            {
                if (GUILayout.Button("현재 UID를 Catalog에서 재시작"))
                {
                    int tutorialUid = _asset != null ? _asset.Uid : 0;
                    TutorialPlayModeTestUtility.StartCatalogTutorialAsync(
                        tutorialUid,
                        restart: true,
                        ApplyPlayModeResult);
                }
            }
        }

        /// <summary>
        /// 임의 Tutorial 이벤트와 현재 Step 조건 이벤트를 발행하는 테스트 UI를 그립니다.
        /// </summary>
        private void DrawPlayModeEventControls()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("이벤트 발행", EditorStyles.boldLabel);
            _playModeEventType = (TutorialEventType)EditorGUILayout.EnumPopup("Event Type", _playModeEventType);
            _playModeEventKey = EditorGUILayout.TextField("Key", _playModeEventKey);
            _playModeEventIntValue = EditorGUILayout.IntField("Int Value", _playModeEventIntValue);
            _playModeEventAmount = EditorGUILayout.IntField("Amount", _playModeEventAmount);
            if (_playModeEventAmount <= 0)
            {
                _playModeEventAmount = 1;
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("이벤트 발행"))
                {
                    ApplyPlayModeResult(TutorialPlayModeTestUtility.PublishEvent(
                        _playModeEventType,
                        _playModeEventKey,
                        _playModeEventIntValue,
                        _playModeEventAmount));
                }
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || _asset == null))
            {
                if (GUILayout.Button("선택 Step의 첫 조건 이벤트 발행"))
                {
                    PublishSelectedStepFirstCondition();
                }
            }
        }

        /// <summary>
        /// TutorialInputBlockPolicy의 입력 차단 상태를 확인하는 UI를 그립니다.
        /// </summary>
        private void DrawPlayModeInputBlockControls()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("입력 차단 확인", EditorStyles.boldLabel);
            _playModeInputActionId = EditorGUILayout.TextField("Input Action ID", _playModeInputActionId);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("입력 차단 여부 확인"))
                {
                    ApplyPlayModeResult(TutorialPlayModeTestUtility.CheckInputBlocked(_playModeInputActionId));
                }
            }
        }

        /// <summary>
        /// 외부 액션 로그 처리기 등록 상태와 기록된 로그를 표시합니다.
        /// </summary>
        private void DrawPlayModeActionLogControls()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("외부 액션 로그", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Logger", TutorialPlayModeTestActionLogger.IsRegistered ? "Registered" : "Not Registered");

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("로그 처리기 등록"))
                    {
                        TutorialPlayModeTestActionLogger.Register();
                        ApplyPlayModeResult(TutorialPlayModeTestResult.Success("테스트 액션 로그 처리기를 등록했습니다."));
                    }

                    if (GUILayout.Button("해제"))
                    {
                        TutorialPlayModeTestActionLogger.Unregister();
                        ApplyPlayModeResult(TutorialPlayModeTestResult.Success("테스트 액션 로그 처리기를 해제했습니다."));
                    }
                }
            }

            if (GUILayout.Button("로그 지우기"))
            {
                TutorialPlayModeTestActionLogger.Clear();
                Repaint();
            }

            _playModeLogScrollPosition = EditorGUILayout.BeginScrollView(_playModeLogScrollPosition, GUILayout.Height(90f));
            IReadOnlyList<string> logs = TutorialPlayModeTestActionLogger.ActionLogs;
            if (logs.Count <= 0)
            {
                EditorGUILayout.LabelField("기록된 외부 액션 로그가 없습니다.", EditorStyles.miniLabel);
            }
            else
            {
                for (int i = logs.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.LabelField(logs[i], EditorStyles.wordWrappedMiniLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 현재 선택된 Step의 첫 완료 조건을 Tutorial 이벤트로 발행합니다.
        /// </summary>
        private void PublishSelectedStepFirstCondition()
        {
            TutorialAuthoringCondition condition = GetSelectedStepFirstCondition();
            if (condition == null || condition.Type == TutorialEventType.None)
            {
                ApplyPlayModeResult(TutorialPlayModeTestResult.Failure("선택한 Step에 발행 가능한 첫 조건이 없습니다."));
                return;
            }

            ApplyPlayModeResult(TutorialPlayModeTestUtility.PublishEvent(
                condition.Type,
                condition.Key,
                condition.IntValue,
                condition.RequiredCount));
        }

        /// <summary>
        /// 현재 선택된 Step에서 None이 아닌 첫 조건을 반환합니다.
        /// </summary>
        /// <returns>발행 가능한 첫 조건입니다. 없으면 null입니다.</returns>
        private TutorialAuthoringCondition GetSelectedStepFirstCondition()
        {
            if (_asset?.Steps == null || _selectedStepIndex < 0 || _selectedStepIndex >= _asset.Steps.Count)
            {
                return null;
            }

            TutorialAuthoringStep step = _asset.Steps[_selectedStepIndex];
            if (step?.Conditions == null)
            {
                return null;
            }

            for (int i = 0; i < step.Conditions.Count; i++)
            {
                TutorialAuthoringCondition condition = step.Conditions[i];
                if (condition != null && condition.Type != TutorialEventType.None)
                {
                    return condition;
                }
            }

            return null;
        }

        /// <summary>
        /// 플레이 모드 테스트 결과를 창 상태 메시지에 반영합니다.
        /// </summary>
        /// <param name="result">반영할 테스트 결과입니다.</param>
        private void ApplyPlayModeResult(TutorialPlayModeTestResult result)
        {
            _statusMessage = result.Message;
            _statusType = result.DisplayType;
            Repaint();
        }
    }
}
