#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuperRodentGame.EditorTools
{
    public static class RatModelImporter
    {
        private const string ExternalRatPath = @"D:\vr-ar-nsu\rat.fbx";
        private const string TargetFolder = "Assets/Art/Models";
        private const string TargetAssetPath = "Assets/Art/Models/rat.fbx";

        [MenuItem("SuperRodent/Import Rat Placeholder Model")]
        public static void ImportRatPlaceholder()
        {
            if (!File.Exists(ExternalRatPath))
            {
                Debug.LogError($"Rat placeholder not found: {ExternalRatPath}");
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Art"))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }

            if (!AssetDatabase.IsValidFolder(TargetFolder))
            {
                AssetDatabase.CreateFolder("Assets/Art", "Models");
            }

            File.Copy(ExternalRatPath, TargetAssetPath, true);
            AssetDatabase.Refresh();
            Debug.Log($"Imported rat model to: {TargetAssetPath}");
        }
    }
}
#endif
