using System.Collections.Generic;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 TableTutorial Export 기능을 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// 현재 제작 데이터 또는 Unity Selection의 제작 데이터 목록을 Tutorial 테이블로 내보냅니다.
        /// </summary>
        private void ExportCurrentTableTutorial()
        {
            List<TutorialAuthoringAsset> assets = TutorialCatalogExporter.CollectSelectedAuthoringAssets();
            if (assets.Count <= 0 && _asset != null)
            {
                assets.Add(_asset);
            }

            if (assets.Count <= 0)
            {
                ApplyExportResult(TutorialExportResult.Failure("TableTutorial에 포함할 TutorialAuthoringAsset이 없습니다."));
                return;
            }

            ApplyModifiedProperties();
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i] == null)
                {
                    continue;
                }

                assets[i].EnsureDefaults();
                EditorUtility.SetDirty(assets[i]);
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "TableTutorial Export",
                TutorialTableExporter.GetDefaultTableFileName(),
                "txt",
                "선택된 TutorialAuthoringAsset 목록을 저장할 TableTutorial txt 경로를 선택하십시오.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TutorialExportResult result = TutorialTableExporter.ExportTable(assets, path);
            if (result.Succeeded)
            {
                _lastTableTutorialPath = result.AssetPath;
            }

            ApplyExportResult(result);
        }
    }
}
