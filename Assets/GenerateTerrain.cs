using MabiWorld;
using MabiWorld.Data;
using MabiWorld.FileFormats.PmgFormat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
	public Material _matTerrain;
	public Material _matPropCutout;
	public Material _matPropOpaque;
	public string _dataPath;
    public string _rgnPath;
	Region _region;
	float _worldScale = 0.01f;

	/// <summary>
	/// Shows the dialog for selecting the data folder.
	/// </summary>
	public void OpenDataFolder() {
		_dataPath = EditorUtility.OpenFolderPanel(
					"Select Data Folder",
					"Data",
					"");
		LoadData(_dataPath);
	}
	/// <summary>
	/// Shows the dialog for selecting the region file.
	/// </summary>
	public void OpenRegionFile() {
		_rgnPath = EditorUtility.OpenFilePanel(
                    "Open file",
                    "Region File",
                    "rgn");
	}

	/// <summary>
	/// Begins the terrain generation process.
	/// </summary>
	public void CreateTerrain() {
		ClearTerrain();
		LoadData(_dataPath);

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

		_region = Region.ReadFromFile(_rgnPath);
		if (_region == null) return;


		// Plane size is Area width / planeX / areaPlaneSize (4)
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
					if (areaPlane.ShowPlane == 1) {
						iAreaPlane++;
						continue;
					}
					String areaPlaneName = "AreaPlane_" + iAreaPlaneX + "_" + iAreaPlaneY;
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
					Mesh mesh = new() {
						name = "mesh_" + areaPlaneName
					};
					MeshRenderer areaPlaneMeshRenderer = areaPlaneObject.AddComponent<MeshRenderer>();
					MeshFilter areaPlaneMeshFilter = areaPlaneObject.AddComponent<MeshFilter>();
					Vector3[] newVertices;
					int[] newTriangles; 

					// Make single primitive if flat surface
					if (areaPlane.MinHeight == areaPlane.MaxHeight) {
						float height = areaPlane.MinHeight * _worldScale;
						newVertices = new Vector3[4];
						newTriangles = new int[6];

						// Vertex
						newVertices[0] = new Vector3(0, height, 0);
						newVertices[1] = new Vector3(0, height, planeSize * 4);
						newVertices[2] = new Vector3(planeSize * 4, height, 0);
						newVertices[3] = new Vector3(planeSize * 4, height, planeSize * 4);

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

						// Track plane iteration
						int iPlane = 0;
						// Iterate Planes on X axis
						for (int iPlaneX = 0; iPlaneX < 5; iPlaneX++) {
							// Iterate Planes on X axis
							for (int iPlaneY = 0; iPlaneY < 5; iPlaneY++) {
								// Get individual plane
								MabiWorld.Plane plane = planes[iPlane];

								float planeX = iPlaneX * planeSize; // X coord
								float planeY = iPlaneY * planeSize; // Y coord

								// Set vertex coords
								newVertices[iPlane] = new Vector3(planeX, plane.Height * _worldScale, planeY);

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

								// Iterate plane
								iPlane++;
							}
						}

					}
					// Assign vertices and triangles to the mesh
					areaPlaneMeshFilter.mesh = mesh;
					areaPlaneMeshRenderer.material = _matTerrain;
					mesh.vertices = newVertices;
					mesh.triangles = newTriangles;
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
	public GameObject SpawnProps() {
		ClearProps();
		LoadData(_dataPath);
		_region = Region.ReadFromFile(_rgnPath);

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
				if (propDb == null) continue;

				if (existingPropObj == null) {
					// Generate new prop from file
					newPropObj = GenerateProp(prop, propDb);
				} else {
					// Duplicate prop if it already exists
					newPropObj = Instantiate(existingPropObj);
				}
				// Set prop transforms
				newPropObj.transform.parent = propsObject.transform;
				UpdatePropTransforms(prop, newPropObj.transform);
			}
        }

		return propsObject;
	}

	void UpdatePropTransforms(Prop prop, Transform propObj) {
		float propRot = prop.Rotation;
		propObj.localPosition = new Vector3(prop.Position.X * _worldScale, prop.Position.Z * _worldScale, prop.Position.Y * _worldScale);
		float rot = propRot * -Mathf.Rad2Deg - 90f;
		propObj.localRotation = Quaternion.Euler(0, rot, 0);
		propObj.localScale = new Vector3(prop.Scale * _worldScale, prop.Scale * _worldScale, prop.Scale * _worldScale);

	}

	/// <summary>
	/// Generates a new prop from the data folder.
	/// </summary>
	GameObject GenerateProp(Prop prop, PropDbEntry propDb) {

		// Find file
		String propPath = Path.Combine(_dataPath, "gfx", "scene");
		FileInfo fileInfo = new DirectoryInfo(propPath).EnumerateFiles(prop.ClassName + ".pmg", SearchOption.AllDirectories).FirstOrDefault();
		if (fileInfo == null) return new GameObject("#File Not Found " + prop.ClassName);

		// Fetch all meshes from file
		List<Pmg> pmgs = PmgFile.ReadFrom(fileInfo.FullName).Meshes;
		if (pmgs == null || pmgs.Count == 0) return new GameObject("#No Meshes in file " + prop.ClassName);

		// Create prop object
		GameObject propObject = new GameObject(prop.ClassName);

		// Iterate each mesh in prop
		foreach (Pmg pmg in pmgs) {
			if (pmg == null) continue;

			// Add material
			Material newMat;
			MaterialDb.TryGetValue(pmg.TextureName, out MaterialDbEntry matDb);

			// Create new object for each mesh
			GameObject meshObject = new GameObject((matDb != null) ? matDb.RenderState : prop.ClassName);
			meshObject.transform.parent = propObject.transform;

			// Create mesh components
			Mesh mesh = new Mesh() {
				name = "mesh_" + propDb.ClassName
			};
			MeshRenderer propMeshRenderer = meshObject.AddComponent<MeshRenderer>();
			MeshFilter propMeshFilter = meshObject.AddComponent<MeshFilter>();


			// Material cont.
            if (matDb != null) {
				//Debug.Log(matDb.Silhouette + " | " + matDb.BodyVisible + " | " + matDb.EnableAlphaSort + " | " + matDb.ReceiveMainLight + "Texture: " + pmg.TextureName);
				newMat = new Material(_matPropOpaque);
			} else {
				//Debug.Log("Texture: " + pmg.TextureName);
				newMat = new Material(_matPropOpaque);
			}
			bool useTexture = false;
			// Add texture
			if (pmg.IsTextureMapped == 1) {
				Texture2D tex = LoadTextureDXT(pmg.TextureName);
				if (tex != null) {
					newMat.mainTexture = tex;
					useTexture = true;
				}
			}

			// Add vertices
			Vector3[] newVertices = new Vector3[pmg.Vertices.Count];
			Vector2[] newUV = (useTexture) ? new Vector2[newVertices.Length] : null;
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
			propMeshRenderer.material = newMat;
			mesh.vertices = newVertices;
			mesh.triangles = newTriangles;
			mesh.uv = newUV;

			// Add mesh transforms
			meshObject.transform.localPosition = pmg.Matrix1.GetPosition();
			meshObject.transform.localRotation = pmg.Matrix1.rotation;
			meshObject.transform.localScale = pmg.Matrix1.lossyScale;
		}
		return propObject;
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
	public void UpdateProps() {

		LoadData(_dataPath);
		_region = Region.ReadFromFile(_rgnPath);

		// Iterate each prop
		foreach (Area area in _region.Areas) {
			foreach (Prop prop in area.Props) {
				// Get existing prop
				GameObject existingProp = GameObject.Find(prop.ClassName);
				if (existingProp == null) continue;
				// Set prop transforms
				UpdatePropTransforms(prop, existingProp.transform);
			}
		}
	}
	/// <summary>
	/// Loads data if data folder setting is valid.
	/// </summary>
	private void LoadData(String path) {
		if (!Directory.Exists(path))
			return;

		var localPath = Path.Combine(path, "local");
		if (Directory.Exists(localPath))
			Local.Load(localPath);

		var featuresPath = Path.Combine(path, "features.xml.compiled");
		if (File.Exists(featuresPath)) {
			Features.Clear();
			Features.Load(featuresPath);
			Features.SelectSetting("USA", false, false);
		}

		var propDbPath = Path.Combine(path, "db", "propdb.xml");
		if (File.Exists(propDbPath))
			PropDb.Load(propDbPath);

		var matDbPath = Path.Combine(path, "material", "_define", "material");
		if (Directory.Exists(matDbPath))
			MaterialDb.Load(matDbPath);

		var propPalettePath = Path.Combine(path, "world", "proppalette.plt");
		if (File.Exists(propPalettePath))
			PropPalette.Load(propPalettePath);

		var miniMapInfoPath = Path.Combine(path, "db", "minimapinfo.xml");
		if (File.Exists(miniMapInfoPath))
			MiniMapInfo.Load(miniMapInfoPath);
	}

	public Texture2D LoadTextureDXT(String texName) {

		// Find texture
		String texPath = Path.Combine(_dataPath, "material");
		FileInfo texFileInfo = new DirectoryInfo(texPath).EnumerateFiles(texName + ".dds", SearchOption.AllDirectories).FirstOrDefault();
		if (texFileInfo == null) {
			Debug.LogError("Texture not found! " + texName);
			return null;
		}

		byte[] ddsBytes = File.ReadAllBytes(texFileInfo.FullName);

		// This header byte should be 124 for DDS image files
		if (ddsBytes[4] != 124) 
			throw new Exception("Invalid DDS DXTn texture. Unable to read");  

		int height = ddsBytes[13] * 256 + ddsBytes[12];
		int width = ddsBytes[17] * 256 + ddsBytes[16];
		int format = ddsBytes[87];
		TextureFormat textureFormat = TextureFormat.DXT1;
		if (format == 53) textureFormat = TextureFormat.DXT5;

		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

		Texture2D texture = new Texture2D(width, height, textureFormat, false);
		texture.LoadRawTextureData(dxtBytes);
		texture.Apply();

		return (texture);
	}
}
