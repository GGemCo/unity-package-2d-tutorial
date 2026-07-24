using System.Collections.Generic;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 플레이 모드에서 외부 튜토리얼 액션 호출을 기록합니다.
    /// </summary>
    internal sealed class TutorialPlayModeTestActionLogger : ITutorialActionHandler
    {
        private static readonly TutorialPlayModeTestActionLogger Instance =
            new TutorialPlayModeTestActionLogger();
        private static readonly List<string> Logs = new List<string>();

        public static bool IsRegistered { get; private set; }
        public static IReadOnlyList<string> ActionLogs => Logs;

        public static void Register()
        {
            if (!IsRegistered)
            {
                TutorialActionHandlerRegistry.Register(Instance);
                IsRegistered = true;
            }
        }

        public static void Unregister()
        {
            TutorialActionHandlerRegistry.Unregister(Instance);
            IsRegistered = false;
        }

        public static void Clear()
        {
            Logs.Clear();
        }

        public bool TryExecute(in TutorialActionContext context)
        {
            TutorialActionDefinition action = context.Action;
            Logs.Add(
                $"Action tutorial={context.TutorialUid}, step={context.StepIndex}, " +
                $"type={action.type}, targetUid={action.targetUid}, int={action.intValue}, " +
                $"inputMask={action.inputMask}, state={action.gameplayState}, " +
                $"guidePageCount={action.guideSpriteAddresses?.Count ?? 0}, " +
                $"legacyGuideSpriteAddress={action.guideSpriteAddress}");
            return false;
        }
    }
}
