using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;


namespace MabiWorld.Data
{
	/// <summary>
	/// Represents an entry in "data/material/_define/material".
	/// </summary>
	public class MaterialDbEntry
	{
		public string MaterialName { get; internal set; }
		public List<string> Textures { get; internal set; }
		public string RenderState { get; internal set; }
		public string GlossTexture { get; internal set; }
		public bool EnableOutline { get; internal set; }
		public bool EnableAlphaSort { get; internal set; }
		public bool ReceiveMainLight { get; internal set; }
		public bool IsGrass { get; internal set; }
		public bool CastShadow { get; internal set; }
		public bool BodyVisible { get; internal set; }
		public bool Silhouette { get; internal set; }
		public bool EnableGlbClrOvr { get; internal set; }
		public bool BackFaceCullWhenAlpha { get; internal set; }
		public string TextureFormat { get; internal set; }
		public string Desc { get; internal set; }
	}

	/// <summary>
	/// Represents data's material.xml.
	/// </summary>
	public static class MaterialDb
	{
		private static Dictionary<string, MaterialDbEntry> _entries = new();
		private static Dictionary<string, MaterialDbEntry> _texturePaths = new();
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
		public static string[] GetAllMaterialNames() {
			return _entries.Select(z => z.Value.MaterialName).ToArray();
		}

		/// <summary>
		/// Get all possible texture names.
		/// </summary>
		public static List<string> GetAllTextureNames() {
			List<string> textureNames = new();
			foreach(MaterialDbEntry entry in _entries.Values)
				foreach (string tex in entry.Textures)
					if (!textureNames.Contains(tex)) textureNames.Add(tex);
			return textureNames;
		}

		/// <summary>
		/// Returns the first material containing the string.
		/// </summary>
		/// <param name="materialName"></param>
		/// <returns></returns>
		public static bool TryGetFromMaterial(string materialName, out MaterialDbEntry entry) {
			return _entries.TryGetValue(materialName, out entry);
		}

		/// <summary>
		/// Returns the first material containing the string.
		/// </summary>
		/// <param name="textureName"></param>
		/// <returns></returns>
		public static bool TryGetFromTexture(string textureName, out MaterialDbEntry entry) {
			return _entries.TryGetValue(textureName.ToLower(), out entry);
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
			}
		}

		/// <summary>
		/// Loads prop data from given XML file.
		/// </summary>
		/// <param name="stream"></param>
		public static void Load(Stream stream) {
			using (var sr = new StreamReader(stream, true))
			using (var xmlReader = new XmlTextReader(sr)) {
				MaterialDbEntry entry = new MaterialDbEntry();
				entry.Textures = new();

				while (xmlReader.Read()) {
					if (xmlReader.GetAttribute("RenderState") != null) {
						entry.MaterialName = xmlReader.GetAttribute("Name");
						entry.RenderState = xmlReader.GetAttribute("RenderState");
						entry.GlossTexture = xmlReader.GetAttribute("GlossTexture");
						entry.EnableOutline = (xmlReader.GetAttribute("EnableOutline") == "true");
						entry.EnableAlphaSort = (xmlReader.GetAttribute("EnableAlphaSort") == "true");
						entry.ReceiveMainLight = (xmlReader.GetAttribute("ReceiveMainLight") == "true");
						entry.IsGrass = (xmlReader.GetAttribute("IsGrass") == "true");
						entry.CastShadow = (xmlReader.GetAttribute("CastShadow") == "true");
						entry.BodyVisible = (xmlReader.GetAttribute("BodyVisible") == "true");
						entry.Silhouette = (xmlReader.GetAttribute("Silhouette") == "true");
						entry.EnableGlbClrOvr = (xmlReader.GetAttribute("EnableGlbClrOvr") == "true");
						entry.BackFaceCullWhenAlpha = (xmlReader.GetAttribute("BackFaceCullWhenAlpha") == "true");
						entry.TextureFormat = xmlReader.GetAttribute("TextureFormat");
						entry.Desc = xmlReader.GetAttribute("Desc");
					}
					if (xmlReader.Name == "TextureDesc") {
						string t = xmlReader.GetAttribute("Name");
						entry.Textures.Add(t);
					}
				}
				foreach (var tex in entry.Textures) {
					_entries[tex.ToLower()] = entry;
				}
			}
		}
	}
}
