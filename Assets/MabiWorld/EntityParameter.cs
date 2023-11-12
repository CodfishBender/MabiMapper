﻿using System.IO;
using System.Runtime.CompilerServices;
using MabiWorld.Extensions;

namespace MabiWorld
{
	/// <summary>
	/// Represents an entity parameter, part of an .area file and used
	/// by entites like Prop and Event.
	/// </summary>
	public class EntityParameter
	{
		public bool IsDefault { get; set; }

		public ParameterType Type { get; set; }

		public SignalType SignalType { get; set; }

		public string Name { get; set; }

		public string Xml { get; set; }

		/// <summary>
		/// Reads parameter from reader and returns it.
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityParameter ReadFrom(BinaryReader br)
		{
			var param = new EntityParameter();

			param.IsDefault = br.ReadBoolean();
			param.Type = (ParameterType)br.ReadInt32();
			param.SignalType = (SignalType)br.ReadInt32();
			param.Name = br.ReadWString();
			param.Xml = br.ReadWString();

			return param;
		}

		/// <summary>
		/// Writes parameter to the given writer.
		/// </summary>
		/// <param name="bw"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteTo(BinaryWriter bw)
		{
			bw.Write(this.IsDefault);
			bw.Write((int)this.Type);
			bw.Write((int)this.SignalType);
			bw.WriteWString(this.Name);
			bw.WriteWString(this.Xml);
		}

		/// <summary>
		/// Creates and returns copy of parameter.
		/// </summary>
		/// <returns></returns>
		public EntityParameter Copy()
		{
			var param = new EntityParameter();

			param.IsDefault = this.IsDefault;
			param.Type = this.Type;
			param.SignalType = this.SignalType;
			param.Name = this.Name;
			param.Xml = this.Xml;

			return param;
		}
	}
}
