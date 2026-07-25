using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial UID에 연결되는 복합 자동 시작 조건 테이블 행입니다.
    /// </summary>
    public sealed class StruckTableTutorialStartCondition : IUidName
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public bool Enabled;
        public int TutorialUid;
        public int Order;
        public TutorialStartConditionSource Source;
        public TutorialEventType EventType;
        public TutorialStartStateType StateType;
        public int TargetUid;
        public TutorialInputActionType InputAction;
        public int IntValue;
        public float FloatValue;
        public int RequiredCount;
        public string Memo;
    }

    /// <summary>
    /// Tutorial별 복합 자동 시작 조건을 로드하고 Tutorial UID 기준으로 캐시합니다.
    /// </summary>
    public sealed class TableTutorialStartCondition :
        DefaultTable<StruckTableTutorialStartCondition>
    {
        private readonly Dictionary<int, List<StruckTableTutorialStartCondition>>
            _rowsByTutorialUid =
                new Dictionary<int, List<StruckTableTutorialStartCondition>>();
        private readonly Dictionary<
            TutorialEventType,
            List<StruckTableTutorialStartCondition>> _rowsByEventType =
                new Dictionary<
                    TutorialEventType,
                    List<StruckTableTutorialStartCondition>>();

        public override string Key =>
            ConfigAddressableTableTutorial.TutorialStartCondition;

        /// <summary>
        /// 테이블을 다시 로드하기 전에 Tutorial별 조건 캐시를 초기화합니다.
        /// </summary>
        protected override void PreLoad()
        {
            _rowsByTutorialUid.Clear();
            _rowsByEventType.Clear();
        }

        /// <summary>
        /// 활성화된 유효 조건을 Tutorial UID별 캐시에 등록합니다.
        /// </summary>
        /// <param name="row">파싱이 완료된 자동 시작 조건 행입니다.</param>
        protected override void OnLoadedData(
            StruckTableTutorialStartCondition row)
        {
            if (!IsValid(row))
            {
                return;
            }

            if (!_rowsByTutorialUid.TryGetValue(
                    row.TutorialUid,
                    out List<StruckTableTutorialStartCondition> rows))
            {
                rows = new List<StruckTableTutorialStartCondition>();
                _rowsByTutorialUid.Add(row.TutorialUid, rows);
            }

            rows.Add(row);
            if (row.Source != TutorialStartConditionSource.Event)
            {
                return;
            }

            if (!_rowsByEventType.TryGetValue(
                    row.EventType,
                    out List<StruckTableTutorialStartCondition> eventRows))
            {
                eventRows = new List<StruckTableTutorialStartCondition>();
                _rowsByEventType.Add(row.EventType, eventRows);
            }

            eventRows.Add(row);
        }

        /// <summary>
        /// 문자열 테이블 행을 자동 시작 조건 데이터로 변환합니다.
        /// </summary>
        /// <param name="data">컬럼 이름과 원문 값 사전입니다.</param>
        /// <returns>파싱된 자동 시작 조건 행입니다.</returns>
        protected override StruckTableTutorialStartCondition BuildRow(
            Dictionary<string, string> data)
        {
            TableRowReader reader = ReadRow(data);
            return new StruckTableTutorialStartCondition
            {
                Uid = reader.Int("Uid"),
                Name = reader.String("Name"),
                Enabled = reader.BoolYN("Enabled", true),
                TutorialUid = reader.Int("TutorialUid"),
                Order = reader.Int("Order"),
                Source = reader.Enum(
                    "Source",
                    TutorialStartConditionSource.Event),
                EventType = reader.Enum(
                    "EventType",
                    TutorialEventType.None),
                StateType = reader.Enum(
                    "StateType",
                    TutorialStartStateType.None),
                TargetUid = reader.Int("TargetUid"),
                InputAction = reader.Enum(
                    "InputAction",
                    TutorialInputActionType.None),
                IntValue = reader.Int("IntValue"),
                FloatValue = reader.Float("FloatValue"),
                RequiredCount = Math.Max(
                    1,
                    reader.Int("RequiredCount", 1)),
                Memo = reader.String("Memo"),
            };
        }

        /// <summary>
        /// 지정한 Tutorial UID의 활성 자동 시작 조건을 제작 순서대로 반환합니다.
        /// </summary>
        /// <param name="tutorialUid">조회할 Tutorial UID입니다.</param>
        /// <returns>정렬된 조건 목록이며 등록된 조건이 없으면 빈 목록입니다.</returns>
        public IReadOnlyList<StruckTableTutorialStartCondition>
            GetRowsByTutorialUid(int tutorialUid)
        {
            if (tutorialUid <= 0 ||
                !_rowsByTutorialUid.TryGetValue(
                    tutorialUid,
                    out List<StruckTableTutorialStartCondition> rows))
            {
                return Array.Empty<StruckTableTutorialStartCondition>();
            }

            rows.Sort(static (left, right) =>
            {
                int orderCompare = left.Order.CompareTo(right.Order);
                return orderCompare != 0
                    ? orderCompare
                    : left.Uid.CompareTo(right.Uid);
            });
            return rows;
        }

        /// <summary>
        /// 지정한 이벤트 타입을 사용하는 모든 활성 자동 시작 조건을 반환합니다.
        /// 프로젝트 전용 이벤트 Publisher가 필요한 조건만 선별할 때 사용합니다.
        /// </summary>
        /// <param name="eventType">조회할 Tutorial 이벤트 타입입니다.</param>
        /// <returns>이벤트 조건 목록이며 등록된 조건이 없으면 빈 목록입니다.</returns>
        public IReadOnlyList<StruckTableTutorialStartCondition>
            GetRowsByEventType(TutorialEventType eventType)
        {
            return _rowsByEventType.TryGetValue(
                eventType,
                out List<StruckTableTutorialStartCondition> rows)
                ? rows
                : Array.Empty<StruckTableTutorialStartCondition>();
        }

        /// <summary>
        /// 조건 행이 자동 시작 판정에 사용할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="row">검증할 조건 행입니다.</param>
        /// <returns>활성화되었고 Source별 타입이 유효하면 <see langword="true"/>입니다.</returns>
        private static bool IsValid(StruckTableTutorialStartCondition row)
        {
            if (row == null ||
                !row.Enabled ||
                row.Uid <= 0 ||
                row.TutorialUid <= 0)
            {
                return false;
            }

            return row.Source == TutorialStartConditionSource.Event
                ? row.EventType != TutorialEventType.None
                : row.StateType != TutorialStartStateType.None;
        }
    }
}
