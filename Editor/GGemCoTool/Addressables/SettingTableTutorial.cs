using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 테이블과 런타임 테이블 팩을 Table Addressables 그룹에 등록합니다.
    /// </summary>
    public sealed class SettingTableTutorial : DefaultAddressable
    {
        private const string Title = "Tutorial 테이블 추가하기";
        private readonly AddressableEditorTutorial _addressableEditor;

        /// <summary>
        /// Tutorial 테이블 Addressables 설정 모듈을 생성합니다.
        /// </summary>
        /// <param name="addressableEditorWindow">버튼 레이아웃을 제공하는 부모 창입니다.</param>
        public SettingTableTutorial(AddressableEditorTutorial addressableEditorWindow)
        {
            _addressableEditor = addressableEditorWindow;
            targetGroupName = ConfigAddressableGroupName.Table;
        }

        /// <summary>
        /// Tutorial 테이블 등록 버튼을 그립니다.
        /// </summary>
        public void OnGUI()
        {
            if (!GUILayout.Button(
                    Title,
                    GUILayout.Width(_addressableEditor.ButtonWidth),
                    GUILayout.Height(_addressableEditor.ButtonHeight)))
            {
                return;
            }

            try
            {
                Setup();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    Title,
                    "Tutorial 테이블 Addressables 설정 중 오류가 발생했습니다.\n자세한 내용은 콘솔 로그를 확인해주세요.",
                    "OK");
            }
        }

        /// <summary>
        /// Tutorial 개별 테이블과 런타임 테이블 팩을 Addressables에 등록합니다.
        /// </summary>
        /// <param name="ctx">
        /// 자동 설정 실행 컨텍스트입니다. null이면 완료 후 에셋을 저장하고 다이얼로그를 표시합니다.
        /// </param>
        public void Setup(EditorSetupContext ctx = null)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                HelperLog.Warn("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.", ctx);
                settings = CreateAddressableSettings();
            }

            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);
            if (!group)
            {
                HelperLog.Error($"'{targetGroupName}' 그룹을 설정할 수 없습니다.", ctx);
                return;
            }

            RegisterRuntimeTablePack(settings, group, ctx);

            // 개별 테이블은 에디터 편집과 런타임 팩 생성의 원본으로 계속 등록합니다.
            foreach (AddressableAssetInfo addressableAssetInfo in ConfigAddressableTableTutorial.All)
            {
                Add(
                    settings,
                    group,
                    addressableAssetInfo.Key,
                    addressableAssetInfo.Path,
                    ConfigAddressableLabel.Table);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);

            if (ctx != null)
            {
                HelperLog.Info("Tutorial 테이블 Addressables 설정 완료", ctx);
                return;
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(Title, "Addressables 설정 완료", "OK");
        }

        /// <summary>
        /// Tutorial 개별 테이블을 런타임 팩으로 생성하고 Addressables에 등록합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정 객체입니다.</param>
        /// <param name="group">등록 대상 Table 그룹입니다.</param>
        /// <param name="ctx">자동 설정 실행 컨텍스트입니다.</param>
        private void RegisterRuntimeTablePack(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            EditorSetupContext ctx)
        {
            AddressableAssetInfo pack = ConfigAddressableTableTutorial.TablePackTutorial;
            bool built = RuntimeTablePackBuilder.Build(
                ConfigAddressableTableTutorial.PackageId,
                pack,
                ConfigAddressableTableTutorial.All,
                ctx);

            if (!built)
            {
                HelperLog.Warn(
                    "Tutorial 런타임 테이블 팩 생성에 실패했습니다. 개별 테이블 등록은 계속 진행합니다.",
                    ctx);
                return;
            }

            Add(settings, group, pack.Key, pack.Path, ConfigAddressableLabel.TablePack);
        }
    }
}
