using System.Collections.Generic;
using System.IO;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 테이블과 JSON 에셋을 Addressables 그룹에 등록합니다.
    /// </summary>
    public class SettingTutorial : DefaultAddressable
    {
        private const string Title = "튜토리얼 추가하기";
        
        /// <summary>
        /// UI 레이아웃(버튼 폭/높이 등) 정보를 제공하는 부모 에디터 윈도우 참조입니다.
        /// </summary>
        private readonly AddressableEditorTutorial _addressableEditor;
        
        /// <summary>
        /// Tutorial Addressables 설정 모듈을 생성합니다.
        /// </summary>
        public SettingTutorial(AddressableEditorTutorial addressableEditorWindow)
        {
            _addressableEditor = addressableEditorWindow;
            targetGroupName = ConfigAddressableGroupNameTutorial.Tutorial;
        }

        /// <summary>
        /// Tutorial Addressables 설정 버튼을 그립니다.
        /// </summary>
        public void OnGUI()
        {
            if (!File.Exists(ConfigAddressableTableTutorial.TableTutorial.Path))
            {
                EditorGUILayout.HelpBox($"{ConfigAddressableTableTutorial.Tutorial} 테이블이 없습니다.", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button(Title, GUILayout.Width(_addressableEditor.ButtonWidth), GUILayout.Height(_addressableEditor.ButtonHeight)))
                {
                    try
                    {
                        Setup();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog(Title, "튜토리얼 Addressable 설정 중 오류가 발생했습니다.\n자세한 내용은 콘솔 로그를 확인해주세요.", "OK");
                    }
                }
            }
        }
        /// <summary>
        /// Tutorial 테이블과 JSON 에셋의 Addressables 설정을 갱신합니다.
        /// </summary>
        /// <param name="ctx">프로젝트 자동 설정 컨텍스트이며, 수동 실행 시 null입니다.</param>
        public void Setup(EditorSetupContext ctx = null)
        {
            if (ctx == null)
            {
                bool result = EditorUtility.DisplayDialog(TextDisplayDialogTitle, TextDisplayDialogMessage, "네", "아니요");
                if (!result) return;
            }
            
            Dictionary<int, StruckTableTutorial> dictionary =
                TableLoaderManagerTutorialEditor.LoadTutorialTable().GetDatas();
            
            // AddressableSettings 가져오기 (없으면 생성)
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                HelperLog.Warn("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.", ctx);
                settings = CreateAddressableSettings();
            }

            // GGemCo_Tables 그룹 가져오기 또는 생성
            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);
            if (!group)
            {
                HelperLog.Error($"'{targetGroupName}' 그룹을 설정할 수 없습니다.", ctx);
                return;
            }
            
            ClearGroupEntries(settings, group);

            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, StruckTableTutorial> outerPair in dictionary)
            {
                var info = outerPair.Value;
                if (info.Uid <= 0) continue;
            
                string key = $"{ConfigAddressableKeyTutorial.Tutorial}_{info.Uid}";
                string assetPath = $"{ConfigAddressablePathTutorial.Tutorial.RootTutorial}/tutorial_{info.Uid}.json";
                string label = ConfigAddressableLabelTutorial.Tutorial;
            
                Add(settings, group, key, assetPath, label);
            }

            TutorialExportResult guideSyncResult =
                TutorialGuideSpriteAddressableSynchronizer.Synchronize();
            if (!guideSyncResult.Succeeded)
            {
                HelperLog.Error(guideSyncResult.Message, ctx);
                return;
            }
            
            // 설정 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            if (ctx != null)
            {
                HelperLog.Info("[Addressable] 튜토리얼 설정 완료", ctx);
            }
            else
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog(Title, "[Addressable] 튜토리얼 설정 완료", "OK");
            }
        }
    }
}
