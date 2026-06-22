using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 표준 액션을 실제 게임 기능에 연결하는 확장 포트입니다.
    /// </summary>
    public interface ITutorialActionHandler
    {
        bool TryExecute(in TutorialActionContext context);
    }

    /// <summary>
    /// 외부 액션 처리기에 전달하는 현재 튜토리얼 실행 정보입니다.
    /// </summary>
    public readonly struct TutorialActionContext
    {
        public readonly int TutorialUid;
        public readonly int StepIndex;
        public readonly TutorialActionDefinition Action;

        /// <summary>
        /// 튜토리얼 액션 실행 컨텍스트를 생성합니다.
        /// </summary>
        public TutorialActionContext(int tutorialUid, int stepIndex, TutorialActionDefinition action)
        {
            TutorialUid = tutorialUid;
            StepIndex = stepIndex;
            Action = action;
        }
    }

    /// <summary>
    /// 외부 튜토리얼 액션 처리기를 등록하고 안전하게 순회합니다.
    /// </summary>
    public static class TutorialActionHandlerRegistry
    {
        private static readonly List<ITutorialActionHandler> Handlers = new List<ITutorialActionHandler>();
        private static readonly List<ITutorialActionHandler> Snapshot = new List<ITutorialActionHandler>();

        public static void Register(ITutorialActionHandler handler)
        {
            if (handler != null && !Handlers.Contains(handler))
            {
                Handlers.Add(handler);
            }
        }

        public static void Unregister(ITutorialActionHandler handler)
        {
            if (handler != null)
            {
                Handlers.Remove(handler);
            }
        }

        internal static bool Execute(in TutorialActionContext context)
        {
            bool handled = false;
            // 처리 중 등록 상태가 바뀌어도 순회를 안정적으로 유지하도록 재사용 스냅샷을 사용합니다.
            Snapshot.Clear();
            Snapshot.AddRange(Handlers);
            for (int i = Snapshot.Count - 1; i >= 0; i--)
            {
                ITutorialActionHandler handler = Snapshot[i];
                if (handler == null)
                {
                    continue;
                }

                try
                {
                    handled |= handler.TryExecute(context);
                }
                catch (Exception exception)
                {
                    GcLogger.LogException(exception);
                }
            }

            Snapshot.Clear();
            return handled;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Handlers.Clear();
            Snapshot.Clear();
        }
    }

    /// <summary>
    /// 현재 튜토리얼 단계에서 허용하는 입력 마스크를 보관합니다.
    /// </summary>
    public sealed class TutorialInputBlockPolicy
    {
        public bool IsActive { get; private set; }
        public TutorialInputActionMask AllowedMask { get; private set; }

        /// <summary>
        /// 지정한 표준 입력이 현재 정책에서 차단되는지 확인합니다.
        /// </summary>
        public bool IsBlocked(TutorialInputActionType inputAction)
        {
            return IsActive && !Contains(AllowedMask, inputAction);
        }

        internal void BlockExcept(TutorialInputActionMask allowedMask)
        {
            AllowedMask = allowedMask;
            IsActive = true;
        }

        internal void Clear()
        {
            AllowedMask = TutorialInputActionMask.None;
            IsActive = false;
        }

        private static bool Contains(
            TutorialInputActionMask mask,
            TutorialInputActionType inputAction)
        {
            TutorialInputActionMask value = inputAction switch
            {
                TutorialInputActionType.Move => TutorialInputActionMask.Move,
                TutorialInputActionType.Jump => TutorialInputActionMask.Jump,
                TutorialInputActionType.Guard => TutorialInputActionMask.Guard,
                TutorialInputActionType.Attack => TutorialInputActionMask.Attack,
                TutorialInputActionType.Dash => TutorialInputActionMask.Dash,
                TutorialInputActionType.Interaction => TutorialInputActionMask.Interaction,
                _ => TutorialInputActionMask.None,
            };
            return value != TutorialInputActionMask.None && (mask & value) != 0;
        }
    }
}
