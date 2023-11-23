using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MabiWorld.Data
{
	/// <summary>
	/// Represents an entry in "data/material/_define/material".
	/// </summary>
	public class RenderStateEntry
	{
		public string Name { get; internal set; }
		public string MaxTextureStateStage { get; internal set; }
		public string TextureFactor { get; internal set; }

		public bool ZDepthTestEnabled { get; internal set; }
		public string ZDepthTestFunc { get; internal set; }
		public bool ZDepthTestWriteEnable { get; internal set; }

		public bool AlphaTestEnabled { get; internal set; }
		public string AlphaTestRef { get; internal set; }
		public string AlphaTestFunc { get; internal set; }


		public bool BlendEnabled { get; internal set; }
		public string BlendOp { get; internal set; }
		public string BlendSrc { get; internal set; }
		public string BlendDest { get; internal set; }


		public bool FogEnabled { get; internal set; }
		public bool FogRangeEnable { get; internal set; }
		public string FogMode { get; internal set; }
		public Color FogColor { get; internal set; }
		public float FogStart { get; internal set; }
		public float FogEnd { get; internal set; }
		public float FogDensity { get; internal set; }


		public bool StencilEnabled { get; internal set; }
		public string StencilFail { get; internal set; }
		public string StencilZFail { get; internal set; }
		public string StencilPass { get; internal set; }
		public string StencilFunc { get; internal set; }
		public string StencilRef { get; internal set; }
		public string StencilMask { get; internal set; }
		public string StencilWriteMask { get; internal set; }


		public bool ColorVertex { get; internal set; }
		public string DiffuseMatSrc { get; internal set; }
		public string SpecularMatSrc { get; internal set; }
		public string AmbientMatSrc { get; internal set; }
		public string EmissiveMatSrc { get; internal set; }
		public Color MatDiffuse { get; internal set; }
		public Color MatAmbient { get; internal set; }
		public Color MatSpecular { get; internal set; }
		public Color MatEmissive { get; internal set; }
		public float MatPower { get; internal set; }
		public string Ambient { get; internal set; }


		public string CullMode { get; internal set; }
		public bool Lighting { get; internal set; }
		public bool NormalizeNormals { get; internal set; }
		public string ShadeMode { get; internal set; }
		public string FillMode { get; internal set; }
		public bool MultiSampleAntiAlias { get; internal set; }
		public bool LastPixel { get; internal set; }
		public bool DitherEnable { get; internal set; }
		public bool LocalViewer { get; internal set; }
	}

	/// <summary>
	/// Represents data's material.xml.
	/// </summary>
	public static class RenderStateDb
	{
		private static Dictionary<string, RenderStateEntry> _entries = new();
		private static Dictionary<string, RenderStateEntry> _texturePaths = new();
		/// <summary>
		/// Returns true if the db has any data loaded.
		/// </summary>
		public static bool HasEntries => (_entries.Count > 0 || _texturePaths.Count > 0);

		/// <summary>
		/// Removes all entries.
		/// </summary>
		public static void Clear() {
			_entries.Clear();
		}

		/// <summary>
		/// Get all possible material names.
		/// </summary>
		public static string[] GetAllRSNames() {
			return _entries.Select(z => z.Value.Name).ToArray();
		}

		/// <summary>
		/// Returns the first material containing the string.
		/// </summary>
		/// <param name="materialName"></param>
		/// <returns></returns>
		public static bool TryGetRenderState(string renderState, out RenderStateEntry entry) {
			return _entries.TryGetValue(renderState, out entry);
		}

		static void GenerateMaterials() {
			if (!AssetDatabase.IsValidFolder("Assets/RenderStates/")) AssetDatabase.CreateFolder("Assets", "RenderStates");
			foreach(RenderStateEntry rs in _entries.Values) {
				Material loadedMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/RenderStatesBackup/" + rs.Name + ".asset", typeof(Material));
				if (loadedMat != null) {
					Material mat = new Material(loadedMat);
					if (rs.TextureFactor.Contains("808080")) {
						mat.SetColor("TextureFactor", Color.gray);
					} else
						mat.SetColor("TextureFactor", Color.black);
					AssetDatabase.CreateAsset(mat, "Assets/RenderStates/" + rs.Name + ".asset");
				}/*
				if (mat == null) {
					if (rs.Name.Contains("_effect")) {
						mat = new Material(Shader.Find("Particles/Standard Unlit"));
						if (rs.Name.Contains("_multiply")) mat.SetFloat("_ColorMode", 1);
						else if (rs.Name.Contains("_add")) mat.SetFloat("_ColorMode", 2);
						else if (rs.Name.Contains("_subtract")) mat.SetFloat("_ColorMode", 3);
						else if (rs.Name.Contains("_alphablend")) mat.SetFloat("_ColorMode", 4);
						mat.SetFloat("_LightingEnabled", rs.Lighting ? 1 : 0);
						mat.SetFloat("_EmissionEnabled", rs.Lighting ? 1 : 0);
						mat.SetFloat("_BlendOp", rs.Lighting ? 1 : 0);
						mat.SetFloat("_SrcBlend", rs.Lighting ? 1 : 0);
						mat.SetFloat("_DstBlend", rs.Lighting ? 1 : 0);
						mat.SetFloat("_ZWrite", rs.ZDepthTestWriteEnable ? 1 : 0);
						mat.SetFloat("_Cull", GetIntFromD3DCULL(rs.CullMode));
					} else {
						mat = new Material(Shader.Find(rs.Lighting ? "StandardDoubleSide" : "StandardDoubleSideUnlit"));
						mat.SetFloat("_Mode", rs.CullMode.Contains("NONE") ? 1 : 0);
					}
					mat.SetColor("_EmissionColor", rs.MatEmissive);
					mat.SetFloat("_SrcBlend", GetIntFromD3DBLEND(rs.BlendSrc));
					mat.SetFloat("_DstBlend", GetIntFromD3DBLEND(rs.BlendSrc));
					mat.SetFloat("_ZWrite", rs.ZDepthTestWriteEnable ? 1 : 0);
					mat.color = rs.MatDiffuse;
					mat.doubleSidedGI = true;
					mat.name = rs.Name;

					// Create asset file
					AssetDatabase.CreateAsset(mat, "Assets/RenderStates/" + rs.Name + ".asset");
				}*/
			}
			AssetDatabase.SaveAssets();
		}

		static int GetIntFromD3DBLEND(string str) {
			return str switch {
				"D3DBLEND_ZERO" => 1,
				"D3DBLEND_ONE" => 2,
				"D3DBLEND_SRCCOLOR" => 3,
				"D3DBLEND_INVSRCCOLOR" => 4,
				"D3DBLEND_SRCALPHA" => 5,
				"D3DBLEND_INVSRCALPHA" => 6,
				"D3DBLEND_DESTALPHA" => 7,
				"D3DBLEND_INVDESTALPHA" => 8,
				"D3DBLEND_DESTCOLOR" => 9,
				"D3DBLEND_INVDESTCOLOR" => 10,
				"D3DBLEND_SRCALPHASAT" => 11,
				"D3DBLEND_BOTHSRCALPHA" => 12,
				"D3DBLEND_BOTHINVSRCALPHA" => 13,
				"D3DBLEND_BLENDFACTOR" => 14,
				"D3DBLEND_INVBLENDFACTOR" => 15,
				"D3DBLEND_SRCCOLOR2" => 16,
				"D3DBLEND_INVSRCCOLOR2" => 17,
				_ => 1,
			};
		}

		static int GetIntFromD3DCULL(string str) {
			return str switch {
				"D3DCULL_NONE" => 1,
				"D3DCULL_CW" => 2,
				"D3DCULL_CCW" => 3,
				_ => 1,
			};
		}

		/// <summary>
		/// Loads material data from all material xml files.
		/// </summary>
		/// <param name="filePath"></param>
		public static void Load(string filePath) {
			Clear();
			FileInfo[] fileInfos = new DirectoryInfo(filePath).EnumerateFiles("*.xml", SearchOption.AllDirectories).ToArray();
			foreach (FileInfo fileInfo in fileInfos) {
				try {
					using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
						Load(fs);
				} catch (Exception ex) { Debug.LogError(fileInfo.FullName + " ### " + ex); }
				// GenerateMaterials(); // Used for generating the initial render state materials
			}
		}

		/// <summary>
		/// Loads prop data from given XML file.
		/// </summary>
		/// <param name="stream"></param>
		public static void Load(Stream stream) {
			using (var sr = new StreamReader(stream, true))
			using (var xmlReader = new XmlTextReader(sr)) {
				RenderStateEntry entry = new RenderStateEntry();

				while (xmlReader.Read()) {
					if (xmlReader.GetAttribute("TextureFactor") != null) {
						entry.Name = xmlReader.GetAttribute("Name");
						entry.MaxTextureStateStage = xmlReader.GetAttribute("MaxTextureStateStage");
						entry.TextureFactor = xmlReader.GetAttribute("TextureFactor");
					}
					if (xmlReader.Name.Equals("ZDepthTest")) {
						entry.ZDepthTestEnabled = xmlReader.GetAttribute("Enable") == "true";
						entry.ZDepthTestFunc = xmlReader.GetAttribute("Func");
						entry.ZDepthTestWriteEnable = xmlReader.GetAttribute("WriteEnable") == "true";
					}
					if (xmlReader.Name.Equals("AlphaTest")) {
						entry.AlphaTestEnabled = xmlReader.GetAttribute("Enable") == "true";
						entry.AlphaTestRef = xmlReader.GetAttribute("Func");
						entry.AlphaTestFunc = xmlReader.GetAttribute("WriteEnable");
					}
					if (xmlReader.Name.Equals("Blend")) {
						entry.BlendEnabled = xmlReader.GetAttribute("Enable") == "true";
						entry.BlendOp = xmlReader.GetAttribute("Func");
						entry.BlendSrc = xmlReader.GetAttribute("WriteEnable");
					}
					if (xmlReader.Name.Equals("Fog")) {
						entry.FogEnabled = xmlReader.GetAttribute("Enable") == "true";
						entry.FogRangeEnable = xmlReader.GetAttribute("RangeEnable") == "true"; ;
						entry.AlphaTestFunc = xmlReader.GetAttribute("WriteEnable");
					}
					if (xmlReader.Name.Equals("Stencil")) {
						entry.StencilEnabled = xmlReader.GetAttribute("Enable") == "true";
						entry.StencilFail = xmlReader.GetAttribute("Fail");
						entry.StencilZFail = xmlReader.GetAttribute("ZFail");
						entry.StencilPass = xmlReader.GetAttribute("Pass");
						entry.StencilFunc = xmlReader.GetAttribute("Func");
						entry.StencilRef = xmlReader.GetAttribute("Ref");
						entry.StencilRef = xmlReader.GetAttribute("Mask");
						entry.StencilRef = xmlReader.GetAttribute("WriteMask");
					}
					if (xmlReader.Name.Equals("ColorSrc")) {
						entry.ColorVertex = xmlReader.GetAttribute("Enable") == "true";
						entry.MatDiffuse = new Color(float.Parse(xmlReader.GetAttribute("MatDiffuseR")), float.Parse(xmlReader.GetAttribute("MatDiffuseG")), float.Parse(xmlReader.GetAttribute("MatDiffuseB")), float.Parse(xmlReader.GetAttribute("MatDiffuseA")));
						entry.MatSpecular = new Color(float.Parse(xmlReader.GetAttribute("MatSpecularR")), float.Parse(xmlReader.GetAttribute("MatSpecularG")), float.Parse(xmlReader.GetAttribute("MatSpecularB")), float.Parse(xmlReader.GetAttribute("MatSpecularA")));
						entry.MatAmbient = new Color(float.Parse(xmlReader.GetAttribute("MatAmbientR")), float.Parse(xmlReader.GetAttribute("MatAmbientG")), float.Parse(xmlReader.GetAttribute("MatAmbientB")), float.Parse(xmlReader.GetAttribute("MatAmbientA")));
						entry.MatEmissive = new Color(float.Parse(xmlReader.GetAttribute("MatEmissiveR")), float.Parse(xmlReader.GetAttribute("MatEmissiveG")), float.Parse(xmlReader.GetAttribute("MatEmissiveB")), float.Parse(xmlReader.GetAttribute("MatEmissiveA")));
						entry.MatPower = float.Parse(xmlReader.GetAttribute("MatPower"));
					}
					if (xmlReader.Name.Equals("Misc")) {
						entry.CullMode = xmlReader.GetAttribute("CullMode");
						entry.Lighting = xmlReader.GetAttribute("Lighting") == "true";
						entry.ShadeMode = xmlReader.GetAttribute("ShadeMode");
						entry.FillMode = xmlReader.GetAttribute("FillMode");
						entry.MultiSampleAntiAlias = xmlReader.GetAttribute("MultiSampleAntiAlias") == "true";
						entry.LastPixel = xmlReader.GetAttribute("LastPixel") == "true";
						entry.DitherEnable = xmlReader.GetAttribute("DitherEnable") == "true";
						entry.LocalViewer = xmlReader.GetAttribute("LocalViewer") == "true";
					}
				}
				_entries[entry.Name] = entry;
			}
		}
	}
}
