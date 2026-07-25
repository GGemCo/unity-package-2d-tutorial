using System;
using System.Collections.Generic;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Catalog의 복합 자동 시작 조건 진행도와 재실행 준비 상태를 관리합니다.
    /// </summary>
    internal sealed class TutorialStartConditionEvaluator
    {
        private readonly Dictionary<int, int[]> _eventProgressByTutorialUid =
            new Dictionary<int, int[]>();
        private readonly HashSet<int> _armedTutorialUids =
            new HashSet<int>();
        private TutorialCatalog _catalog;

        /// <summary>
        /// 새 Catalog를 기준으로 이벤트 진행도와 자동 시작 준비 상태를 초기화합니다.
        /// </summary>
        /// <param name="catalog">자동 시작 조건을 포함하는 Tutorial Catalog입니다.</param>
        public void Initialize(TutorialCatalog catalog)
        {
            _catalog = catalog;
            _eventProgressByTutorialUid.Clear();
            _armedTutorialUids.Clear();
            if (catalog?.tutorials == null)
            {
                return;
            }

            for (int i = 0; i < catalog.tutorials.Count; i++)
            {
                TutorialCatalogEntry entry = catalog.tutorials[i];
                int conditionCount =
                    entry?.startConditions?.conditions?.Count ?? 0;
                if (entry == null || entry.uid <= 0 || conditionCount <= 0)
                {
                    continue;
                }

                _eventProgressByTutorialUid[entry.uid] =
                    new int[conditionCount];
                _armedTutorialUids.Add(entry.uid);
            }
        }

        /// <summary>
        /// 발행된 게임 이벤트와 일치하는 모든 이벤트 조건의 누적 횟수를 갱신합니다.
        /// </summary>
        /// <param name="tutorialEvent">발행된 Tutorial 게임 이벤트입니다.</param>
        public void HandleEvent(in TutorialGameEvent tutorialEvent)
        {
            if (_catalog?.tutorials == null)
            {
                return;
            }

            for (int entryIndex = 0;
                 entryIndex < _catalog.tutorials.Count;
                 entryIndex++)
            {
                TutorialCatalogEntry entry = _catalog.tutorials[entryIndex];
                List<TutorialStartConditionDefinition> conditions =
                    entry?.startConditions?.conditions;
                if (entry == null ||
                    conditions == null ||
                    !_eventProgressByTutorialUid.TryGetValue(
                        entry.uid,
                        out int[] progress))
                {
                    continue;
                }

                for (int conditionIndex = 0;
                     conditionIndex < conditions.Count;
                     conditionIndex++)
                {
                    TutorialStartConditionDefinition condition =
                        conditions[conditionIndex];
                    if (condition?.source !=
                            TutorialStartConditionSource.Event ||
                        !TutorialConditionMatcher.Matches(
                            condition,
                            tutorialEvent))
                    {
                        continue;
                    }

                    int requiredCount = Math.Max(1, condition.requiredCount);
                    int amount = Math.Max(1, tutorialEvent.Amount);
                    int next = progress[conditionIndex] + amount;
                    progress[conditionIndex] =
                        next < requiredCount ? next : requiredCount;
                }
            }
        }

        /// <summary>
        /// 지정한 Catalog 항목이 현재 자동 시작할 수 있는 상승 경계 상태인지 확인합니다.
        /// 조건이 거짓이 되면 다음 충족을 받을 수 있도록 자동으로 다시 준비합니다.
        /// </summary>
        /// <param name="entry">평가할 Tutorial Catalog 항목입니다.</param>
        /// <returns>조건이 충족되었고 아직 소비되지 않았으면 <see langword="true"/>입니다.</returns>
        public bool IsReady(TutorialCatalogEntry entry)
        {
            bool satisfied = IsSatisfied(entry);
            if (!satisfied)
            {
                if (entry != null && entry.uid > 0)
                {
                    _armedTutorialUids.Add(entry.uid);
                }

                return false;
            }

            return entry != null &&
                   _armedTutorialUids.Contains(entry.uid);
        }

        /// <summary>
        /// 자동 시작 시도를 소비하여 같은 상태가 유지되는 동안 반복 시작되지 않게 합니다.
        /// </summary>
        /// <param name="tutorialUid">시도한 Tutorial UID입니다.</param>
        public void MarkAttempted(int tutorialUid)
        {
            _armedTutorialUids.Remove(tutorialUid);
        }

        /// <summary>
        /// 비동기 로드 실패 등으로 시작하지 못한 Tutorial을 다시 시도 가능한 상태로 되돌립니다.
        /// </summary>
        /// <param name="tutorialUid">시작하지 못한 Tutorial UID입니다.</param>
        public void MarkAttemptFailed(int tutorialUid)
        {
            if (tutorialUid > 0)
            {
                _armedTutorialUids.Add(tutorialUid);
            }
        }

        /// <summary>
        /// 자동 시작에 성공한 Tutorial의 누적 이벤트 조건 횟수를 초기화합니다.
        /// 상태 조건의 상승 경계 소비 상태는 유지하여 즉시 반복 시작되는 것을 막습니다.
        /// </summary>
        /// <param name="tutorialUid">시작된 Tutorial UID입니다.</param>
        public void ResetEventProgress(int tutorialUid)
        {
            if (_eventProgressByTutorialUid.TryGetValue(
                    tutorialUid,
                    out int[] progress))
            {
                Array.Clear(progress, 0, progress.Length);
            }
        }

        /// <summary>
        /// 지정한 Catalog 항목의 All 또는 Any 복합 조건을 현재 상태 기준으로 평가합니다.
        /// </summary>
        /// <param name="entry">평가할 Catalog 항목입니다.</param>
        /// <returns>조건 그룹의 논리식이 충족되면 <see langword="true"/>입니다.</returns>
        private bool IsSatisfied(TutorialCatalogEntry entry)
        {
            List<TutorialStartConditionDefinition> conditions =
                entry?.startConditions?.conditions;
            if (entry == null ||
                conditions == null ||
                conditions.Count <= 0 ||
                !_eventProgressByTutorialUid.TryGetValue(
                    entry.uid,
                    out int[] progress))
            {
                return false;
            }

            bool matchAny =
                entry.startConditions.matchMode ==
                TutorialStartConditionMatchMode.Any;
            for (int i = 0; i < conditions.Count; i++)
            {
                TutorialStartConditionDefinition condition = conditions[i];
                bool conditionSatisfied = condition?.source ==
                    TutorialStartConditionSource.State
                    ? TutorialStartStateRegistry.IsSatisfied(condition)
                    : progress[i] >= Math.Max(
                        1,
                        condition?.requiredCount ?? 1);

                if (matchAny && conditionSatisfied)
                {
                    return true;
                }

                if (!matchAny && !conditionSatisfied)
                {
                    return false;
                }
            }

            return !matchAny;
        }
    }
}
