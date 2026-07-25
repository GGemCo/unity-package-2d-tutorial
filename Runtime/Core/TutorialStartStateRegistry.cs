using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 자동 시작 상태 변경의 종류와 대상 UID를 전달합니다.
    /// </summary>
    public readonly struct TutorialStartStateChangedEventData
    {
        public readonly TutorialStartStateType StateType;
        public readonly int TargetUid;

        /// <summary>
        /// 자동 시작 상태 변경 이벤트 데이터를 생성합니다.
        /// </summary>
        /// <param name="stateType">변경된 상태 종류입니다.</param>
        /// <param name="targetUid">변경된 상태의 대상 UID입니다.</param>
        public TutorialStartStateChangedEventData(
            TutorialStartStateType stateType,
            int targetUid)
        {
            StateType = stateType;
            TargetUid = targetUid;
        }
    }

    /// <summary>
    /// 상위 게임 계층이 전달한 현재 UI 상태와 Core 저장 상태를 자동 시작 판정에 제공합니다.
    /// </summary>
    public static class TutorialStartStateRegistry
    {
        private static readonly HashSet<int> VisibleWindowUids =
            new HashSet<int>();

        /// <summary>
        /// 자동 시작에 사용되는 상태가 변경되었을 때 발생합니다.
        /// </summary>
        public static event Action<TutorialStartStateChangedEventData> Changed;

        /// <summary>
        /// 지정한 Window UID의 현재 표시 상태를 갱신합니다.
        /// 실제 값이 변경된 경우에만 자동 시작 재평가 이벤트를 발행합니다.
        /// </summary>
        /// <param name="windowUid">Window 테이블 UID입니다.</param>
        /// <param name="visible">현재 화면에 표시 중이면 <see langword="true"/>입니다.</param>
        public static void SetWindowVisible(int windowUid, bool visible)
        {
            if (windowUid <= 0)
            {
                return;
            }

            bool changed = visible
                ? VisibleWindowUids.Add(windowUid)
                : VisibleWindowUids.Remove(windowUid);
            if (!changed)
            {
                return;
            }

            Changed?.Invoke(new TutorialStartStateChangedEventData(
                TutorialStartStateType.WindowVisible,
                windowUid));
        }

        /// <summary>
        /// 지정한 맵의 클리어 저장 상태가 변경되었음을 자동 시작 판정에 알립니다.
        /// 실제 상태 값은 Core의 MapProgressController에서 다시 조회합니다.
        /// </summary>
        /// <param name="mapUid">클리어된 TableMap UID입니다.</param>
        public static void NotifyMapCleared(int mapUid)
        {
            if (mapUid <= 0)
            {
                return;
            }

            Changed?.Invoke(new TutorialStartStateChangedEventData(
                TutorialStartStateType.MapCleared,
                mapUid));
        }

        /// <summary>
        /// 지정한 자동 시작 상태 조건이 현재 충족되었는지 확인합니다.
        /// </summary>
        /// <param name="condition">평가할 상태 조건입니다.</param>
        /// <returns>현재 상태가 조건과 일치하면 <see langword="true"/>입니다.</returns>
        internal static bool IsSatisfied(TutorialStartConditionDefinition condition)
        {
            if (condition == null ||
                condition.source != TutorialStartConditionSource.State ||
                condition.targetUid <= 0)
            {
                return false;
            }

            switch (condition.stateType)
            {
                case TutorialStartStateType.WindowVisible:
                    return VisibleWindowUids.Contains(condition.targetUid);
                case TutorialStartStateType.MapCleared:
                    return SceneGame.Instance?.saveDataManager
                               ?.MapProgressController
                               ?.IsMapCleared(condition.targetUid) == true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 플레이 모드 재시작 시 정적 상태와 구독자를 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            VisibleWindowUids.Clear();
            Changed = null;
        }
    }
}
