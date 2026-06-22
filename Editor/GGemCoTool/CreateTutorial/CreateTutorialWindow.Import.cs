using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 Tutorial JSON Import 흐름을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 파일 선택 창에서 Tutorial JSON을 선택하고 Authoring Asset으로 가져옵니다.
        /// </summary>
        private void ImportTutorialJson()
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Tutorial JSON Import",
                Application.dataPath,
                "json");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            TutorialImportResult loadResult = TutorialJsonImporter.LoadDefinition(filePath);
            if (!loadResult.Succeeded || loadResult.Definition == null)
            {
                ApplyImportResult(loadResult);
                return;
            }

            TutorialDefinition definition = loadResult.Definition;
            StruckTableTutorial tableRow = FindTutorialTableRow(definition.uid);
            if (tableRow == null)
            {
                ApplyImportResult(TutorialImportResult.Failure(
                    $"Tutorial 테이블에서 JSON UID를 찾을 수 없습니다. uid: {definition.uid}"));
                return;
            }

            RebuildAuthoringAssetCache();
            TutorialAuthoringAsset existingAsset = FindAuthoringAsset(definition.uid);
            if (existingAsset != null &&
                !EditorUtility.DisplayDialog(
                    "Tutorial JSON Import",
                    $"UID {definition.uid}의 제작 데이터가 이미 존재합니다.\n" +
                    "제작 메모와 Step 표시 정보는 유지하고 JSON 런타임 데이터를 덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
            {
                return;
            }

            TutorialImportResult importResult = TutorialJsonImporter.Import(
                definition,
                tableRow,
                existingAsset);
            ApplyImportResult(importResult);
        }

        /// <summary>
        /// 지정한 UID에 대응하는 Tutorial 테이블 행을 반환합니다.
        /// </summary>
        /// <param name="tutorialUid">조회할 Tutorial UID입니다.</param>
        /// <returns>테이블 행입니다. 로드되지 않았거나 UID가 없으면 null입니다.</returns>
        private StruckTableTutorial FindTutorialTableRow(int tutorialUid)
        {
            if (_tableTutorial == null)
            {
                ReloadTutorialTable();
            }

            return _tableTutorial != null &&
                   tutorialUid > 0 &&
                   _tableTutorial.TryGetDataByUid(tutorialUid, out StruckTableTutorial row)
                ? row
                : null;
        }

        /// <summary>
        /// Import 결과를 현재 선택, 캐시와 상태 메시지에 반영합니다.
        /// </summary>
        /// <param name="result">반영할 Import 결과입니다.</param>
        private void ApplyImportResult(TutorialImportResult result)
        {
            if (result?.Asset == null)
            {
                _statusMessage = result?.Message ?? "Tutorial JSON Import 결과가 없습니다.";
                _statusType = MessageType.Error;
                Repaint();
                return;
            }

            RebuildAuthoringAssetCache();
            SetAsset(result.Asset);
            _statusMessage = result.Message;
            _statusType = MessageType.Info;
            Selection.activeObject = result.Asset;
            EditorGUIUtility.PingObject(result.Asset);
            Repaint();
        }
    }
}
