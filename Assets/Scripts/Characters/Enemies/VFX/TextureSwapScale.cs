using UnityEngine;

namespace ProjectColombo.Enemies
{
    public class TextureSwapScale : MonoBehaviour
    {
        public Material mainMajorMaterial;
        public Material weaponMajorMaterial;
        public Material accessoryMajorMaterial;

        public Material mainMinorMaterial;
        public Material weaponMinorMaterial;
        public Material accessoryMinorMaterial;

        public SkinnedMeshRenderer modelRenderer;
        public MeshRenderer weaponRenderer;

        public void SetMaterials(GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                // Set both materials: index 0 = main, index 1 = accessory
                modelRenderer.materials = new Material[] { mainMajorMaterial, accessoryMajorMaterial };
                weaponRenderer.material = weaponMajorMaterial;
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                modelRenderer.materials = new Material[] { mainMinorMaterial, accessoryMinorMaterial };
                weaponRenderer.material = weaponMinorMaterial;
            }
        }

    }
}