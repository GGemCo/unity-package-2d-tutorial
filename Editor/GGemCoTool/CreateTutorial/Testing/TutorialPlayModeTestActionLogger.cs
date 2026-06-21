using System;
using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 플레이 모드 테스트 중 외부 Tutorial 액션 호출을 기록하는 Editor 전용 처리기입니다.
    /// </summary>
    internal sealed class TutorialPlayModeTestActionLogger : ITutorialActionHandler
    {
        private const int MaxLogCount = 80;
        private static readonly TutorialPlayModeTestActionLogger Instance = new TutorialPlayModeTestActionLogger();
        private static readonly List<string> Logs = new List<string>();
        private static bool _isRegistered;

        /// <summary>
        /// 현재 테스트 액션 로거가 Runtime 레지스트리에 등록되어 있는지 여부입니다.
        /// </summary>
        public static bool IsRegistered => _isRegistered;

        /// <summary>
        /// 기록된 외부 액션 로그 목록입니다.
        /// </summary>
        public static IReadOnlyList<string> ActionLogs => Logs;

        /// <summary>
        /// 외부 액션 로그 처리기를 등록합니다.
        /// </summary>
        public static void Register()
        {
            if (_isRegistered)
            {
                return;
            }

            TutorialActionHandlerRegistry.Register(Instance);
            _isRegistered = true;
            AddLog("테스트 액션 로그 처리기를 등록했습니다.");
        }

        /// <summary>
        /// 외부 액션 로그 처리기를 해제합니다.
        /// </summary>
        public static void Unregister()
        {
            if (!_isRegistered)
            {
                return;
            }

            TutorialActionHandlerRegistry.Unregister(Instance);
            _isRegistered = false;
            AddLog("테스트 액션 로그 처리기를 해제했습니다.");
        }

        /// <summary>
        /// 기록된 액션 로그를 모두 지웁니다.
        /// </summary>
        public static void Clear()
        {
            Logs.Clear();
        }

        /// <summary>
        /// Tutorial 외부 액션을 기록하고 테스트 환경에서는 처리 완료로 간주합니다.
        /// </summary>
        /// <param name="context">실행된 Tutorial 액션 컨텍스트입니다.</param>
        /// <returns>테스트용 처리기에서 액션을 처리했으므로 true입니다.</returns>
        public bool TryExecute(in TutorialActionContext context)
        {
            TutorialActionDefinition action = context.Action;
            if (action == null)
            {
                return false;
            }

            string values = action.stringValues == null || action.stringValues.Length <= 0
                ? "-"
                : string.Join(", ", action.stringValues);
            AddLog(
                $"Action tutorial={context.TutorialUid}, step={context.StepIndex}, type={action.type}, key={action.key}, int={action.intValue}, values={values}");
            return true;
        }

        /// <summary>
        /// 로그를 최대 보관 개수에 맞춰 추가합니다.
        /// </summary>
        /// <param name="message">추가할 로그 메시지입니다.</param>
        private static void AddLog(string message)
        {
            if (Logs.Count >= MaxLogCount)
            {
                Logs.RemoveAt(0);
            }

            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        /// <summary>
        /// Unity Editor 로드 시 플레이 모드 상태 변경 콜백을 연결합니다.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeEditorCallbacks()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        /// <summary>
        /// 플레이 모드 종료 시 테스트 로그 처리기를 안전하게 해제합니다.
        /// </summary>
        /// <param name="state">변경된 플레이 모드 상태입니다.</param>
        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
            {
                Unregister();
                Clear();
            }
        }
    }
}
