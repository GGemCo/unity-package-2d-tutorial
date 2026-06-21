using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지 외부에서 UI, Quest, 기능 잠금 등의 액션을 처리하는 확장 포트입니다.
    /// </summary>
    public interface ITutorialActionHandler
    {
        /// <summary>
        /// 지정한 튜토리얼 액션을 처리합니다.
        /// </summary>
        /// <param name="context">현재 튜토리얼과 단계 정보가 포함된 액션 컨텍스트입니다.</param>
        /// <returns>액션을 처리했으면 true를 반환합니다.</returns>
        bool TryExecute(in TutorialActionContext context);
    }

    /// <summary>
    /// 외부 액션 처리기에 전달되는 실행 컨텍스트입니다.
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
    /// 외부 Tutorial 액션 처리기를 등록하고 안전하게 순회하는 레지스트리입니다.
    /// </summary>
    public static class TutorialActionHandlerRegistry
    {
        private static readonly List<ITutorialActionHandler> Handlers = new List<ITutorialActionHandler>();

        /// <summary>
        /// 액션 처리기를 중복 없이 등록합니다.
        /// </summary>
        public static void Register(ITutorialActionHandler handler)
        {
            if (handler == null || Handlers.Contains(handler))
            {
                return;
            }

            Handlers.Add(handler);
        }

        /// <summary>
        /// 등록된 액션 처리기를 해제합니다.
        /// </summary>
        public static void Unregister(ITutorialActionHandler handler)
        {
            if (handler != null)
            {
                Handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 등록된 처리기에 액션을 전달합니다.
        /// 처리기 내부에서 레지스트리가 변경되어도 순회가 깨지지 않도록 역순으로 접근합니다.
        /// </summary>
        internal static bool Execute(in TutorialActionContext context)
        {
            bool handled = false;
            // 처리기 실행 중 등록/해제가 발생해도 현재 순회를 안정적으로 유지합니다.
            ITutorialActionHandler[] snapshot = Handlers.ToArray();
            for (int i = snapshot.Length - 1; i >= 0; i--)
            {
                ITutorialActionHandler handler = snapshot[i];
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

            return handled;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Handlers.Clear();
        }
    }

    /// <summary>
    /// 현재 튜토리얼 단계가 허용한 입력 액션 목록을 보관하고 차단 여부를 제공합니다.
    /// </summary>
    public sealed class TutorialInputBlockPolicy
    {
        private readonly HashSet<string> _allowedActionIds = new HashSet<string>();

        /// <summary>
        /// 입력 제한이 활성화되어 있는지 여부입니다.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 지정한 입력 액션이 현재 튜토리얼 정책에 의해 차단되는지 확인합니다.
        /// </summary>
        /// <param name="actionId">확인할 입력 액션 식별자입니다.</param>
        /// <returns>입력 제한이 활성화되어 있고 허용 목록에 없으면 true입니다.</returns>
        public bool IsBlocked(string actionId)
        {
            return IsActive &&
                   (string.IsNullOrWhiteSpace(actionId) || !_allowedActionIds.Contains(actionId));
        }

        /// <summary>
        /// 지정한 입력 액션만 허용하도록 정책을 갱신합니다.
        /// </summary>
        internal void BlockExcept(string[] allowedActionIds)
        {
            _allowedActionIds.Clear();
            if (allowedActionIds != null)
            {
                for (int i = 0; i < allowedActionIds.Length; i++)
                {
                    string actionId = allowedActionIds[i];
                    if (!string.IsNullOrWhiteSpace(actionId))
                    {
                        _allowedActionIds.Add(actionId);
                    }
                }
            }

            IsActive = true;
        }

        /// <summary>
        /// 모든 튜토리얼 입력 제한을 해제합니다.
        /// </summary>
        internal void Clear()
        {
            _allowedActionIds.Clear();
            IsActive = false;
        }
    }
}
