using System;

namespace NES
{
	public interface IMemory
	{
		byte Read (UInt16 address);

		void Write (UInt16 address, byte value);
	}


	public class Memory : IMemory
	{
		private Console console;


		public Memory (Console console)
		{
			this.console = console;
		}


		public byte Read (UInt16 address)
		{
			if (address < 0x2000) {
				return console.ram [address % 0x0800];
			} else if (address < 0x4000) {
				return console.ppu.ReadRegister((UInt16)(0x2000 + address%8));
			} else if (address == 0x4014) {
				return console.ppu.ReadRegister(address);
			} else if (address == 0x4015) {
				//		return mem.console.APU.readRegister(address)
			} else if (address == 0x4016) {
				// return mem.console.Controller1.Read()
			} else if (address == 0x4017) {
				// return mem.console.Controller2.Read()
			} else if (address >= 0x6000) {
				return console.mapper.Read (address);
			}

			return 0;
		}

		public void Write (UInt16 address, byte value)
		{
			if (address < 0x2000) {
				console.ram [address % 0x0800] = value;
			} else if (address < 0x4000) {
				//mem.console.PPU.writeRegister(0x2000+address%8, value)
			} else if (address < 0x4014) {
				//mem.console.APU.writeRegister(address, value)
			} else if (address == 0x4014) {
				//mem.console.PPU.writeRegister(address, value)
			} else if (address == 0x4015) {
				//mem.console.APU.writeRegister(address, value)
			} else if (address == 0x4016) {
				//mem.console.Controller1.Write(value)
				//mem.console.Controller2.Write(value)
			} else if (address == 0x4017) {
				// mem.console.APU.writeRegister(address, value)
			} else if (address < 0x6000) {
				// TODO: I/O registers
			} else if (address >= 0x6000) {
				console.mapper.Write (address, value);
			}
		}
	}


	public class PPUMemory : IMemory {
		private Console console;


		private enum Mirror {
			Horizontal = 0,
			MirrorVertical,
			MirrorSingle0,
			MirrorSingle1,
			MirrorFour
		};


		private static UInt16[][] mirrorLookup = new ushort[][] {
			new ushort[] {0, 0, 1, 1},
			new ushort[] {0, 1, 0, 1},
			new ushort[] {0, 0, 0, 0},
			new ushort[] {1, 1, 1, 1},
			new ushort[] {0, 1, 2, 3},
		};


		public PPUMemory(Console console) {
			this.console = console;
		}


		public byte Read (UInt16 address)
		{
			address %= 0x4000;

			if (address < 0x2000) {
				return console.mapper.Read (address);
			} else if (address < 0x3F00) {
				byte mode = console.cart.mirror;
				return console.ppu.nameTableData [MirrorAddress (mode, address) % 2048];
			} else if (address < 0x4000) {
				return console.ppu.ReadPalette ((UInt16)(address % 32));
			}

			UnityEngine.Debug.LogError("Error reading ppu address: " + address.ToString());

			return 0;
		}


		public void Write (UInt16 address, byte value)
		{
			address %= 0x4000;

			if (address < 0x2000) {
				console.mapper.Write (address, value);
			} else if (address < 0x3F00) {
				byte mode = console.cart.mirror;
				console.ppu.nameTableData [MirrorAddress (mode, address) % 2048] = value;
			} else if (address < 0x4000) {
				console.ppu.WritePalette ((UInt16)(address % 32), value);
			} else {
				UnityEngine.Debug.LogError ("Error writing to ppu address: " + address);
			}
		}


		private UInt16 MirrorAddress(byte mode, UInt16 address) {
			address = (UInt16)((address - 0x2000) % 0x1000);

			int table = address / 0x0400;
			int offset = address % 0x0400;

			return (UInt16)(0x2000 + mirrorLookup [mode] [table] * 0x0400 + offset);
		}
	}
}
