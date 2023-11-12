using MabiWorld;
using MabiWorld.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
	public Material terrainMaterial;
	public string _dataPath;
	public string _rgnPath;
	public Region _region;
	public List<Area> _areas;

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
	/// Removes all children from the terrain object.
	/// </summary>
	public void ClearTerrain() {
		for (int i = 0; i<transform.childCount; i++) {
			DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

	/// <summary>
	/// Begins the terrain generation process.
	/// </summary>
	public void CreateTerrain() {
		/* Mabinogi's terrain structure is as follows:
			- Region
				A Region is essentially an entire game world map.
			- Area
				Areas are sub sections of the region that can be any number.
				They can contain any amount of AreaPlanes.
			- AreaPlane
				AreaPlanes are sub sections of areas that can be any number.
				They contain a fixed size of 25 Planes in 5x5 grid.
				Think of this as a square of 5 x 5 verticies.
			- Plane
				Planes are sub sections of AreaPlanes that function as 3D verticies. 
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
		_areas = _region.Areas;
		if (_region == null) return;
		if (_areas == null) return;

		int planeSize = 160;
		int areaPlaneSize = planeSize * 5;

		// Create Region object
		GameObject regionObject = new GameObject(_region.Name);

		// Set the parent to the main terrain object
		regionObject.transform.parent = this.transform;

		// Iterate Areas
		for (int iArea = 0; iArea < _areas.Count; iArea++) {
			// Get individual area
			Area area = _areas[iArea];
			float areaX = area.TopLeft.X; // X coord
			float areaY = area.TopLeft.Z; // Y coord
			float areaZ = area.TopLeft.Y; // Z coord

			// Get AreaPlanes for this Area
			List<AreaPlane> areaPlanes = area.AreaPlanes;

			// Create Area object
			GameObject areaObject = new GameObject(area.Name);
			areaObject.transform.position = new Vector3(areaX, areaZ, areaY);

			// Set the parent to the main terrain object
			areaObject.transform.parent = regionObject.transform;

			// Track plane iteration
			int iAreaPlane = 0;
			// Iterate AreasPlanes on X axis
			for (int iAreaPlaneX = 0; iAreaPlaneX < area.PlaneX; iAreaPlaneX++) {
				// Iterate AreasPlanes on Y axis
				for (int iAreaPlaneY = 0; iAreaPlaneY < area.PlaneY; iAreaPlaneY++) {
					// Get individual AreaPlane
					AreaPlane areaPlane = areaPlanes[iAreaPlane];
					String areaPlaneName = "AreaPlane_" + iAreaPlaneX + "_" + iAreaPlaneY;
					float areaPlaneX = (iAreaPlaneX * areaPlaneSize) - iAreaPlaneX * areaPlaneSize;
					float areaPlaneY = (iAreaPlaneY * areaPlaneSize) - iAreaPlaneY * areaPlaneSize;

					// Get collection of Planes
					List<MabiWorld.Plane> planes = areaPlane.Planes;

					// Create AreaPlate object
					GameObject areaPlaneObject = new GameObject(areaPlaneName);
					areaPlaneObject.transform.position = new Vector3 (areaPlaneX, 0, areaPlaneY);

					Vector3[] newVertices = new Vector3[25]; // 25 planes per areaPlane
					int[] newTriangles = new int[150]; // 3 axis per vertex

					// Set the parent to the area object
					areaPlaneObject.transform.parent = areaObject.transform;

					// Track plane iteration
					int iPlane = 0;
					// Iterate Planes on X axis
					for (int iPlaneX = 0; iPlaneX < 5; iPlaneX++) {
						// Iterate Planes on X axis
						for (int iPlaneY = 0; iPlaneY < 5; iPlaneY++) {
							// Get individual plane
							MabiWorld.Plane plane = planes[iPlane];
							int planeX = iPlaneX * planeSize; // X coord
							int planeY = iPlaneY * planeSize; // Y coord

							// Set vertex coords
							newVertices[iPlane] = new Vector3(planeX, plane.Height, planeY);

							/* Planes don't hold triangle data so we have to assume which verticies connect.
								[ 20 21 22 23 24 ]
								[ 15 16 17 18 19 ]
								[ 10 11 12 13 14 ]
								[ 5  6  7  8  9  ]
								[ 0  1  2  3  4  ]
							*/
							// We have to form 2 triangles from the (quad) plane.
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
					// Instantiate mesh object and attach it to mesh filter
					MeshRenderer areaPlaneMeshRenderer = (MeshRenderer)areaPlaneObject.AddComponent(typeof(MeshRenderer));
					MeshFilter areaPlaneMeshFilter = (MeshFilter)areaPlaneObject.AddComponent(typeof(MeshFilter));
					Mesh mesh = new Mesh();
					mesh.name = "mesh_" + areaPlaneName;
					areaPlaneMeshFilter = areaPlaneObject.GetComponent<MeshFilter>();
					areaPlaneMeshFilter.mesh = mesh;
					areaPlaneMeshRenderer.material = terrainMaterial;


					// Assign vertices and triangles to the mesh
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
	/// Loads data if data folder setting is valid.
	/// </summary>
	private void LoadData(String path) {
		try {
			if (!Directory.Exists(path))
				return;

			var localPath = Path.Combine(path, "local");
			if (Directory.Exists(localPath))
				Local.Load(localPath);

			var featuresPath = Path.Combine(path, "features.xml.compiled");
			if (File.Exists(featuresPath)) {
				Features.Load(featuresPath);
				Features.SelectSetting("USA", false, false);
			}

			var propDbPath = Path.Combine(path, "db", "propdb.xml");
			if (File.Exists(propDbPath))
				PropDb.Load(propDbPath);

			var propPalettePath = Path.Combine(path, "world", "proppalette.plt");
			if (File.Exists(propPalettePath))
				PropPalette.Load(propPalettePath);

			var miniMapInfoPath = Path.Combine(path, "db", "minimapinfo.xml");
			if (File.Exists(miniMapInfoPath))
				MiniMapInfo.Load(miniMapInfoPath);
		} catch (Exception ex) {
			Console.WriteLine("An error ocurred while loading data: " + ex);
		}
	}
}
