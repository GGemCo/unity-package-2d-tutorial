using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTutorialEditor
{
    public class DefaultEditorWindowTutorial : DefaultEditorWindow
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            packageType = ConfigPackageInfo.PackageType.Tutorial;
        }
    }
}