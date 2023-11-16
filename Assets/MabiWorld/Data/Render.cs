using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataDogLib;
using UnityEditor;
using UnityEngine;

namespace MabiWorld.Data
{
	/// <summary>
	/// Represents an entry in the TexMatList table.
	/// </summary>
	public class TexMatListEntry
	{
		/// <summary>
		/// Gets or sets object name.
		/// </summary>
		public string Texture { get; set; }

		/// <summary>
		/// Gets or sets the material used by the object.
		/// </summary>
		public string Material { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int IsManaged { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string AliasTexture { get; set; }
	}

	/// <summary>
	/// Represents an entry in the MaterialList table.
	/// </summary>
	public class MaterialListEntry
	{
		/// <summary>
		/// Gets or sets object name.
		/// </summary>
		public string Material { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableCartoonRender { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool IsAlphaSort { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string GlossTexture { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool IsManagedTexture { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string RenderState { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool ReceiveMainLight { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool IsGrass { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool CastShadow { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool BodyVisible { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool Silhouette { get; set; }

		/// <summary>
		/// Gets or sets what texture format to use (e.g. dxt5) blank = dxt1.
		/// </summary>
		public string DdsType { get; set; }

		/// <summary>
		/// Gets or sets whether to use the color set in the prop data.
		/// </summary>
		public bool EnableGlobalColorOverride { get; set; }

		/// <summary>
		/// Gets or sets whether to use the color set in the prop data.
		/// </summary>
		public bool BackFaceCullWhenAlpha { get; set; }
	}

	/// <summary>
	/// Represents an entry in the TexMatList table.
	/// </summary>
	public class RenderStateListEntry
	{
		/// <summary>
		/// Gets or sets object name.
		/// </summary>
		public string RenderState { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string __NormalParameter__ { get; set; }

		/// <summary>
		/// Gets or sets the property (hex rgba, e.g. FF808080).
		/// </summary>
		public string TextureFactor { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string TextureStageState_0 { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string TextureStageState_1 { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string TextureStageState_2 { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableLight { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool NormalizeNormal { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int ShadingMethod { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableFSAA { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int Culling { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte __ZBuffer__ { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableZDepthTest { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int ZDepthFunc { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableZWrite { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte __Alpha__ { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableAlphaTest { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int AlphaReference { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int AlphaFunc { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableAlphaBlend { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int AlphaBlendOp { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int AlphaBlendSrc { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int AlphaBlendDest { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int __ColorSource__ { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool UseVertexColor { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int ColorSrcDiffuse { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int ColorSrcSpecular { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int ColorSrcEmissive { get; set; }

		/// <summary>
		/// Gets or sets the property (hex).
		/// </summary>
		public string MaterialDiffuse { get; set; }

		/// <summary>
		/// Gets or sets the property (hex).
		/// </summary>
		public string MaterialSpecular { get; set; }

		/// <summary>
		/// Gets or sets the property (hex).
		/// </summary>
		public string MaterialAmbient { get; set; }

		/// <summary>
		/// Gets or sets the property (hex).
		/// </summary>
		public string MaterialEmissive { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public string __PointSprite__ { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnablePointSprite { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int PointSpriteSize { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int PointSpriteSizeMin { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int PointSpriteSizeMax { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool ScalePointSpriteInCameraSpace { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte PointSpriteScaleFactorA { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte PointSpriteScaleFactorB { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte PointSpriteScaleFactorC { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public byte __FOG__ { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public bool EnableFog { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int FogBegin { get; set; }

		/// <summary>
		/// Gets or sets the property (?).
		/// </summary>
		public int FogEnd { get; set; }
	}

	/// <summary>
	/// Represents an entry in the TextureStageStateList table.
	/// </summary>
	public class TextureStageStateListEntry
	{
		/// <summary>
		/// Gets or sets object name.
		/// </summary>
		public string TextureStageState { get; set; }

	}
	/// <summary>
	/// Represents "data/db/tileindex.data", which contains material
	/// information for terrains.
	/// </summary>
	public static class Render
	{
		private static Dictionary<string, MaterialListEntry> _materialList = new();
		private static Dictionary<string, RenderStateListEntry> _renderStateList = new();
		private static Dictionary<string, TexMatListEntry> _texMatList = new();

		/// <summary>
		/// Removes all entries.
		/// </summary>
		public static void Clear() {
			_materialList.Clear();
			_renderStateList.Clear();
			_texMatList.Clear();
		}

		/// <summary>
		/// Returns the material with the given string via out. Returns false if
		/// the material string doesn't exist.
		/// </summary>
		/// <param name="materialName"></param>
		/// <param name="materialListEntry"></param>
		/// <returns></returns>
		public static bool TryGet(string materialName, out MaterialListEntry entry) {
			return _materialList.TryGetValue(materialName, out entry);
		}

		/// <summary>
		/// Returns the render state with the given string via out. Returns false if
		/// the render state string doesn't exist.
		/// </summary>
		/// <param name="renderState"></param>
		/// <param name="renderStateEntry"></param>
		/// <returns></returns>
		public static bool TryGet(string renderState, out RenderStateListEntry entry) {
			return _renderStateList.TryGetValue(renderState, out entry);
		}

		/// <summary>
		/// Returns the material with the given string via out. Returns false if
		/// the material string doesn't exist.
		/// </summary>
		/// <param name="textureName"></param>
		/// <param name="materialListEntry"></param>
		/// <returns></returns>
		public static bool TryGet(string textureName, out TexMatListEntry entry) {
			return _texMatList.TryGetValue(textureName, out entry);
		}

		/// <summary>
		/// Loads render data from path.
		/// </summary>
		/// <param name="filePath"></param>
		public static void Load(string filePath) {
			Clear();
			if (Path.GetFileName(filePath) != "render.data")
				throw new ArgumentException("Expected file called render.data.");
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				Load(fs);
		}

		static Shader GetShaderFromRS(RenderStateListEntry rs) {
            return rs.RenderState switch {
				"rs_default" => Shader.Find("Standard"),
				"rs_useGlossMapGloss" => Shader.Find("Standard"),
				"rs_useGlossMapSphere" => Shader.Find("Standard"),
				"rs_terrain" => Shader.Find("Standard"),
				"rs_distantmesh" => Shader.Find("Standard"),
				"rs_gui_meshadd" => Shader.Find("Standard"),
				"rs_grass" => Shader.Find("Standard"),
				"rs_shade" => Shader.Find("Standard"),
				"rs_face" => Shader.Find("Standard"),
				"rs_useVertColOnly" => Shader.Find("Standard"),
				"rs_cursor" => Shader.Find("Standard"),
				"rs_terrain_obj" => Shader.Find("Standard"),
				"rs_useVertColOnly2x" => Shader.Find("Standard"),
				"rs_useVertColOnly_recvShadow" => Shader.Find("Standard"),
				"rs_effect" => Shader.Find("Particles/Standard Unlit"),
				"rs_useGlossMapGloss_half" => Shader.Find("Standard"),
				"rs_terrain_obj_cs" => Shader.Find("Standard"),
				"rs_useGlossMapGloss_1x" => Shader.Find("Standard"),
				"rs_effect_add" => Shader.Find("Particles/Standard Unlit"),
				"rs_effect_subtract" => Shader.Find("Particles/Standard Unlit"),
				"rs_useGlossMapSphereAdd" => Shader.Find("Standard"),
				"rs_useGlossMapSphere_1x" => Shader.Find("Standard"),
				"rs_nao_cloth" => Shader.Find("Standard"),
				"rs_nao_cloth2" => Shader.Find("Standard"),
				"rs_useVertColOnly_recvshadow_1x" => Shader.Find("Standard"),
				"rs_effect_alphablend" => Shader.Find("Particles/Standard Unlit"),
				"rs_useVertColOnly_mul_2x" => Shader.Find("Standard"),
				"rs_effect_add_noz" => Shader.Find("Particles/Standard Unlit"),
				"rs_waterstream1" => Shader.Find("Standard"),
				"rs_waterstream2" => Shader.Find("Standard"),
				"rs_useVertColOnly_add_2x_nozwrit" => Shader.Find("Standard"),
				"rs_useGlossMapGloss_add" => Shader.Find("Standard"),
				"rs_fx_astro" => Shader.Find("Particles/Standard Unlit"),
				"rs_useGlossMapGloss_zx" => Shader.Find("Standard"),
				"rs_VertColOnly_getShadow_alp_tsX" => Shader.Find("Standard"),
				"rs_VertColOnly2x_Wrap" => Shader.Find("Standard"),
				"rs_default_zwriteX" => Shader.Find("Standard"),
				"rs_obj_recvshadow" => Shader.Find("Standard"),
				"rs_useGlossMapGloss_add_wrap" => Shader.Find("Standard"),
				"rs_useVertColOnly2x_backX" => Shader.Find("Standard"),
				"rs_useGlossMapGloss_add_nosort" => Shader.Find("Standard"),
				_ => Shader.Find("Standard"),
			};
		}

		/// <summary>
		/// Loads tiles from given stream of a data dog file.
		/// </summary>
		/// <param name="stream"></param>
		public static void Load(Stream stream) {
			var dataDogFile = DataDogFile.Read(stream);

			if (!dataDogFile.Lists.TryGetValue("MaterialList", out var materialList))
				throw new ArgumentException($"DataDog object list 'MaterialList' not found in file.");
			if (!dataDogFile.Lists.TryGetValue("RenderStateList", out var renderStateList))
				throw new ArgumentException($"DataDog object list 'RenderStateList' not found in file.");
			if (!dataDogFile.Lists.TryGetValue("TexMatList", out var texMatList))
				throw new ArgumentException($"DataDog object list 'TexMatList' not found in file.");

			foreach (var obj in materialList.Objects) {
				var entry = new MaterialListEntry {
					Material = obj.Name,
					EnableCartoonRender = (bool)obj.Fields["EnableCartoonRender"].Value,
					GlossTexture = (string)obj.Fields["GlossTexture"].Value,
					IsManagedTexture = (bool)obj.Fields["IsManagedTexture"].Value,
					RenderState = (string)obj.Fields["RenderState"].Value,
					ReceiveMainLight = (bool)obj.Fields["ReceiveMainLight"].Value,
					IsGrass = (bool)obj.Fields["IsGrass"].Value,
					CastShadow = (bool)obj.Fields["CastShadow"].Value,
					BodyVisible = (bool)obj.Fields["BodyVisible"].Value,
					Silhouette = (bool)obj.Fields["Silhouette"].Value,
					DdsType = (string)obj.Fields["ddsType"].Value,
					EnableGlobalColorOverride = (bool)obj.Fields["EnableGlobalColorOverride"].Value,
					BackFaceCullWhenAlpha = (bool)obj.Fields["BackFaceCullWhenAlpha"].Value
				};
				_materialList[entry.Material] = entry;
			}
			foreach (var obj in renderStateList.Objects) {
				var entry = new RenderStateListEntry {
					RenderState = obj.Name,
					EnableAlphaBlend = (bool)obj.Fields["EnableAlphaBlend"].Value,
					EnableFog = (bool)obj.Fields["EnableFog"].Value,
					EnableAlphaTest = (bool)obj.Fields["EnableAlphaTest"].Value,
					EnableFSAA = (bool)obj.Fields["EnableFSAA"].Value,
					EnableLight = (bool)obj.Fields["EnableLight"].Value,
					EnablePointSprite = (bool)obj.Fields["EnablePointSprite"].Value,
					EnableZDepthTest = (bool)obj.Fields["EnableZDepthTest"].Value,
					EnableZWrite = (bool)obj.Fields["EnableZWrite"].Value,
					NormalizeNormal = (bool)obj.Fields["NormalizeNormal"].Value,
					ShadingMethod = (int)obj.Fields["ShadingMethod"].Value,
					UseVertexColor = (bool)obj.Fields["UseVertexColor"].Value,
					Culling = (int)obj.Fields["Culling"].Value
				};
				_renderStateList[entry.RenderState] = entry;
			}
			foreach (var obj in texMatList.Objects) {
				var entry = new TexMatListEntry {
					Texture = obj.Name,
					Material = (string)obj.Fields["Material"].Value,
					AliasTexture = (string)obj.Fields["AliasTexture"].Value,
					IsManaged = (int)obj.Fields["IsManaged"].Value
				};
				_texMatList[entry.Texture] = entry;
			}
		}
	}
}
