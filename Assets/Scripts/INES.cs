using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NES {
	public class INES : INESLoader {
		private const UInt32 INES_MAGIC_NUMBER = 0x1a53454e;


		[StructLayout(LayoutKind.Sequential)]
		public struct INESHeader {
			public UInt32 magic;
			public byte numPRG;
			public byte numCHR;
			public byte control1;
			public byte control2;
			public byte numRam;

			[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 7)]
			public byte[] padding;
		}


		public INES() {}


		public Cartridge Load(BinaryReader reader) {
			INESHeader header = reader.ReadStruct<INESHeader> ();

			if (header.magic != INES_MAGIC_NUMBER)
				throw new Exception("File is not .NES file");

			int mapper1 = header.control1 >> 4;
			int mapper2 = header.control2 >> 4;
			int mapper = mapper1 | (mapper2 << 4);

			int mirror1 = header.control1 & 1;
			int mirror2 = (header.control1 >> 3) & 1;
			int mirror = mirror1 | (mirror2 << 1);

			int battery = (header.control1 >> 1) & 1;

			// trainer
			if ((header.control1 & 4) == 4)
				reader.ReadBytes (512);

			byte[] prg = reader.ReadBytes (header.numPRG * 16384);
			byte[] chr = null;

			if (header.numCHR > 0)
				chr = reader.ReadBytes (header.numCHR * 8192);
			else
				chr = new byte[8192];

			return new Cartridge {
				prg = prg,
				chr = chr,
				mapper = (byte)mapper,
				mirror = (byte)mirror,
				battery = (byte)battery
			};
		}
	}
}
