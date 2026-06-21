using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지의 런타임 설정을 보관하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTutorial.Tutorial.FileName,
        menuName = ConfigScriptableObjectTutorial.Tutorial.MenuName,
        order = ConfigScriptableObjectTutorial.Tutorial.Ordering)]
    public sealed class GGemCoTutorialSettings : ScriptableObject
    {
        [Header("Addressables")]
        [SerializeField]
        [Tooltip("Tutorial Catalog TextAsset을 로드할 때 사용하는 Addressables 키입니다.")]
        private string catalogAddressableKey = TutorialConstants.DefaultCatalogKey;

        /// <summary>
        /// Tutorial Catalog TextAsset의 Addressables 키를 반환합니다.
        /// </summary>
        public string CatalogAddressableKey =>
            string.IsNullOrWhiteSpace(catalogAddressableKey)
                ? TutorialConstants.DefaultCatalogKey
                : catalogAddressableKey;
    }
}
