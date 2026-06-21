using System;
using System.IO;
using System.Reflection;
using GGemCo2DCore;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 JSON 에셋을 Addressables에 등록하는 Editor 전용 보조 유틸리티입니다.
    /// Addressables Editor 어셈블리를 직접 참조하지 않고 Reflection으로 호출하여 패키지 참조 누락 시 컴파일 오류를 방지합니다.
    /// </summary>
    internal static class TutorialAddressableRegisterUtility
    {
        /// <summary>
        /// 튜토리얼 JSON을 배치할 기본 Addressables 그룹 이름입니다.
        /// </summary>
        public const string DefaultGroupName = "Tutorial";

        /// <summary>
        /// 개별 튜토리얼 JSON에 부여할 기본 라벨입니다.
        /// </summary>
        public const string DefaultDefinitionLabel = "tutorial-definition";

        /// <summary>
        /// 튜토리얼 Catalog JSON에 부여할 기본 라벨입니다.
        /// </summary>
        public const string DefaultCatalogLabel = "tutorial-catalog";

        /// <summary>
        /// Tutorial 테이블 txt에 부여할 기본 라벨입니다.
        /// </summary>
        public const string DefaultTableLabel = ConfigAddressableLabel.Table;

        private const string AddressableAssetSettingsDefaultObjectTypeName = "UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject";
        private const string BundledAssetGroupSchemaTypeName = "UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema";
        private const string ContentUpdateGroupSchemaTypeName = "UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema";

        /// <summary>
        /// 지정한 Unity 에셋을 Addressables에 등록하고 주소, 그룹, 라벨을 설정합니다.
        /// </summary>
        /// <param name="assetPath">등록할 Unity 프로젝트 상대 에셋 경로입니다.</param>
        /// <param name="address">Addressables 주소입니다.</param>
        /// <param name="groupName">Addressables 그룹 이름입니다.</param>
        /// <param name="label">부여할 Addressables 라벨입니다. 비어 있으면 라벨을 설정하지 않습니다.</param>
        /// <returns>등록 결과입니다.</returns>
        public static TutorialAddressableRegistrationResult RegisterAsset(
            string assetPath,
            string address,
            string groupName,
            string label)
        {
            if (!TryNormalizeAssetPath(assetPath, out string normalizedPath, out string pathError))
            {
                return TutorialAddressableRegistrationResult.Failure(pathError);
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                return TutorialAddressableRegistrationResult.Failure("Addressables 주소가 비어 있습니다.");
            }

            string resolvedGroupName = string.IsNullOrWhiteSpace(groupName) ? DefaultGroupName : groupName.Trim();
            string resolvedAddress = address.Trim();
            string resolvedLabel = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(normalizedPath);
            if (asset == null)
            {
                return TutorialAddressableRegistrationResult.Failure($"등록할 에셋을 찾을 수 없습니다. path: {normalizedPath}");
            }

            if (!TryGetAddressableSettings(out object settings, out string settingsError))
            {
                return TutorialAddressableRegistrationResult.Failure(settingsError);
            }

            try
            {
                object group = FindGroup(settings, resolvedGroupName) ?? CreateGroup(settings, resolvedGroupName);
                if (group == null)
                {
                    return TutorialAddressableRegistrationResult.Failure($"Addressables 그룹을 찾거나 생성하지 못했습니다. group: {resolvedGroupName}");
                }

                string guid = AssetDatabase.AssetPathToGUID(normalizedPath);
                if (string.IsNullOrEmpty(guid))
                {
                    return TutorialAddressableRegistrationResult.Failure($"에셋 GUID를 찾을 수 없습니다. path: {normalizedPath}");
                }

                object entry = CreateOrMoveEntry(settings, guid, group);
                if (entry == null)
                {
                    return TutorialAddressableRegistrationResult.Failure($"Addressables Entry 생성에 실패했습니다. path: {normalizedPath}");
                }

                SetEntryAddress(entry, resolvedAddress);
                if (!string.IsNullOrWhiteSpace(resolvedLabel))
                {
                    AddLabel(settings, resolvedLabel);
                    SetEntryLabel(entry, resolvedLabel);
                }

                MarkSettingsDirty(settings, entry);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return TutorialAddressableRegistrationResult.Success(
                    $"Addressables에 등록했습니다. address: {resolvedAddress}, group: {resolvedGroupName}",
                    normalizedPath,
                    resolvedAddress,
                    asset);
            }
            catch (Exception exception)
            {
                return TutorialAddressableRegistrationResult.Failure($"Addressables 등록 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// Unity Selection에서 TextAsset을 찾아 프로젝트 상대 경로를 반환합니다.
        /// </summary>
        /// <param name="assetPath">선택된 TextAsset의 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>TextAsset 경로를 찾으면 true입니다.</returns>
        public static bool TryGetSelectedTextAssetPath(out string assetPath, out string error)
        {
            assetPath = null;
            error = null;

            if (Selection.activeObject == null)
            {
                error = "프로젝트 창에서 등록할 JSON TextAsset을 선택하십시오.";
                return false;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!TryNormalizeAssetPath(path, out assetPath, out error))
            {
                return false;
            }

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (textAsset == null)
            {
                error = "선택된 에셋이 TextAsset이 아닙니다.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unity 프로젝트 상대 경로를 검증합니다.
        /// </summary>
        /// <param name="assetPath">검증할 경로입니다.</param>
        /// <param name="normalizedPath">보정된 경로입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>유효하면 true입니다.</returns>
        private static bool TryNormalizeAssetPath(
            string assetPath,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = null;
            error = null;

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                error = "에셋 경로가 비어 있습니다.";
                return false;
            }

            string path = assetPath.Trim().Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) && path != "Assets")
            {
                error = "Unity 프로젝트의 Assets 폴더 하위 에셋만 등록할 수 있습니다.";
                return false;
            }

            if (path == "Assets" || string.IsNullOrWhiteSpace(Path.GetFileName(path)))
            {
                error = "등록할 파일 경로가 아닙니다.";
                return false;
            }

            normalizedPath = path;
            return true;
        }

        /// <summary>
        /// 현재 프로젝트의 Addressables Settings 객체를 Reflection으로 가져옵니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="error">실패 시 오류 메시지입니다.</param>
        /// <returns>Settings를 찾으면 true입니다.</returns>
        private static bool TryGetAddressableSettings(out object settings, out string error)
        {
            settings = null;
            error = null;

            Type defaultObjectType = FindType(AddressableAssetSettingsDefaultObjectTypeName);
            if (defaultObjectType == null)
            {
                error = "Addressables Editor 어셈블리를 찾을 수 없습니다. com.unity.addressables 패키지 설치 또는 Editor asmdef 참조를 확인하십시오.";
                return false;
            }

            PropertyInfo settingsProperty = defaultObjectType.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
            settings = settingsProperty?.GetValue(null);
            if (settings == null)
            {
                error = "Addressables Settings가 없습니다. Window > Asset Management > Addressables > Groups에서 Settings를 먼저 생성하십시오.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 현재 AppDomain에 로드된 어셈블리에서 타입을 찾습니다.
        /// </summary>
        /// <param name="fullName">찾을 전체 타입 이름입니다.</param>
        /// <returns>찾은 타입입니다. 없으면 null입니다.</returns>
        private static Type FindType(string fullName)
        {
            Type type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Addressables Settings에서 그룹을 찾습니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="groupName">찾을 그룹 이름입니다.</param>
        /// <returns>찾은 그룹 객체입니다.</returns>
        private static object FindGroup(object settings, string groupName)
        {
            MethodInfo method = settings.GetType().GetMethod("FindGroup", new[] { typeof(string) });
            return method?.Invoke(settings, new object[] { groupName });
        }

        /// <summary>
        /// Addressables Settings에 그룹을 생성합니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="groupName">생성할 그룹 이름입니다.</param>
        /// <returns>생성된 그룹 객체입니다.</returns>
        private static object CreateGroup(object settings, string groupName)
        {
            Type bundledSchemaType = FindType(BundledAssetGroupSchemaTypeName);
            Type contentUpdateSchemaType = FindType(ContentUpdateGroupSchemaTypeName);
            Type[] schemaTypes = contentUpdateSchemaType != null
                ? new[] { bundledSchemaType, contentUpdateSchemaType }
                : new[] { bundledSchemaType };
            if (bundledSchemaType == null)
            {
                schemaTypes = Type.EmptyTypes;
            }

            MethodInfo method = FindMethod(settings.GetType(), "CreateGroup", 6);
            return method?.Invoke(settings, new object[]
            {
                groupName,
                false,
                false,
                true,
                null,
                schemaTypes,
            });
        }

        /// <summary>
        /// 지정 이름과 매개변수 개수를 가진 메서드를 찾습니다.
        /// </summary>
        /// <param name="type">메서드를 찾을 타입입니다.</param>
        /// <param name="methodName">메서드 이름입니다.</param>
        /// <param name="parameterCount">매개변수 개수입니다.</param>
        /// <returns>찾은 메서드입니다.</returns>
        private static MethodInfo FindMethod(Type type, string methodName, int parameterCount)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name == methodName && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Addressables Entry를 생성하거나 지정 그룹으로 이동합니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="guid">등록할 에셋 GUID입니다.</param>
        /// <param name="group">대상 그룹 객체입니다.</param>
        /// <returns>생성 또는 이동된 Entry 객체입니다.</returns>
        private static object CreateOrMoveEntry(object settings, string guid, object group)
        {
            MethodInfo[] methods = settings.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name != "CreateOrMoveEntry" || parameters.Length < 2)
                {
                    continue;
                }

                object[] args = new object[parameters.Length];
                args[0] = guid;
                args[1] = group;
                for (int j = 2; j < args.Length; j++)
                {
                    args[j] = parameters[j].ParameterType == typeof(bool) ? (object)false : null;
                }

                return method.Invoke(settings, args);
            }

            return null;
        }

        /// <summary>
        /// Addressables Entry 주소를 설정합니다.
        /// </summary>
        /// <param name="entry">주소를 설정할 Entry 객체입니다.</param>
        /// <param name="address">설정할 Addressables 주소입니다.</param>
        private static void SetEntryAddress(object entry, string address)
        {
            MethodInfo method = FindMethod(entry.GetType(), "SetAddress", 2);
            if (method != null)
            {
                method.Invoke(entry, new object[] { address, false });
                return;
            }

            method = FindMethod(entry.GetType(), "SetAddress", 1);
            if (method != null)
            {
                method.Invoke(entry, new object[] { address });
                return;
            }

            PropertyInfo property = entry.GetType().GetProperty("address", BindingFlags.Public | BindingFlags.Instance)
                ?? entry.GetType().GetProperty("Address", BindingFlags.Public | BindingFlags.Instance);
            property?.SetValue(entry, address);
        }

        /// <summary>
        /// Addressables Settings에 라벨을 추가합니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="label">추가할 라벨입니다.</param>
        private static void AddLabel(object settings, string label)
        {
            MethodInfo[] methods = settings.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name != "AddLabel" || parameters.Length <= 0 || parameters[0].ParameterType != typeof(string))
                {
                    continue;
                }

                object[] args = new object[parameters.Length];
                args[0] = label;
                for (int j = 1; j < args.Length; j++)
                {
                    args[j] = parameters[j].ParameterType == typeof(bool) ? (object)false : null;
                }

                method.Invoke(settings, args);
                return;
            }
        }

        /// <summary>
        /// Addressables Entry에 라벨을 설정합니다.
        /// </summary>
        /// <param name="entry">라벨을 설정할 Entry 객체입니다.</param>
        /// <param name="label">설정할 라벨입니다.</param>
        private static void SetEntryLabel(object entry, string label)
        {
            MethodInfo[] methods = entry.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name != "SetLabel" || parameters.Length < 2 || parameters[0].ParameterType != typeof(string))
                {
                    continue;
                }

                object[] args = new object[parameters.Length];
                args[0] = label;
                args[1] = true;
                for (int j = 2; j < args.Length; j++)
                {
                    args[j] = parameters[j].ParameterType == typeof(bool) ? (object)false : null;
                }

                method.Invoke(entry, args);
                return;
            }
        }

        /// <summary>
        /// Addressables Settings 변경 사항을 Dirty 처리합니다.
        /// </summary>
        /// <param name="settings">Addressables Settings 객체입니다.</param>
        /// <param name="entry">변경된 Entry 객체입니다.</param>
        private static void MarkSettingsDirty(object settings, object entry)
        {
            MethodInfo[] methods = settings.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "SetDirty")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                object[] args = new object[parameters.Length];
                for (int j = 0; j < parameters.Length; j++)
                {
                    Type parameterType = parameters[j].ParameterType;
                    if (parameterType.IsEnum)
                    {
                        args[j] = ResolveEnumValue(parameterType, "EntryModified")
                            ?? ResolveEnumValue(parameterType, "GroupSchemaModified")
                            ?? Enum.GetValues(parameterType).GetValue(0);
                    }
                    else if (parameterType == typeof(object))
                    {
                        args[j] = entry;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        args[j] = true;
                    }
                    else
                    {
                        args[j] = null;
                    }
                }

                method.Invoke(settings, args);
                return;
            }

            if (settings is UnityEngine.Object settingsObject)
            {
                EditorUtility.SetDirty(settingsObject);
            }
        }
        /// <summary>
        /// 지정한 이름의 enum 값을 찾습니다.
        /// </summary>
        /// <param name="enumType">검색할 enum 타입입니다.</param>
        /// <param name="name">찾을 enum 이름입니다.</param>
        /// <returns>찾은 enum 값입니다. 없으면 null입니다.</returns>
        private static object ResolveEnumValue(Type enumType, string name)
        {
            if (enumType == null || string.IsNullOrWhiteSpace(name) || !enumType.IsEnum)
            {
                return null;
            }

            string[] names = Enum.GetNames(enumType);
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], name, StringComparison.Ordinal))
                {
                    return Enum.Parse(enumType, names[i]);
                }
            }

            return null;
        }

    }
}
