using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 정의의 사전 로드 정책입니다.
    /// </summary>
    public enum TutorialPreloadPolicy
    {
        None = 0,
        Boot = 1,
        MapEnter = 2,
    }

    /// <summary>
    /// 튜토리얼 카탈로그를 구성하는 테이블 행입니다.
    /// </summary>
    public sealed class StruckTableTutorial : IUidName
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public bool Enabled;
        public bool Repeatable;
        public int Priority;
        public TutorialStartConditionMatchMode StartMatchMode;
        public TutorialEventType StartEventType;
        public int StartTargetUid;
        public TutorialInputActionType StartInputAction;
        public int StartIntValue;
        public float StartFloatValue;
        public int StartRequiredCount;
        public TutorialPreloadPolicy PreloadPolicy;
        public string Memo;
    }

    /// <summary>
    /// 튜토리얼 테이블을 로드하고 시작 이벤트별 조회 캐시를 구성합니다.
    /// </summary>
    public sealed class TableTutorial : DefaultTable<StruckTableTutorial>
    {
        private readonly List<StruckTableTutorial> _enabledRows = new List<StruckTableTutorial>();
        private readonly Dictionary<TutorialEventType, List<StruckTableTutorial>> _rowsByStartEvent =
            new Dictionary<TutorialEventType, List<StruckTableTutorial>>();

        public override string Key => ConfigAddressableTableTutorial.Tutorial;

        protected override void PreLoad()
        {
            _enabledRows.Clear();
            _rowsByStartEvent.Clear();
        }

        protected override void OnLoadedData(StruckTableTutorial row)
        {
            if (row == null || row.Uid <= 0 || !row.Enabled)
            {
                return;
            }

            _enabledRows.Add(row);
            if (row.StartEventType == TutorialEventType.None)
            {
                return;
            }

            if (!_rowsByStartEvent.TryGetValue(
                    row.StartEventType,
                    out List<StruckTableTutorial> rows))
            {
                rows = new List<StruckTableTutorial>();
                _rowsByStartEvent.Add(row.StartEventType, rows);
            }

            rows.Add(row);
        }

        protected override StruckTableTutorial BuildRow(Dictionary<string, string> data)
        {
            TableRowReader reader = ReadRow(data);
            return new StruckTableTutorial
            {
                Uid = reader.Int("Uid"),
                Name = reader.String("Name"),
                Enabled = reader.BoolYN("Enabled", true),
                Repeatable = reader.BoolYN("Repeatable"),
                Priority = reader.Int("Priority"),
                StartMatchMode = reader.Enum(
                    "StartMatchMode",
                    TutorialStartConditionMatchMode.All),
                StartEventType = reader.Enum("StartEventType", TutorialEventType.None),
                StartTargetUid = reader.Int("StartTargetUid"),
                StartInputAction = reader.Enum(
                    "StartInputAction",
                    TutorialInputActionType.None),
                StartIntValue = reader.Int("StartIntValue"),
                StartFloatValue = reader.Float("StartFloatValue"),
                StartRequiredCount = Math.Max(1, reader.Int("StartRequiredCount", 1)),
                PreloadPolicy = reader.Enum(
                    "PreloadPolicy",
                    TutorialPreloadPolicy.None),
                Memo = reader.String("Memo"),
            };
        }

        /// <summary>
        /// 활성화된 튜토리얼 행을 우선순위 순으로 반환합니다.
        /// </summary>
        public IReadOnlyList<StruckTableTutorial> GetEnabledRows()
        {
            SortRows(_enabledRows);
            return _enabledRows;
        }

        /// <summary>
        /// 지정한 자동 시작 이벤트를 사용하는 행을 우선순위 순으로 반환합니다.
        /// </summary>
        public IReadOnlyList<StruckTableTutorial> GetRowsByStartEvent(TutorialEventType eventType)
        {
            if (!_rowsByStartEvent.TryGetValue(
                    eventType,
                    out List<StruckTableTutorial> rows))
            {
                return Array.Empty<StruckTableTutorial>();
            }

            SortRows(rows);
            return rows;
        }

        /// <summary>
        /// 테이블 행을 런타임 카탈로그 항목으로 변환합니다.
        /// </summary>
        /// <param name="row">변환할 Tutorial Catalog 행입니다.</param>
        /// <param name="startConditionRows">Tutorial UID에 연결된 신규 복합 시작 조건 행입니다.</param>
        /// <returns>런타임 Catalog 항목이며 유효하지 않은 행이면 null입니다.</returns>
        public static TutorialCatalogEntry ToCatalogEntry(
            StruckTableTutorial row,
            IReadOnlyList<StruckTableTutorialStartCondition> startConditionRows = null)
        {
            if (row == null || row.Uid <= 0)
            {
                return null;
            }

            TutorialConditionDefinition legacyCondition =
                BuildLegacyStartCondition(row);
            return new TutorialCatalogEntry
            {
                uid = row.Uid,
                addressableKey =
                    ConfigAddressableKeyTutorial.GetDefinitionAddressableKey(row.Uid),
                repeatable = row.Repeatable,
                startCondition = legacyCondition,
                startConditions = BuildStartConditionGroup(
                    row,
                    startConditionRows,
                    legacyCondition),
            };
        }

        private static void SortRows(List<StruckTableTutorial> rows)
        {
            rows?.Sort(static (left, right) =>
            {
                int priorityCompare = left.Priority.CompareTo(right.Priority);
                return priorityCompare != 0
                    ? priorityCompare
                    : left.Uid.CompareTo(right.Uid);
            });
        }

        /// <summary>
        /// 신규 하위 테이블 조건을 우선 사용하고, 없으면 기존 단일 컬럼을 한 개짜리 그룹으로 변환합니다.
        /// </summary>
        /// <param name="row">Tutorial Catalog 기본 행입니다.</param>
        /// <param name="conditionRows">Tutorial UID에 연결된 신규 조건 행 목록입니다.</param>
        /// <param name="legacyCondition">기존 단일 시작 조건입니다.</param>
        /// <returns>자동 시작에 사용할 조건 그룹이며 유효 조건이 없으면 null입니다.</returns>
        private static TutorialStartConditionGroup BuildStartConditionGroup(
            StruckTableTutorial row,
            IReadOnlyList<StruckTableTutorialStartCondition> conditionRows,
            TutorialConditionDefinition legacyCondition)
        {
            var group = new TutorialStartConditionGroup
            {
                matchMode = row.StartMatchMode,
            };

            if (conditionRows != null && conditionRows.Count > 0)
            {
                for (int i = 0; i < conditionRows.Count; i++)
                {
                    StruckTableTutorialStartCondition conditionRow =
                        conditionRows[i];
                    if (conditionRow == null)
                    {
                        continue;
                    }

                    group.conditions.Add(new TutorialStartConditionDefinition
                    {
                        source = conditionRow.Source,
                        eventType = conditionRow.EventType,
                        stateType = conditionRow.StateType,
                        targetUid = conditionRow.TargetUid,
                        inputAction = conditionRow.InputAction,
                        intValue = conditionRow.IntValue,
                        floatValue = conditionRow.FloatValue,
                        requiredCount = Math.Max(
                            1,
                            conditionRow.RequiredCount),
                    });
                }
            }
            else if (legacyCondition != null)
            {
                group.conditions.Add(new TutorialStartConditionDefinition
                {
                    source = TutorialStartConditionSource.Event,
                    eventType = legacyCondition.type,
                    targetUid = legacyCondition.targetUid,
                    inputAction = legacyCondition.inputAction,
                    intValue = legacyCondition.intValue,
                    floatValue = legacyCondition.floatValue,
                    requiredCount = Math.Max(
                        1,
                        legacyCondition.requiredCount),
                });
            }

            return group.conditions.Count > 0 ? group : null;
        }

        /// <summary>
        /// 기존 tutorial 테이블의 단일 자동 시작 컬럼을 런타임 조건으로 변환합니다.
        /// </summary>
        /// <param name="row">변환할 Tutorial 테이블 행입니다.</param>
        /// <returns>유효한 단일 조건이며 타입이 없으면 null입니다.</returns>
        private static TutorialConditionDefinition BuildLegacyStartCondition(
            StruckTableTutorial row)
        {
            if (row == null || row.StartEventType == TutorialEventType.None)
            {
                return null;
            }

            return new TutorialConditionDefinition
            {
                type = row.StartEventType,
                targetUid = row.StartTargetUid,
                inputAction = row.StartInputAction,
                intValue = row.StartIntValue,
                floatValue = row.StartFloatValue,
                requiredCount = Math.Max(1, row.StartRequiredCount),
            };
        }
    }
}
