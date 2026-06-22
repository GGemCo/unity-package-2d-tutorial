using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 상단 툴바의 Tutorial JSON Import 흐름을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
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

            TutorialImportResult loadResult =
                TutorialJsonImporter.LoadDefinition(filePath);
            if (!loadResult.Succeeded || loadResult.Definition == null)
            {
                ApplyImportResult(loadResult);
                return;
            }

            TutorialDefinition definition = loadResult.Definition;
            StruckTableTutorial tableRow =
                FindTutorialTableRow(definition.uid) ??
                CreateDefaultTableRow(definition);

            RebuildAuthoringAssetCache();
            TutorialAuthoringAsset existing =
                FindAuthoringAsset(definition.uid);
            if (existing != null &&
                !EditorUtility.DisplayDialog(
                    "Tutorial JSON Import",
                    $"UID {definition.uid} 제작 데이터가 이미 존재합니다.\nJSON 내용으로 교체하시겠습니까?",
                    "교체",
                    "취소"))
            {
                return;
            }

            ApplyImportResult(
                TutorialJsonImporter.Import(definition, tableRow, existing));
        }

        private StruckTableTutorial FindTutorialTableRow(int tutorialUid)
        {
            if (_tableTutorial == null)
            {
                ReloadTutorialTable();
            }

            return _tableTutorial != null &&
                   tutorialUid > 0 &&
                   _tableTutorial.TryGetDataByUid(
                       tutorialUid,
                       out StruckTableTutorial row)
                ? row
                : null;
        }

        /// <summary>
        /// 테이블에 없는 JSON을 가져올 때 Export 가능한 기본 카탈로그 설정을 생성합니다.
        /// </summary>
        private static StruckTableTutorial CreateDefaultTableRow(
            TutorialDefinition definition)
        {
            return new StruckTableTutorial
            {
                Uid = definition.uid,
                Name = string.IsNullOrWhiteSpace(definition.title)
                    ? $"Tutorial {definition.uid}"
                    : definition.title,
                Enabled = true,
                Repeatable = false,
                Priority = 0,
                StartEventType = TutorialEventType.None,
                StartRequiredCount = 1,
                PreloadPolicy = TutorialPreloadPolicy.None,
            };
        }

        private void ApplyImportResult(TutorialImportResult result)
        {
            if (result?.Asset == null)
            {
                _statusMessage =
                    result?.Message ?? "Tutorial JSON Import 결과가 없습니다.";
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
