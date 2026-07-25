using System.Collections.Generic;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 Tutorial 테이블 선택과 제작 데이터 생성 기능을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        private readonly List<SearchableDropdownUtility.Option<StruckTableTutorial>> _tutorialTableOptions =
            new List<SearchableDropdownUtility.Option<StruckTableTutorial>>();
        private readonly Dictionary<int, TutorialAuthoringAsset> _authoringAssetsByUid =
            new Dictionary<int, TutorialAuthoringAsset>();

        private TableTutorial _tableTutorial;
        private int _selectedTutorialUid;

        /// <summary>
        /// Tutorial 데이터 테이블을 다시 로드하고 UID 선택 목록과 제작 데이터 캐시를 갱신합니다.
        /// </summary>
        private void ReloadTutorialTable()
        {
            _tableTutorial = TableLoaderManagerBase.LoadTable<TableTutorial>(
                ConfigAddressableTableTutorial.TableTutorial.Path,
                forceReload: true);

            RebuildTutorialTableOptions();
            RebuildAuthoringAssetCache();

            if (_selectedTutorialUid > 0)
            {
                SelectTutorialUid(_selectedTutorialUid, updateStatus: false);
            }
        }

        /// <summary>
        /// Tutorial 테이블 UID 선택, 에셋 조회 결과, 생성 버튼을 포함한 패널을 그립니다.
        /// </summary>
        private void DrawTutorialTableSelectionPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Tutorial 테이블", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("새로고침", EditorStyles.miniButton, GUILayout.Width(64f)))
                    {
                        ReloadTutorialTable();
                    }
                }

                if (_tutorialTableOptions.Count <= 0)
                {
                    EditorGUILayout.HelpBox(
                        $"Tutorial 테이블 데이터가 없습니다.\n경로: {ConfigAddressableTableTutorial.TableTutorial.Path}",
                        MessageType.Warning);
                    return;
                }

                SearchableDropdownUtility.DrawButtonAndShow(
                    buttonText: GetSelectedTutorialText(),
                    options: _tutorialTableOptions,
                    selectedIndex: GetSelectedTutorialOptionIndex(),
                    onSelected: OnTutorialTableSelected,
                    defaultSearchMode: SearchableDropdownUtility.SearchMode.Both,
                    selectedKey: _selectedTutorialUid > 0 ? _selectedTutorialUid.ToString() : null);

                StruckTableTutorial selectedRow = GetSelectedTutorialRow();
                TutorialAuthoringAsset existingAsset = FindAuthoringAsset(_selectedTutorialUid);
                if (selectedRow != null)
                {
                    // string assetState = existingAsset != null
                    //     ? $"제작 데이터 있음: {AssetDatabase.GetAssetPath(existingAsset)}"
                    //     : "제작 데이터 없음";
                    // EditorGUILayout.HelpBox(assetState, existingAsset != null ? MessageType.Info : MessageType.Warning);
                }

                using (new EditorGUI.DisabledScope(selectedRow == null || existingAsset != null))
                {
                    if (GUILayout.Button("선택 UID 생성하기"))
                    {
                        CreateAuthoringAssetFromTableRow(selectedRow);
                    }
                }
            }
        }

        /// <summary>
        /// 로드된 Tutorial 테이블 행을 UID 오름차순의 검색 가능한 선택 항목으로 변환합니다.
        /// </summary>
        private void RebuildTutorialTableOptions()
        {
            _tutorialTableOptions.Clear();
            if (_tableTutorial == null)
            {
                return;
            }

            List<StruckTableTutorial> rows = new List<StruckTableTutorial>(_tableTutorial.GetDatas().Values);
            rows.Sort(static (left, right) => left.Uid.CompareTo(right.Uid));

            for (int i = 0; i < rows.Count; i++)
            {
                StruckTableTutorial row = rows[i];
                if (row == null || row.Uid <= 0)
                {
                    continue;
                }

                _tutorialTableOptions.Add(new SearchableDropdownUtility.Option<StruckTableTutorial>(
                    row.Uid.ToString(),
                    string.IsNullOrWhiteSpace(row.Name) ? $"Tutorial {row.Uid}" : row.Name,
                    row));
            }
        }

        /// <summary>
        /// 프로젝트에 존재하는 TutorialAuthoringAsset을 UID 기준으로 캐시합니다.
        /// 동일 UID가 여러 개면 처음 검색된 에셋을 사용합니다.
        /// </summary>
        private void RebuildAuthoringAssetCache()
        {
            _authoringAssetsByUid.Clear();

            string[] guids = AssetDatabase.FindAssets("t:TutorialAuthoringAsset");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                TutorialAuthoringAsset authoringAsset =
                    AssetDatabase.LoadAssetAtPath<TutorialAuthoringAsset>(assetPath);
                if (authoringAsset == null || authoringAsset.Uid <= 0 ||
                    _authoringAssetsByUid.ContainsKey(authoringAsset.Uid))
                {
                    continue;
                }

                _authoringAssetsByUid.Add(authoringAsset.Uid, authoringAsset);
            }
        }

        /// <summary>
        /// Tutorial 테이블 드롭다운에서 선택한 행을 현재 UID로 반영합니다.
        /// </summary>
        /// <param name="index">선택된 드롭다운 옵션 인덱스입니다.</param>
        /// <param name="option">선택된 Tutorial 테이블 옵션입니다.</param>
        private void OnTutorialTableSelected(
            int index,
            SearchableDropdownUtility.Option<StruckTableTutorial> option)
        {
            if (option.Data == null)
            {
                return;
            }

            SelectTutorialUid(option.Data.Uid, updateStatus: true);
            Repaint();
        }

        /// <summary>
        /// 지정한 Tutorial UID의 제작 데이터를 찾아 편집 대상으로 연결합니다.
        /// 제작 데이터가 없으면 현재 편집 대상을 비우고 생성 가능한 상태로 전환합니다.
        /// </summary>
        /// <param name="uid">선택할 Tutorial UID입니다.</param>
        /// <param name="updateStatus">선택 결과를 상태 메시지에 표시할지 여부입니다.</param>
        private void SelectTutorialUid(int uid, bool updateStatus)
        {
            _selectedTutorialUid = uid;
            TutorialAuthoringAsset existingAsset = FindAuthoringAsset(uid);
            if (existingAsset != null)
            {
                SetAsset(existingAsset);
                if (updateStatus)
                {
                    _statusMessage = $"UID {uid} 제작 데이터를 찾았습니다: {AssetDatabase.GetAssetPath(existingAsset)}";
                    _statusType = MessageType.Info;
                }

                return;
            }

            SetAsset(null);
            if (updateStatus)
            {
                _statusMessage = $"UID {uid} 제작 데이터가 없습니다. 생성하기 버튼을 사용할 수 있습니다.";
                _statusType = MessageType.Warning;
            }
        }

        /// <summary>
        /// 선택된 UID에 대응하는 Tutorial 테이블 행을 반환합니다.
        /// </summary>
        /// <returns>선택된 테이블 행입니다. 선택이 없거나 행이 없으면 null입니다.</returns>
        private StruckTableTutorial GetSelectedTutorialRow()
        {
            if (_tableTutorial == null || _selectedTutorialUid <= 0)
            {
                return null;
            }

            return _tableTutorial.TryGetDataByUid(
                _selectedTutorialUid,
                out StruckTableTutorial row)
                ? row
                : null;
        }

        /// <summary>
        /// 지정한 UID의 TutorialAuthoringAsset을 캐시에서 찾습니다.
        /// </summary>
        /// <param name="uid">조회할 Tutorial UID입니다.</param>
        /// <returns>찾은 제작 데이터입니다. 없으면 null입니다.</returns>
        private TutorialAuthoringAsset FindAuthoringAsset(int uid)
        {
            return uid > 0 && _authoringAssetsByUid.TryGetValue(uid, out TutorialAuthoringAsset authoringAsset)
                ? authoringAsset
                : null;
        }

        /// <summary>
        /// 현재 선택된 Tutorial UID와 이름을 드롭다운 표시 문자열로 반환합니다.
        /// </summary>
        /// <returns>드롭다운 버튼에 표시할 문자열입니다.</returns>
        private string GetSelectedTutorialText()
        {
            StruckTableTutorial row = GetSelectedTutorialRow();
            if (row == null)
            {
                return "Tutorial UID 선택...";
            }

            return string.IsNullOrWhiteSpace(row.Name)
                ? row.Uid.ToString()
                : $"{row.Uid} - {row.Name}";
        }

        /// <summary>
        /// 현재 선택된 Tutorial UID의 드롭다운 옵션 인덱스를 반환합니다.
        /// </summary>
        /// <returns>선택된 옵션 인덱스입니다. 없으면 -1입니다.</returns>
        private int GetSelectedTutorialOptionIndex()
        {
            for (int i = 0; i < _tutorialTableOptions.Count; i++)
            {
                StruckTableTutorial row = _tutorialTableOptions[i].Data;
                if (row != null && row.Uid == _selectedTutorialUid)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 선택한 Tutorial 테이블 행을 기본값으로 사용하는 TutorialAuthoringAsset을 생성합니다.
        /// </summary>
        /// <param name="row">제작 데이터의 기본값으로 사용할 Tutorial 테이블 행입니다.</param>
        private void CreateAuthoringAssetFromTableRow(StruckTableTutorial row)
        {
            if (row == null || row.Uid <= 0 || FindAuthoringAsset(row.Uid) != null)
            {
                return;
            }

            string path = TutorialAuthoringNamingUtility.BuildUniqueAuthoringAssetPath(row.Uid);
            TutorialAuthoringAsset newAsset = CreateInstance<TutorialAuthoringAsset>();
            newAsset.Uid = row.Uid;
            newAsset.Title = string.IsNullOrWhiteSpace(row.Name) ? $"Tutorial {row.Uid}" : row.Name;
            newAsset.Memo = row.Memo;
            newAsset.Enabled = row.Enabled;
            newAsset.Repeatable = row.Repeatable;
            newAsset.Priority = row.Priority;
            newAsset.PreloadPolicy = row.PreloadPolicy;
            newAsset.EnsureDefaults();

            // 테이블의 자동 시작 조건을 제작 데이터에 복사하여 런타임 정책과 초기 상태를 일치시킵니다.
            TutorialAuthoringCondition startCondition = newAsset.StartCondition;
            startCondition.Type = row.StartEventType;
            startCondition.TargetUid = row.StartTargetUid;
            startCondition.InputAction = row.StartInputAction;
            startCondition.IntValue = row.StartIntValue;
            startCondition.FloatValue = row.StartFloatValue;
            startCondition.RequiredCount = row.StartRequiredCount;
            PopulateStartConditionsFromTable(newAsset, row);

            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _authoringAssetsByUid[row.Uid] = newAsset;
            SetAsset(newAsset);
            Selection.activeObject = newAsset;
            EditorGUIUtility.PingObject(newAsset);
            _statusMessage = $"UID {row.Uid} 제작 데이터를 생성했습니다: {path}";
            _statusType = MessageType.Info;
        }

        /// <summary>
        /// 신규 하위 조건 테이블을 우선 사용하고 없으면 기존 단일 시작 조건을 제작 목록으로 변환합니다.
        /// </summary>
        /// <param name="asset">조건 목록을 채울 제작 에셋입니다.</param>
        /// <param name="row">기존 Tutorial Catalog 행입니다.</param>
        private static void PopulateStartConditionsFromTable(
            TutorialAuthoringAsset asset,
            StruckTableTutorial row)
        {
            asset.StartMatchMode = row.StartMatchMode;
            asset.StartConditions.Clear();
            TableTutorialStartCondition table =
                TableLoaderManagerTutorialEditor
                    .LoadTutorialStartConditionTable();
            IReadOnlyList<StruckTableTutorialStartCondition> rows =
                table?.GetRowsByTutorialUid(row.Uid);
            if (rows != null && rows.Count > 0)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    StruckTableTutorialStartCondition source = rows[i];
                    TutorialAuthoringCondition condition =
                        TutorialAuthoringCondition.CreateDefault(
                            source.EventType);
                    condition.StartSource = source.Source;
                    condition.StartStateType = source.StateType;
                    condition.TargetUid = source.TargetUid;
                    condition.InputAction = source.InputAction;
                    condition.IntValue = source.IntValue;
                    condition.FloatValue = source.FloatValue;
                    condition.RequiredCount = source.RequiredCount;
                    condition.Memo = source.Memo;
                    asset.StartConditions.Add(condition);
                }

                return;
            }

            if (row.StartEventType == TutorialEventType.None)
            {
                return;
            }

            TutorialAuthoringCondition legacy =
                TutorialAuthoringCondition.CreateDefault(row.StartEventType);
            legacy.StartSource = TutorialStartConditionSource.Event;
            legacy.TargetUid = row.StartTargetUid;
            legacy.InputAction = row.StartInputAction;
            legacy.IntValue = row.StartIntValue;
            legacy.FloatValue = row.StartFloatValue;
            legacy.RequiredCount = row.StartRequiredCount;
            asset.StartConditions.Add(legacy);
        }
    }
}
