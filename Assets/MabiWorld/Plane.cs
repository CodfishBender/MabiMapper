using System.IO;
using System.Runtime.CompilerServices;
using MabiWorld.Extensions;
using UnityEngine;

namespace MabiWorld
{
	/// <summary>
	/// Represents a plane, a part of an .area file.
	/// </summary>
	public class Plane
	{
		public float Height { get; set; }
		public float UVX { get; set; }
		public float UVY { get; set; }
		public Color VertColor { get; set; }

		/// <summary>
		/// Reads plane from reader and returns it.
		/// </summary>
		/// <param name="br"></param>
		/// <param name="areaPlaneVersion"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Plane ReadFrom(BinaryReader br, byte? areaPlaneVersion)
		{
			var plane = new Plane();

			plane.Height = br.ReadSingle();
			if (areaPlaneVersion != null)
			{
				plane.UVX = br.ReadSingle();
				plane.UVY = br.ReadSingle();
			}
			plane.VertColor = br.ReadColor();

			return plane;
		}

		/// <summary>
		/// Writes plane to given writer.
		/// </summary>
		/// <param name="bw"></param>
		/// <param name="areaPlaneVersion"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteTo(BinaryWriter bw, byte? areaPlaneVersion)
		{
			bw.Write(this.Height);
			if (areaPlaneVersion != null)
			{
				bw.Write(this.UVX);
				bw.Write(this.UVY);
			}
			bw.WriteColor(this.VertColor);
		}
	}
}
