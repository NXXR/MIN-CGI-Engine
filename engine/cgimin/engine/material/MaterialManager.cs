using System;
using Engine.cgimin.engine.material.simpleblend;
using cgimin.engine.material.gbufferlayout;
using cgimin.engine.material.gbuffervalues;
using cgimin.engine.material.castshadow;

namespace cgimin.engine.material
{
	/// <summary>
	/// An Enum of all available Materials in the order of which they are drawn.
	/// </summary>
	public enum Material
	{
		GBUFFERLAYOUT = 0,
        GBUFFERVALUESET = 1,
        GBUFFERCOMPONENTS = 2,
        CASTSHADOW = 3,
        SIMPLE_BLEND = 4
	}

	public static class MaterialManager
	{
		private static readonly BaseMaterial[] Materials;

		static MaterialManager()
		{
			Materials = new BaseMaterial[Enum.GetNames(typeof(Material)).Length];

            Materials[(int)Material.GBUFFERLAYOUT] = new GBufferFromTwoTexturesMaterial();
            Materials[(int)Material.GBUFFERVALUESET] = new GBufferFromValuesMaterial();
            Materials[(int)Material.GBUFFERCOMPONENTS] = new GBufferFromComponentsMaterial();
            Materials[(int)Material.CASTSHADOW] = new CastShadowMaterial();
            Materials[(int)Material.SIMPLE_BLEND] = new SimpleBlendMaterial();
		}

		public static BaseMaterial GetMaterial(Material material) => Materials[(int) material];

		/// <summary>
		/// Draws all solid Materials in succession.
		/// </summary>
		public static void DrawAllSolidMaterials()
		{
			foreach (var material in Materials)
			{
				if (!material.SortFarToNear) material.DrawAll();
			}
		}

        /// <summary>
        /// Draws all transparent Materials in succession.
        /// </summary>
        public static void DrawAllTransparentMaterials()
        {
            foreach (var material in Materials)
            {
                if (material.SortFarToNear) material.DrawAll();
            }
        }
    }
}