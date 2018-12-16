using System;
using Engine.cgimin.engine.material.simpleblend;
using cgimin.engine.material.gbufferlayout;

namespace cgimin.engine.material
{
	/// <summary>
	/// An Enum of all available Materials in the order of which they are drawn.
	/// </summary>
	public enum Material
	{
		GBUFFERLAYOUT = 0,
		SIMPLE_BLEND = 1
	}

	public static class MaterialManager
	{
		private static readonly BaseMaterial[] Materials;

		static MaterialManager()
		{
			Materials = new BaseMaterial[Enum.GetNames(typeof(Material)).Length];

            Materials[(int)Material.GBUFFERLAYOUT] = new GBufferLayoutMaterial();
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