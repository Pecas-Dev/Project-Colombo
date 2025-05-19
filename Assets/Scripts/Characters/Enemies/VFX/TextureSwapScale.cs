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
        public MeshRenderer accessoryRenderer;

        public void SetMaterials(GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                modelRenderer.material = mainMajorMaterial;
                weaponRenderer.material = weaponMajorMaterial;
                accessoryRenderer.material = accessoryMajorMaterial;
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                modelRenderer.material = mainMinorMaterial;
                weaponRenderer.material = weaponMinorMaterial;
                accessoryRenderer.material = accessoryMinorMaterial;
            }
        }
    }
}