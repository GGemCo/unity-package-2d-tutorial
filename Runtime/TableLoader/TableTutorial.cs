using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 테이블의 프리로드 정책입니다.
    /// </summary>
    public enum TutorialPreloadPolicy
    {
        None = 0,
        Boot = 1,
        MapEnter = 2,
    }

    /// <summary>
    /// Tutorial Catalog 역할을 수행하는 테이블 행 데이터입니다.
    /// </summary>
    public sealed class StruckTableTutorial : IUidName
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public bool Enabled;
        public bool Repeatable;
        public int Priority;
        public TutorialEventType StartEventType;
        public string StartKey;
        public int StartIntValue;
        public int StartRequiredCount;
        public TutorialPreloadPolicy PreloadPolicy;
        public string Memo;
    }

    /// <summary>
    /// Tutorial Catalog 역할을 수행하는 테이블입니다.
    /// </summary>
    public sealed class TableTutorial : DefaultTable<StruckTableTutorial>
    {
        private static readonly List<StruckTableTutorial> EnabledRows = new List<StruckTableTutorial>();
        private static readonly Dictionary<TutorialEventType, List<StruckTableTutorial>> RowsByStartEvent =
            new Dictionary<TutorialEventType, List<StruckTableTutorial>>();

        /// <summary>
        /// Tutorial 테이블 Addressables 키입니다.
        /// </summary>
        public override string Key => ConfigAddressableTableTutorial.Tutorial;

        /// <summary>
        /// 테이블 재적재 전에 자동 시작 인덱스를 초기화합니다.
        /// </summary>
        protected override void PreLoad()
        {
            EnabledRows.Clear();
            RowsByStartEvent.Clear();
        }

        /// <summary>
        /// 로드된 Tutorial 행을 자동 시작 이벤트 타입별로 인덱싱합니다.
        /// </summary>
        /// <param name="row">로드가 완료된 Tutorial 테이블 행입니다.</param>
        protected override void OnLoadedData(StruckTableTutorial row)
        {
            if (row == null || row.Uid <= 0 || !row.Enabled)
            {
                return;
            }

            EnabledRows.Add(row);
            if (row.StartEventType == TutorialEventType.None)
            {
                return;
            }

            if (!RowsByStartEvent.TryGetValue(row.StartEventType, out List<StruckTableTutorial> rows))
            {
                rows = new List<StruckTableTutorial>();
                RowsByStartEvent.Add(row.StartEventType, rows);
            }

            rows.Add(row);
        }

        /// <summary>
        /// 테이블 행을 Tutorial 테이블 DTO로 변환합니다.
        /// </summary>
        /// <param name="data">헤더명과 값을 담은 테이블 행 사전입니다.</param>
        /// <returns>변환된 Tutorial 테이블 행입니다.</returns>
        protected override StruckTableTutorial BuildRow(Dictionary<string, string> data)
        {
            TableRowReader reader = ReadRow(data);
            int requiredCount = Math.Max(1, reader.Int("StartRequiredCount", 1));
            return new StruckTableTutorial
            {
                Uid = reader.Int("Uid"),
                Name = reader.String("Name"),
                Enabled = reader.BoolYN("Enabled", true),
                Repeatable = reader.BoolYN("Repeatable"),
                Priority = reader.Int("Priority"),
                StartEventType = reader.Enum("StartEventType", TutorialEventType.None),
                StartKey = reader.String("StartKey"),
                StartIntValue = reader.Int("StartIntValue"),
                StartRequiredCount = requiredCount,
                PreloadPolicy = reader.Enum("PreloadPolicy", TutorialPreloadPolicy.None),
                Memo = reader.String("Memo"),
            };
        }

        /// <summary>
        /// 활성화된 모든 Tutorial 테이블 행을 우선순위 기준으로 반환합니다.
        /// </summary>
        /// <returns>활성화된 Tutorial 테이블 행 목록입니다.</returns>
        public IReadOnlyList<StruckTableTutorial> GetEnabledRows()
        {
            SortRows(EnabledRows);
            return EnabledRows;
        }

        /// <summary>
        /// 지정한 이벤트 타입으로 자동 시작될 수 있는 Tutorial 테이블 행을 반환합니다.
        /// </summary>
        /// <param name="eventType">조회할 시작 이벤트 타입입니다.</param>
        /// <returns>이벤트 타입과 연결된 Tutorial 행 목록입니다.</returns>
        public IReadOnlyList<StruckTableTutorial> GetRowsByStartEvent(TutorialEventType eventType)
        {
            if (!RowsByStartEvent.TryGetValue(eventType, out List<StruckTableTutorial> rows))
            {
                return Array.Empty<StruckTableTutorial>();
            }

            SortRows(rows);
            return rows;
        }

        /// <summary>
        /// TableTutorial 행을 런타임 Catalog Entry로 변환합니다.
        /// </summary>
        /// <param name="row">변환할 Tutorial 테이블 행입니다.</param>
        /// <returns>런타임 Catalog Entry입니다.</returns>
        public static TutorialCatalogEntry ToCatalogEntry(StruckTableTutorial row)
        {
            if (row == null || row.Uid <= 0)
            {
                return null;
            }

            string addressableKey = TutorialAddressableKeyUtility.GetDefinitionAddressableKey(row.Uid);
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                return null;
            }

            return new TutorialCatalogEntry
            {
                uid = row.Uid,
                addressableKey = addressableKey,
                repeatable = row.Repeatable,
                startCondition = BuildStartCondition(row),
            };
        }

        /// <summary>
        /// 우선순위 오름차순, UID 오름차순으로 행 목록을 정렬합니다.
        /// </summary>
        /// <param name="rows">정렬할 행 목록입니다.</param>
        private static void SortRows(List<StruckTableTutorial> rows)
        {
            rows?.Sort(static (left, right) =>
            {
                int priorityCompare = left.Priority.CompareTo(right.Priority);
                return priorityCompare != 0 ? priorityCompare : left.Uid.CompareTo(right.Uid);
            });
        }

        /// <summary>
        /// 테이블 시작 조건 컬럼을 런타임 조건 정의로 변환합니다.
        /// </summary>
        /// <param name="row">시작 조건을 보유한 Tutorial 테이블 행입니다.</param>
        /// <returns>조건 정의입니다. 시작 이벤트가 없으면 null입니다.</returns>
        private static TutorialConditionDefinition BuildStartCondition(StruckTableTutorial row)
        {
            if (row == null || row.StartEventType == TutorialEventType.None)
            {
                return null;
            }

            return new TutorialConditionDefinition
            {
                type = row.StartEventType,
                key = string.IsNullOrWhiteSpace(row.StartKey) ? null : row.StartKey.Trim(),
                intValue = row.StartIntValue,
                requiredCount = Math.Max(1, row.StartRequiredCount),
            };
        }
    }
}
