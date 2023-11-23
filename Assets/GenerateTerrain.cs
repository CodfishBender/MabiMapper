using MabiWorld;
using MabiWorld.Data;
using MabiWorld.FileFormats.PmgFormat;
using MabiWorld.FileFormats.SetFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
	public Material _defaultMatertial;
	public string _dataPath;
    public string _rgnPath;
	Region _region;
	float _worldScale = 0.01f; // Converts units from milimmeters to meters
	Dictionary<string, string> _texPaths = new();
	Dictionary<string, Material> _terrainMats = new();

	/// <summary>
	/// Shows the dialog for selecting the data folder.
	/// </summary>
	public void OpenDataDialog() {
		_dataPath = EditorUtility.OpenFolderPanel(
					"Select Data Folder",
					"Data",
					"");
		LoadData(_dataPath);
	}
	/// <summary>
	/// Shows the dialog for selecting the region file.
	/// </summary>
	public void OpenRegionDialog() {
		_rgnPath = EditorUtility.OpenFilePanel(
                    "Open file",
                    "Region File",
                    "rgn");
		LoadData(_dataPath);
	}

	/// <summary>
	/// Loads from data if data paths are valid.
	/// </summary>
	void LoadData(string dataPath) {
		_region = Region.ReadFromFile(_rgnPath);
		string path;
		if (!Directory.Exists(dataPath)) {
			Debug.LogError("Directory doesn't exist: " + dataPath);
			return;
		}

		path = Path.Combine(dataPath, "local");
		if (Directory.Exists(path)) Local.Load(path);

		path = Path.Combine(dataPath, "features.xml.compiled");
		if (File.Exists(path)) {
			Features.Clear();
			Features.Load(path);
			Features.SelectSetting("USA", false, false);
		}

		path = Path.Combine(dataPath, "db", "propdb.xml");
		if (File.Exists(path)) PropDb.Load(path);

		path = Path.Combine(dataPath, "material", "_define", "material");
		if (Directory.Exists(path)) MaterialDb.Load(path);

		path = Path.Combine(dataPath, "material", "_define", "render_state");
		if (Directory.Exists(path)) RenderStateDb.Load(path);

		path = Path.Combine(dataPath, "world", "proppalette.plt");
		if (File.Exists(path)) PropPalette.Load(path);

		path = Path.Combine(dataPath, "db", "minimapinfo.xml");
		if (File.Exists(path)) MiniMapInfo.Load(path);

		path = Path.Combine(dataPath, "db", "tileindex.data");
		if (File.Exists(path)) TileIndex.Load(path);

		// Unused - render.data was made obsolete after a certain point (unknown when)
		//path = Path.Combine(regionPath, "db", "render.data");
		//if (File.Exists(path) && !Render.HasEntries) Render.Load(path);
	}

	/// <summary>
	/// Begins the terrain generation process.
	/// </summary>
	public void CreateTerrain() {
		if (string.IsNullOrWhiteSpace(_dataPath) || !MaterialDb.HasEntries) {
			Debug.LogError("Set data path before generating terrain.");
			return;
		}
		LoadData(_dataPath);
		if (!MaterialDb.HasEntries) {
			Debug.LogError("No MaterialDb found!");
			return;
		}
		ClearTerrain();

		/* Mabinogi's terrain structure is as follows:
			- Region
				A Region is essentially an entire game world map.
			- Area
				Areas are sub sections of the region.
				They can contain any amount of AreaPlanes.
			- AreaPlane
				AreaPlanes are sub sections of areas.
				They contain a fixed size of 25 Planes in 5x5 grid.
				Think of this as a square of 5 x 5 vertices.
			- Plane
				Planes are sub sections of AreaPlanes that function as 3D vertices. 
				Planes are arranged in a 5 x 5 grid, with the last column/row 
				overlapping the next AreaPlane. Planes only have height data.
				x/y is inferred from the order of the array.
				[ 20 21 22 23 24 ]
				[ 15 16 17 18 19 ]
				[ 10 11 12 13 14 ]
				[ 5  6  7  8  9  ]
				[ 0  1  2  3  4  ]
		*/

		// Plane size is Area width / planeX / areaPlaneSize(always 4)
		// 5th row/column overlaps, so areaPlanes are only 4 planes wide
		float planeSize = 200f * _worldScale; // Mabinogi's units are in millimeters, so each vertex is 200mm apart
		float areaPlaneSize = planeSize * 4;

		// Create Region object
		GameObject regionObject = new GameObject(_region.Name);

		// Set the parent to the main terrain object
		regionObject.transform.parent = this.transform;
		regionObject.tag = "Region";

		// Iterate Areas
		for (int iArea = 0; iArea < _region.AreaCount; iArea++) {
			// Get individual area
			Area area = _region.Areas[iArea];
			// Origin point on terrain is always bottom left
			float areaX = area.BottomLeft.X * _worldScale; // X coord
			float areaZ = area.BottomLeft.Y * _worldScale; // Z coord
											    // Y coord (height) is set by planes
			// Get AreaPlanes for this Area
			List<AreaPlane> areaPlanes = area.AreaPlanes;

			// Create Area object
			GameObject areaObject = new GameObject(area.Name);

			// Set the parent to the main terrain object
			areaObject.transform.parent = regionObject.transform;
			areaObject.transform.localPosition = new Vector3(areaX, 0, areaZ);

			// Track plane iteration
			int iAreaPlane = 0;
			// Iterate AreasPlanes on X axis
			for (int iAreaPlaneX = 0; iAreaPlaneX < area.PlaneX; iAreaPlaneX++) {
				// Iterate AreasPlanes on Y axis
				for (int iAreaPlaneY = 0; iAreaPlaneY < area.PlaneY; iAreaPlaneY++) {
					// Get individual AreaPlane
					AreaPlane areaPlane = areaPlanes[iAreaPlane];

					// Skip Plane if hidden
					if (areaPlane.ShowPlane == 1) { iAreaPlane++; continue; }

					string areaPlaneName = "AreaPlane_" + iAreaPlaneX + "_" + iAreaPlaneY;
					float areaPlaneX = iAreaPlaneX * areaPlaneSize;
					float areaPlaneY = iAreaPlaneY * areaPlaneSize;

					// Get collection of Planes
					List<MabiWorld.Plane> planes = areaPlane.Planes;

					// Create AreaPlate object
					GameObject areaPlaneObject = new GameObject(areaPlaneName);

					// Set the parent to the area object
					areaPlaneObject.transform.parent = areaObject.transform;
					areaPlaneObject.transform.localPosition = new Vector3(areaPlaneX, 0, areaPlaneY);

					// Instantiate mesh object and attach it to mesh filter
					Mesh mesh = new();
					MeshRenderer areaPlaneMeshRenderer = areaPlaneObject.AddComponent<MeshRenderer>();
					MeshFilter areaPlaneMeshFilter = areaPlaneObject.AddComponent<MeshFilter>();
					Vector3[] newVertices;
					int[] newTriangles;
					Vector2[] newUV;

					// Get tile name and set ids for areaPlane
					// ## I'm not smart enough to figure out tilesets, and Unity has no tileset terrain features to work with
					/*
					int[] tilesetIds;
					if (areaPlane.UseTiles == 0) {
						// Get tilesets
						tilesetIds = new int[areaPlane.MaterialSlots.Length];
						for (int i = 0; i < areaPlane.MaterialSlots.Length; i++) {
							if (TileIndex.TryGet(areaPlane.MaterialSlots[i], out TileIndexEntry entry)) {
								tileName = entry.TileName;
								tilesetIds[i] = entry.TileID;
							}
						}
					} else {
						// Get individual tiles
						tilesetIds = new int[areaPlane.MaterialSlotIndexes.Length];
						for (int i = 0; i < areaPlane.MaterialSlotIndexes.Length; i++) {
							if (TileIndex.TryGet(areaPlane.MaterialSlotIndexes[i], out TileIndexEntry entry)) {
								tileName = entry.TileName;
								tilesetIds[i] = entry.TileID;
							}
						}
					}*/

					// Set areaPlane texture
					Material areaPlaneMat = _defaultMatertial;
					if (TileIndex.TryGet(areaPlane.MaterialSlots[0], out TileIndexEntry tile))
						// Try get existing material before generating a new one
						if (!_terrainMats.TryGetValue(tile.TileName, out areaPlaneMat))
							if (TryAddMatAndTexture(tile.TileName, out areaPlaneMat))
								_terrainMats[tile.TileName] = areaPlaneMat;

					// Make single primitive if flat surface
					if (areaPlane.MinHeight == areaPlane.MaxHeight) {
						float height = areaPlane.MinHeight * _worldScale;
						newVertices = new Vector3[4];
						newTriangles = new int[6];
						newUV = new Vector2[newVertices.Length];

						// Vertex
						newVertices[0] = new Vector3(0, height, 0);
						newVertices[1] = new Vector3(0, height, planeSize * 4);
						newVertices[2] = new Vector3(planeSize * 4, height, 0);
						newVertices[3] = new Vector3(planeSize * 4, height, planeSize * 4);

						// Assign UV
						for (int i = 0; i < newVertices.Length; i++) {
							newUV[i] = new Vector2(newVertices[i].x, newVertices[i].z).normalized;
						}

						// Tris
						newTriangles[0] = 1;
						newTriangles[1] = 2;
						newTriangles[2] = 0;

						newTriangles[3] = 1;
						newTriangles[4] = 3;
						newTriangles[5] = 2;
					} else {
						newVertices = new Vector3[25]; // 25 planes per areaPlane
						newTriangles = new int[150]; // 6 per vertex
						newUV = new Vector2[newVertices.Length];

						// Track plane iteration
						int iPlane = 0;
						// Iterate Planes on X axis
						for (int iPlaneX = 0; iPlaneX < 5; iPlaneX++) {
							// Iterate Planes on Y axis
							for (int iPlaneY = 0; iPlaneY < 5; iPlaneY++) {
								// Get individual plane
								MabiWorld.Plane plane = planes[iPlane];

								float[] planeXY = { iPlaneX * planeSize, iPlaneY * planeSize }; // X coord

								// Set vertex coords
								newVertices[iPlane] = new Vector3(planeXY[0], plane.Height * _worldScale, planeXY[1]);

								/* Planes don't hold triangle data so we have to assume which vertices connect.
									[ 20 21 22 23 24 ]
									[ 15 16 17 18 19 ]
									[ 10 11 12 13 14 ]
									[ 5  6  7  8  9  ]
									[ 0  1  2  3  4  ]
								*/
								// We have to form 2 triangles from the (quad) plane
								if (iPlaneX < 4 && iPlaneY < 4) {
									int iTri = iPlane * 6;
									newTriangles[iTri] = iPlane + 1;
									newTriangles[iTri + 1] = iPlane + 5;
									newTriangles[iTri + 2] = iPlane;

									newTriangles[iTri + 3] = iPlane + 1;
									newTriangles[iTri + 4] = iPlane + 6;
									newTriangles[iTri + 5] = iPlane + 5;
								}

								// Assign UV
								newUV[iPlane] = new Vector2(newVertices[iPlane].x / planeSize, newVertices[iPlane].z / planeSize) * 0.25f;

								// Iterate plane
								iPlane++;
							}
						}
					}

					// Assign vertices and triangles to the mesh
					areaPlaneMeshFilter.mesh = mesh;
					areaPlaneMeshRenderer.material = areaPlaneMat;
					mesh.vertices = newVertices;
					mesh.triangles = newTriangles;
					mesh.uv = newUV;
					mesh.Optimize();
					mesh.RecalculateNormals();

					// Iterate area plane
					iAreaPlane++;
				}
			}
		}
	}

	/// <summary>
	/// Removes all region objects from the terrain.
	/// </summary>
	public void ClearTerrain() {
		DestroyImmediate(GameObject.FindGameObjectWithTag("Region"));
	}

	/// <summary>
	/// Generates and populates the map with all props.
	/// </summary>
	public GameObject SpawnProps(bool spawnNormalProps, bool spawnEventProps, bool spawnDisabledProps) {
		if (string.IsNullOrWhiteSpace(_dataPath)) {
			Debug.LogError("Set data path before spawning props.");
			return null;
		}
		LoadData(_dataPath);
		if (!MaterialDb.HasEntries) {
			Debug.LogError("No MaterialDb found!");
			return null;
		}
		ClearProps();

		// Create Props parent
		GameObject propsObject = new GameObject("Props");

		// Set the parent to the main terrain object
		propsObject.transform.parent = this.transform;
		propsObject.tag = "Prop";

		// Iterate each prop
		foreach(Area area in _region.Areas) {
			foreach(Prop prop in area.Props) {
				GameObject newPropObj;
				GameObject existingPropObj = GameObject.Find(prop.ClassName);

				// Try get prop xml data
				PropDb.TryGetEntry(prop.Id, out PropDbEntry propDb);

				// Return if object is null
				if (propDb == null) {
					Debug.LogWarning("Could not find prop ID: " + prop.Id);
					continue;
				}

				bool isEventProp = propDb.StringID.Value.Contains("/event/") && propDb.UsedServer;
				if (isEventProp && !spawnEventProps) continue;
				bool isDisabledProp = !string.IsNullOrWhiteSpace(propDb.Feature) && !Features.IsEnabled(propDb.Feature);
				if (isDisabledProp && !spawnDisabledProps) continue;
				if (!isDisabledProp && !isEventProp && !spawnNormalProps) continue;

				if (existingPropObj == null) {
					// Generate new prop from file
					newPropObj = GenerateProp(prop, prop.Colors);
				} else {
					// Duplicate prop if it already exists
					newPropObj = CloneProp(existingPropObj, prop.Colors);
				}
				// Set prop transforms
				newPropObj.transform.parent = propsObject.transform;
				UpdatePropTransforms(prop, newPropObj.transform);
			}
		}
		EditorWindow view = EditorWindow.GetWindow<SceneView>();
		view.Repaint();
		return propsObject;
	}

	/// <summary>
	/// Clones an existing prop with updated colors.
	/// </summary>
	GameObject CloneProp(GameObject prop, Color[] colorOverrides) {
		// Instantiate new prop
		GameObject newProp = Instantiate(prop);
		// Set colors for new prop
		foreach (Renderer obj in newProp.GetComponentsInChildren<Renderer>()) {
			
			Color c = colorOverrides[obj.sharedMaterial.GetInteger("ColorIndex")];
			obj.sharedMaterial.color = TextureFactor(c);
			obj.transform.name = TextureFactor(c).ToString();
		}
		return newProp;
	}

	/// <summary>
	/// Generates a new prop from the data folder.
	/// </summary>
	GameObject GenerateProp(Prop prop, Color[] colorOverrides) {
		if (prop == null || prop.ClassName == null || prop.ClassName == "") return new GameObject("No prop data found for prop");

		string propPath = Path.Combine(_dataPath, "gfx");
		if (!Directory.Exists(propPath)) {
			Debug.LogError("Directory not found: " + propPath);
			return new GameObject("Directory not found: " + prop.ClassName);
		}
		PmgFile pmgFile;

		// Try get .set file
		FileInfo setFileInfo = new DirectoryInfo(propPath).EnumerateFiles(prop.ClassName + ".set", SearchOption.AllDirectories).FirstOrDefault();
		if (setFileInfo == null) {
			Debug.LogError("No .set file found: " + prop.ClassName + ".set");
			return new GameObject("No .set file found: " + prop.ClassName + ".set");
		}

		// Read set file data
		SetFile setFile = SetFile.ReadFrom(setFileInfo.FullName);

		// Fetch first pmg from set file
		FileInfo pmgFileInfo = new DirectoryInfo(setFileInfo.Directory.FullName).EnumerateFiles(setFile.Items[0].FileName + ".pmg", SearchOption.AllDirectories).FirstOrDefault();
		if (pmgFileInfo == null) {
			Debug.LogError("No .pmg file found: " + setFile.Items[0].FileName + ".pmg");
			return new GameObject("No .pmg file found: " + setFile.Items[0].FileName + ".pmg");
		}

		// Read all meshes from pmg file
		pmgFile = PmgFile.ReadFrom(pmgFileInfo.FullName);

		// Return if empty file
		if (pmgFile.Meshes == null || pmgFile.Meshes.Count == 0) return new GameObject("No pmgs in file: " + pmgFileInfo.FullName + ".pmg");

		// Create prop object
		GameObject propObject = new GameObject(prop.ClassName);


		// Iterate each mesh in prop
		for (int i = 0; i < pmgFile.Meshes.Count; i++) {
			Pmg pmg = pmgFile.Meshes[i];
			if (pmg == null) continue;
			if (pmg.TextureName.Equals("") || pmg.TextureName.Contains("__")) continue;

			// Create new object for each mesh
			GameObject meshObject = new GameObject(prop.ClassName);
			meshObject.transform.parent = propObject.transform;

			// Create mesh components
			Mesh mesh = new Mesh() { name = "mesh_" + pmg.MeshName };
			MeshRenderer propMeshRenderer = meshObject.AddComponent<MeshRenderer>();
			MeshFilter propMeshFilter = meshObject.AddComponent<MeshFilter>();

			// Add Material / Texture / Color
			// Set colors for new prop
			bool useTexture = TryAddMatAndTexture(pmg.TextureName, out Material meshMaterial);
			Color c = colorOverrides[pmg.ColorIndex];
			meshMaterial.color = TextureFactor(c);
			meshMaterial.SetInteger("ColorIndex", pmg.ColorIndex);

			// Add vertices
			Vector3[] newVertices = new Vector3[pmg.Vertices.Count];
			Vector2[] newUV = useTexture ? new Vector2[newVertices.Length] : null;
			for (int iVertex = 0; iVertex < newVertices.Length; iVertex++) {
				// Add vertex
				Vertex v = pmg.Vertices[iVertex];
				newVertices[iVertex] = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);

				// Assign UV
				if (useTexture) {
					newUV[iVertex].x = v.UV.X;
					newUV[iVertex].y = v.UV.Y;
				}
			}

			// Add tris
			int[] newTriangles = new int[pmg.IndexCount];
			for (int iFace = 0; iFace < newTriangles.Length; iFace++) {
				newTriangles[iFace] = pmg.Indices[iFace];
			}

			// Assign vertices and triangles to the mesh
			propMeshFilter.mesh = mesh;
			propMeshRenderer.material = meshMaterial;
			mesh.vertices = newVertices;
			mesh.triangles = newTriangles;
			mesh.uv = newUV;
			mesh.Optimize();
			mesh.RecalculateNormals();

			// Add mesh transforms
			meshObject.transform.localPosition = pmg.Matrix1.GetPosition();
			meshObject.transform.localRotation = pmg.Matrix1.rotation;
			meshObject.transform.localScale = pmg.Matrix1.lossyScale;
		}
		return propObject;
	}

	Color TextureFactor(Color baseColor) {
		float d = 128f;
		float r = Math.Min(baseColor.g / d, 1f);
		float g = Math.Min(baseColor.b / d, 1f);
		float b = Math.Min(baseColor.a / d, 1f);
		return new Color(r, g, b, 1f);
	}

	/// <summary>
	/// Takes a texture name and atempts to assign it to a material
	/// </summary>
	bool TryAddMatAndTexture(string textureName, out Material newMat) {

		// Load Unity material from materialDb data
		if (MaterialDb.TryGetFromTexture(textureName, out MaterialDbEntry matDb)) {

			// Load Unity material from file
			Material loadedMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/RenderStates/" + matDb.RenderState + ".asset", typeof(Material));
			// Create new temp material from loaded material
			newMat = new Material(loadedMat) { name = matDb.RenderState };
		} else {
			// Material data cannot be found in database
			// Create new temp material from default
			newMat = new Material(_defaultMatertial) { name = textureName };
			Debug.LogWarning("No material data for: " + textureName);
			return false;
		}

		// Try get existing texture path
		if (!_texPaths.TryGetValue(textureName, out string texPath))
			// Find texture in data folder
			texPath = FindTexture(textureName);

		if (texPath != null) {
			// Load texture from file
			Texture2D tex = LoadTextureDXT(texPath);
			// Update path dictionary
			_texPaths[textureName] = texPath;
			// Set texture
			if (tex != null) newMat.SetTexture("_MainTex", tex);
		} else
			return false;

		return true;
	}

	/// <summary>
	/// Generates and populates the map with all props.
	/// </summary>
	public void ClearProps() {
		DestroyImmediate(GameObject.FindGameObjectWithTag("Prop"));
	}

	/// <summary>
	/// Updates all prop transforms.
	/// </summary>
	void UpdatePropTransforms(Prop prop, Transform propObj) {
		float propRot = prop.Rotation;
		propObj.localPosition = new Vector3(prop.Position.X * _worldScale, prop.Position.Z * _worldScale, prop.Position.Y * _worldScale);
		float rot = propRot * -Mathf.Rad2Deg - 90f;
		propObj.localRotation = Quaternion.Euler(0, rot, 0);
		propObj.localScale = new Vector3(prop.Scale * _worldScale, prop.Scale * _worldScale, prop.Scale * _worldScale);

	}

	string FindTexture(string texName) {
		// Find texture
		// Check terrain folder
		string path = Path.Combine(_dataPath, "material", "terrain", texName, texName + ".dds");
		if (File.Exists(path)) return path;

		FileInfo texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "fx")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "obj")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "etc")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "interior")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "char")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "giant")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "glossmap")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "guildemblem")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "monster")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null)
			texFileInfo = new DirectoryInfo(Path.Combine(_dataPath, "material", "statue")).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		// Return if nothing found
		if (texFileInfo == null) {
			Debug.LogError("Cannot find texture file: " + texName + ".dds");
			return null;
		}
		return texFileInfo.FullName;
	}

	Texture2D LoadTextureDXT(string texPath) {

		byte[] ddsBytes = File.ReadAllBytes(texPath);

		// This header byte should be 124 for DDS image files
		if (ddsBytes[4] != 124)
			throw new Exception("Invalid DDS DXTn texture. Unable to read");

		int height = ddsBytes[13] * 256 + ddsBytes[12];
		int width = ddsBytes[17] * 256 + ddsBytes[16];

		// Try infer texture format from file
		int format = ddsBytes[87];
        var textureFormat = format switch {
            49 => TextureFormat.DXT1,
            51 => TextureFormat.DXT5, // Unity doesn't support DXT3, so we use DXT5
            53 => TextureFormat.DXT5,
            _ => TextureFormat.DXT5,
        };
        int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

		Texture2D texture = new Texture2D(width, height, textureFormat, false);
		texture.LoadRawTextureData(dxtBytes);
		texture.Apply();

		return (texture);
	}
}
