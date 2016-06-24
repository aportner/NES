using System;
using System.IO;

namespace NES
{
	public class Cartridge
	{
		public byte[] prg;
		public byte[] chr;
		public byte[] sram;
		public byte mirror;
		public byte mapper;
		public byte battery;

		public Cartridge ()
		{
			sram = new byte[0x2000];
		}
	}

	public interface INESLoader {
		Cartridge Load(BinaryReader reader);
	}

}

