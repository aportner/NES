using System;
using System.IO;
using UnityEngine;

namespace NES
{
	public class PPU
	{
		/// <summary>
		/// Memory interface
		/// </summary>
		private IMemory memory;

		/// <summary>
		/// Console reference
		/// </summary>
		private Console console;


		private UInt32 cycle;
		// 0-340

		private UInt32 scanLine;
		// 0-261, 0-239=visible, 240=post, 241-260=vblank, 261=pre

		public byte[] paletteData;

		public byte[] nameTableData;

		public byte[] oamData;

		/// <summary>
		/// current vram address (15 bit)
		/// </summary>
		private UInt16 v;

		/// <summary>
		/// temporary vram address (15 bit)
		/// </summary>
		private UInt16 t;

		/// <summary>
		/// fine x scroll (3 bit)
		/// </summary>
		private byte x;

		/// <summary>
		/// write toggle (1 bit)
		/// </summary>
		private byte w;

		/// <summary>
		/// even/odd frame flag (1 bit)
		/// </summary>
		private byte f;

		private byte register;


		/// <summary>
		/// Frame counter
		/// </summary>
		private UInt64 frame;


		// NMI Flags
		private bool nmiOccurred;
		private bool nmiOutput;
		private bool nmiPrevious;
		private byte nmiDelay;

		//Background temporary variables
		private byte nameTableByte;
		private byte attributeTableByte;
		private byte lowTileByte;
		private byte highTileByte;
		private UInt64 tileData;

		// sprite temporary variables
		private int spriteCount;
		private UInt32[] spritePatterns;
		private byte[] spritePositions;
		private byte[] spritePriorities;
		private byte[] spriteIndexes;


		public Texture2D front;
		public Texture2D back;


		// $2000 PPUCTRL
		/// <summary>
		/// 0: $2000; 1: $2400; 2: $2800; 3: $2C00
		/// </summary>
		private byte flagNameTable;

		/// <summary>
		/// 0: add 1; 1: add 32
		/// </summary>
		private byte flagIncrement;

		/// <summary>
		/// 0: $0000; 1: $1000; ignored in 8x16 mode
		/// </summary>
		private byte flagSpriteTable;

		/// <summary>
		/// 0: $0000; 1: $1000
		/// </summary>
		private byte flagBackgroundTable;

		/// <summary>
		/// 0: 8x8; 1: 8x16
		/// </summary>
		private byte flagSpriteSize;

		/// <summary>
		/// 0: read EXT; 1: write EXT
		/// </summary>
		private byte flagMasterSlave;

		// $2001 PPUMASK
		/// <summary>
		/// Grayscale flag. 0: color; 1: grayscale
		/// </summary>
		private byte flagGrayscale;

		/// <summary>
		/// The flag show left background. 0: hide; 1: show
		/// </summary>
		private byte flagShowLeftBackground;

		/// <summary>
		/// The flag show left sprites. 0: hide; 1: show
		/// </summary>
		private byte flagShowLeftSprites;

		/// <summary>
		/// The flag show background. 0: hide; 1: show
		/// </summary>
		private byte flagShowBackground;

		/// <summary>
		/// The flag show sprites. 0: hide; 1: show
		/// </summary>
		private byte flagShowSprites;

		/// <summary>
		/// The flag red tint. 0: normal; 1: emphasized
		/// </summary>
		private byte flagRedTint;

		/// <summary>
		/// The flag green tint. 0: normal; 1: emphasized
		/// </summary>
		private byte flagGreenTint;

		/// <summary>
		/// The flag blue tint. 0: normal; 1: emphasized
		/// </summary>
		private byte flagBlueTint;

		// $2002 PPUSTATUS
		private byte flagSpriteZeroHit;
		private byte flagSpriteOverflow;

		// $2003 OAMADDR
		private byte oamAddress;

		// $2007 PPUDATA
		/// <summary>
		/// For buffered reads
		/// </summary>
		private byte bufferedData;


		public PPU (Console console)
		{
			this.console = console;
			memory = new PPUMemory (console);

			front = new Texture2D (256, 240);
			back = new Texture2D (256, 240);

			paletteData = new byte[32];
			nameTableData = new byte[2048];
			oamData = new byte[256];

			spritePatterns = new uint[8];
			spritePositions = new byte[8];
			spritePriorities = new byte[8];
			spriteIndexes = new byte[8];

			Reset ();
		}


		public void Save (BinaryWriter writer)
		{
			/*
			encoder.Encode(ppu.Cycle)
			encoder.Encode(ppu.ScanLine)
			encoder.Encode(ppu.Frame)
			encoder.Encode(ppu.paletteData)
			encoder.Encode(ppu.nameTableData)
			encoder.Encode(ppu.oamData)
			encoder.Encode(ppu.v)
			encoder.Encode(ppu.t)
			encoder.Encode(ppu.x)
			encoder.Encode(ppu.w)
			encoder.Encode(ppu.f)
			encoder.Encode(ppu.register)
			encoder.Encode(ppu.nmiOccurred)
			encoder.Encode(ppu.nmiOutput)
			encoder.Encode(ppu.nmiPrevious)
			encoder.Encode(ppu.nmiDelay)
			encoder.Encode(ppu.nameTableByte)
			encoder.Encode(ppu.attributeTableByte)
			encoder.Encode(ppu.lowTileByte)
			encoder.Encode(ppu.highTileByte)
			encoder.Encode(ppu.tileData)
			encoder.Encode(ppu.spriteCount)
			encoder.Encode(ppu.spritePatterns)
			encoder.Encode(ppu.spritePositions)
			encoder.Encode(ppu.spritePriorities)
			encoder.Encode(ppu.spriteIndexes)
			encoder.Encode(ppu.flagNameTable)
			encoder.Encode(ppu.flagIncrement)
			encoder.Encode(ppu.flagSpriteTable)
			encoder.Encode(ppu.flagBackgroundTable)
			encoder.Encode(ppu.flagSpriteSize)
			encoder.Encode(ppu.flagMasterSlave)
			encoder.Encode(ppu.flagGrayscale)
			encoder.Encode(ppu.flagShowLeftBackground)
			encoder.Encode(ppu.flagShowLeftSprites)
			encoder.Encode(ppu.flagShowBackground)
			encoder.Encode(ppu.flagShowSprites)
			encoder.Encode(ppu.flagRedTint)
			encoder.Encode(ppu.flagGreenTint)
			encoder.Encode(ppu.flagBlueTint)
			encoder.Encode(ppu.flagSpriteZeroHit)
			encoder.Encode(ppu.flagSpriteOverflow)
			encoder.Encode(ppu.oamAddress)
			encoder.Encode(ppu.bufferedData)
			*/
		}

		public void Load (BinaryReader reader)
		{
			/*
			decoder.Decode(&ppu.Cycle)
			decoder.Decode(&ppu.ScanLine)
			decoder.Decode(&ppu.Frame)
			decoder.Decode(&ppu.paletteData)
			decoder.Decode(&ppu.nameTableData)
			decoder.Decode(&ppu.oamData)
			decoder.Decode(&ppu.v)
			decoder.Decode(&ppu.t)
			decoder.Decode(&ppu.x)
			decoder.Decode(&ppu.w)
			decoder.Decode(&ppu.f)
			decoder.Decode(&ppu.register)
			decoder.Decode(&ppu.nmiOccurred)
			decoder.Decode(&ppu.nmiOutput)
			decoder.Decode(&ppu.nmiPrevious)
			decoder.Decode(&ppu.nmiDelay)
			decoder.Decode(&ppu.nameTableByte)
			decoder.Decode(&ppu.attributeTableByte)
			decoder.Decode(&ppu.lowTileByte)
			decoder.Decode(&ppu.highTileByte)
			decoder.Decode(&ppu.tileData)
			decoder.Decode(&ppu.spriteCount)
			decoder.Decode(&ppu.spritePatterns)
			decoder.Decode(&ppu.spritePositions)
			decoder.Decode(&ppu.spritePriorities)
			decoder.Decode(&ppu.spriteIndexes)
			decoder.Decode(&ppu.flagNameTable)
			decoder.Decode(&ppu.flagIncrement)
			decoder.Decode(&ppu.flagSpriteTable)
			decoder.Decode(&ppu.flagBackgroundTable)
			decoder.Decode(&ppu.flagSpriteSize)
			decoder.Decode(&ppu.flagMasterSlave)
			decoder.Decode(&ppu.flagGrayscale)
			decoder.Decode(&ppu.flagShowLeftBackground)
			decoder.Decode(&ppu.flagShowLeftSprites)
			decoder.Decode(&ppu.flagShowBackground)
			decoder.Decode(&ppu.flagShowSprites)
			decoder.Decode(&ppu.flagRedTint)
			decoder.Decode(&ppu.flagGreenTint)
			decoder.Decode(&ppu.flagBlueTint)
			decoder.Decode(&ppu.flagSpriteZeroHit)
			decoder.Decode(&ppu.flagSpriteOverflow)
			decoder.Decode(&ppu.oamAddress)
			decoder.Decode(&ppu.bufferedData)
			*/
		}


		public void Reset ()
		{
			cycle = 340;
			scanLine = 240;
			frame = 0;

			WriteControl (0);
			WriteMask (0);
			WriteOAMAddress (0);
		}


		public byte ReadPalette (UInt16 address)
		{
			if (address >= 16 && address % 4 == 0) {
				address -= 16;
			}

			return paletteData [address];
		}


		public void WritePalette (UInt16 address, byte value)
		{
			if (address >= 16 && address % 4 == 0) {
				address -= 16;
			}

			paletteData [address] = value;
		}


		public byte ReadRegister (UInt16 address)
		{
			if (address == 0x2002)
				return ReadStatus ();
			else if (address == 0x2004)
				return ReadOAMData ();
			else if (address == 0x2007)
				return ReadData ();
			
			return 0;
		}



		public void WriteRegister (UInt16 address, byte value)
		{
			register = value;

			if (address == 0x2000)
				WriteControl (value);
			else if (address == 0x2001)
				WriteMask (value);
			else if (address == 0x2003)
				WriteOAMAddress (value);
			else if (address == 0x2004)
				WriteOAMData (value);
			else if (address == 0x2005)
				WriteScroll (value);
			else if (address == 0x2006)
				WriteAddress (value);
			else if (address == 0x2007)
				WriteData (value);
			else if (address == 0x4014)
				WriteDMA (value);
		}


		private void WriteControl (byte value)
		{
			flagNameTable = (byte)((value >> 0) & 3);
			flagIncrement = (byte)((value >> 2) & 1);
			flagSpriteTable = (byte)((value >> 3) & 1);
			flagBackgroundTable = (byte)((value >> 4) & 1);
			flagSpriteSize = (byte)((value >> 5) & 1);
			flagMasterSlave = (byte)((value >> 6) & 1);
			nmiOutput = (((value >> 7) & 1) == 1);
			NMIChange ();

			t = (UInt16)((t & 0xF3FF) | (((UInt16)(value) & 0x03) << 10));
		}


		private void WriteMask (byte value)
		{
			flagGrayscale = (byte)((value >> 0) & 1);
			flagShowLeftBackground = (byte)((value >> 1) & 1);
			flagShowLeftSprites = (byte)((value >> 2) & 1);
			flagShowBackground = (byte)((value >> 3) & 1);
			flagShowSprites = (byte)((value >> 4) & 1);
			flagRedTint = (byte)((value >> 5) & 1);
			flagGreenTint = (byte)((value >> 6) & 1);
			flagBlueTint = (byte)((value >> 7) & 1);
		}


		private byte ReadStatus ()
		{
			int result = register & 0x1F
			             | (flagSpriteOverflow << 5)
			             | (flagSpriteZeroHit << 6);
			
			if (nmiOccurred) {
				result |= 1 << 7;
			}

			nmiOccurred = false;
			NMIChange ();

			w = 0;

			return (byte)result;
		}

		private void WriteOAMAddress (byte value)
		{
			oamAddress = value;
		}


		private byte ReadOAMData ()
		{
			return oamData [oamAddress];
		}

		private void WriteOAMData (byte value)
		{
			oamData [oamAddress] = value;
			++oamAddress;
		}


		private void WriteScroll (byte value)
		{
			if (w == 0) {
				t = (UInt16)((t & 0xFFE0) | (value >> 3));
				x = (byte)(value & 0x07);
				w = 1;
			} else {
				t = (UInt16)((t & 0x8FFF) | ((value & 0x07) << 12));
				t = (UInt16)((t & 0xFC1F) | ((value & 0xF8) << 2));
				w = 0;
			}
		}


		private void WriteAddress (byte value)
		{
			if (w == 0) {
				t = (UInt16)((t & 0x80FF) | ((value & 0x3F) << 8));
				w = 1;
			} else {
				t = (UInt16)((t & 0xFF00) | value);
				v = t;
				w = 0;
			}
		}


		private byte ReadData ()
		{
			byte value = memory.Read (v);

			if ((v % 0x4000) < 0x3F00) {
				byte buffered = bufferedData;
				bufferedData = value;
				value = buffered;
			} else {
				bufferedData = memory.Read ((UInt16)(v - 0x1000));
			}

			// increment address
			IncrementAddress ();

			return value;
		}


		private void IncrementAddress ()
		{
			if (flagIncrement == 0)
				v += 1;
			else
				v += 32;
		}

		// $2007: PPUDATA (write)
		private void WriteData (byte value)
		{
			memory.Write (v, value);
			IncrementAddress ();
		}


		private void WriteDMA (byte value)
		{
			CPU cpu = console.cpu;
			int address = value << 8;

			for (int i = 0; i < 256; ++i) {
				oamData [oamAddress] = cpu.memory.Read ((UInt16)address);
				++oamAddress;
				++address;
			}

			cpu.stall += 513;

			if (cpu.cycles % 2 == 1)
				++cpu.stall;
		}


		private void IncrementX ()
		{
			// increment hori(v)
			// if coarse X == 31
			if ((v & 0x001F) == 31) {
				// coarse X = 0
				v &= 0xFFE0;
					
				// switch horizontal nametable
				v ^= 0x0400;
			} else {
				// increment coarse X
				++v;
			}
		}


		private void IncrementY ()
		{
			// increment vert(v)
			// if fine Y < 7
			if ((v & 0x7000) != 0x7000) {
				v += 0x1000;
			} else {
				// fine Y = 0
				v &= 0x8FFF;

				// let y = coarse Y
				int y = ((v & 0x03E0) >> 5);

				if (y == 29) {
					// coarse Y = 0
					y = 0;
						
					// switch vertical nametable
					v ^= 0x0800;
				} else if (y == 31) {
					// coarse Y = 0, nametable not switched
					y = 0;
				} else {
					// increment coarse Y
					++y;
				}

				// put coarse Y back into v
				v = (UInt16)((v & 0xFC1F) | (y << 5));
			}
		}


		private void CopyX ()
		{
			// hori(v) = hori(t)
			// v: .....F.. ...EDCBA = t: .....F.. ...EDCBA
			v = (UInt16)((v & 0xFBE0) | (t & 0x041F));
		}


		private void CopyY ()
		{
			// vert(v) = vert(t)
			// v: .IHGF.ED CBA..... = t: .IHGF.ED CBA.....
			v = (UInt16)((v & 0x841F) | (t & 0x7BE0));
		}


		private void NMIChange ()
		{
			bool nmi = (nmiOutput && nmiOccurred);

			if (nmi && nmiPrevious) {
				// TODO: this fixes some games but the delay shouldn't have to be so
				// long, so the timings are off somewhere
				nmiDelay = 15;
			}

			nmiPrevious = nmi;
		}


		private void SetVerticalBlank ()
		{
			Texture2D temp = front;
			back = front;
			front = temp;


			nmiOccurred = true;
			NMIChange ();
		}


		private void ClearVerticalBlank ()
		{
			nmiOccurred = false;
			NMIChange ();
		}


		private void FetchNameTableByte ()
		{
			int address = 0x2000 | (v & 0x0FFF);
			nameTableByte = memory.Read ((UInt16)address);
		}


		private void FetchAttributeTableByte ()
		{
			int address = 0x23C0 | (v & 0x0C00) | ((v >> 4) & 0x38) | ((v >> 2) & 0x07);
			int	shift = ((v >> 4) & 4) | (v & 2);

			attributeTableByte = (byte)(((memory.Read ((UInt16)address) >> shift) & 3) << 2);
		}


		private void FetchLowTileByte ()
		{
			int fineY = (v >> 12) & 7;
			byte table = flagBackgroundTable;
			byte tile = nameTableByte;

			int	address = 0x1000 * (UInt16)(table) + (UInt16)(tile) * 16 + fineY;
			lowTileByte = memory.Read ((UInt16)address);
		}


		private void FetchHighTileByte ()
		{
			int fineY = (v >> 12) & 7;
			byte table = flagBackgroundTable;
			byte tile = nameTableByte;

			int address = 0x1000 * (UInt16)(table) + (UInt16)(tile) * 16 + fineY;
			highTileByte = memory.Read ((UInt16)(address + 8));
		}


		private void StoreTileData ()
		{
			UInt32 data = 0;

			for (int i = 0; i < 8; ++i) {
				byte a = attributeTableByte;

				UInt32 p1 = (UInt32)((lowTileByte & 0x80) >> 7);
				UInt32 p2 = (UInt32)((highTileByte & 0x80) >> 6);

				lowTileByte = (byte)(lowTileByte << 1);
				highTileByte = (byte)(highTileByte << 1);

				data = data << 4;
				data = (data | a | p1 | p2);
			}

			tileData |= (UInt64)(data);
		}


		private UInt32 FetchTileData ()
		{
			return (UInt32)(tileData >> 32);
		}


		private byte BackgroundPixel ()
		{
			if (flagShowBackground == 0) {
				return 0;
			}

			UInt32 data = FetchTileData () >> ((7 - x) * 4);

			return (byte)(data & 0x0F);
		}


		private static byte[] emptySpritePixel = new byte[]{ 0, 0 };


		private byte[] SpritePixel ()
		{
			if (flagShowSprites == 0) {
				return emptySpritePixel;
			}

			for (int i = 0; i < spriteCount; i++) {
				int offset = (int)((cycle - 1) - (int)spritePositions [i]);

				if (offset < 0 || offset > 7)
					continue;
				
				offset = 7 - offset;

				byte color = (byte)((spritePatterns [i] >> (byte)(offset * 4)) & 0x0F);

				if (color % 4 == 0)
					continue;
				
				return new byte[] { (byte)i, color };
			}

			return emptySpritePixel;
		}


		private void RenderPixel ()
		{
			UInt32 x = cycle - 1;
			UInt32 y = scanLine;

			byte background = BackgroundPixel ();
			byte[] spritePixel = SpritePixel ();

			byte i = spritePixel [0];
			byte sprite = spritePixel [1];

			if (x < 8 && flagShowLeftBackground == 0) {
				background = 0;
			}

			if (x < 8 && flagShowLeftSprites == 0) {
				sprite = 0;
			}

			bool b = ((background % 4) != 0);
			bool s = ((sprite % 4) != 0);

			byte color;

			if (!b && !s) {
				color = 0;
			} else if (!b && s) {
				color = (byte)(sprite | 0x10);
			} else if (b && !s) {
				color = background;
			} else {
				if (spriteIndexes [i] == 0 && x < 255) {
					flagSpriteZeroHit = 1;
				}

				if (spritePriorities [i] == 0) {
					color = (byte)(sprite | 0x10);
				} else {
					color = background;
				}
			}

			Color32 c = Palette.GetColor (ReadPalette ((UInt16)color) % 64);

			back.SetPixel ((int)x, (int)y, c);
		}



		private UInt32 FetchSpritePattern (int i, int row)
		{
			byte tile = oamData [i * 4 + 1];
			byte attributes = oamData [i * 4 + 2];

			UInt16 address = 0;

			if (flagSpriteSize == 0) {
				if ((attributes & 0x80) == 0x80) {
					row = 7 - row;
				}

				byte table = flagSpriteTable;
				address = (UInt16)(0x1000 * (UInt16)table + (UInt16)tile * 16 + (UInt16)row);
			} else {
				if ((attributes & 0x80) == 0x80) {
					row = 15 - row;
				}

				byte table = (byte)(tile & 1);
				tile = (byte)(tile & 0xFE);

				if (row > 7) {
					++tile;
					row -= 8;
				}

				address = (UInt16)(0x1000 * (UInt16)table + (UInt16)tile * 16 + (UInt16)row);
			}

			byte a = (byte)((attributes & 3) << 2);
			byte lowTileByte = memory.Read (address);
			byte highTileByte = memory.Read ((UInt16)(address + 8));

			UInt32 data = 0;

			for (i = 0; i < 8; ++i) {
				byte p1 = 0, p2 = 0;

				if ((attributes & 0x40) == 0x40) {
					p1 = (byte)((lowTileByte & 1) << 0);
					p2 = (byte)((highTileByte & 1) << 1);

					lowTileByte >>= 1;
					highTileByte >>= 1;
				} else {
					p1 = (byte)((lowTileByte & 0x80) >> 7);
					p2 = (byte)((highTileByte & 0x80) >> 6);

					lowTileByte <<= 1;
					highTileByte <<= 1;
				}

				data <<= 4;
				data |= (UInt32)(a | p1 | p2);
			}

			return data;
		}


		void EvaluateSprites ()
		{
			int h = (flagSpriteSize == 0) ? 8 : 16;

			int count = 0;

			for (int i = 0; i < 64; ++i) {
				byte y = oamData [i * 4 + 0];
				byte a = oamData [i * 4 + 2];
				byte x = oamData [i * 4 + 3];

				int	row = (int)scanLine - (int)y;

				if (row < 0 || row >= h)
					continue;
				
				if (count < 8) {
					spritePatterns [count] = FetchSpritePattern (i, row);
					spritePositions [count] = x;
					spritePriorities [count] = (byte)((a >> 5) & 1);
					spriteIndexes [count] = (byte)i;
				}

				++count;
			}

			if (count > 8) {
				count = 8;
				flagSpriteOverflow = 1;
			}

			spriteCount = count;
		}


		// tick updates Cycle, ScanLine and Frame counters
		void Tick ()
		{
			if (nmiDelay > 0) {
				--nmiDelay;

				if (nmiDelay == 0 && nmiOutput && nmiOccurred) {
					console.cpu.TriggerNMI ();
				}
			}

			if (flagShowBackground != 0 || flagShowSprites != 0) {
				if (f == 1 && scanLine == 261 & cycle == 339) {
					cycle = 0;
					scanLine = 0;
					frame++;
					f ^= 1;

					return;
				}
			}

			++cycle;

			if (cycle > 340) {
				cycle = 0;
				++scanLine;

				if (scanLine > 261) {
					scanLine = 0;
					frame++;
					f ^= 1;
				}
			}
		}


		// Step executes a single PPU cycle
		public void Step ()
		{
			Tick ();

			bool renderingEnabled = flagShowBackground != 0 || flagShowSprites != 0;
			bool preLine = (scanLine == 261);
			bool visibleLine = (scanLine < 240);

			// postLine := ppu.ScanLine == 240
			bool renderLine = preLine || visibleLine;
			bool preFetchCycle = cycle >= 321 && cycle <= 336;
			bool visibleCycle = cycle >= 1 && cycle <= 256;
			bool fetchCycle = preFetchCycle || visibleCycle;

			// background logic
			if (renderingEnabled) {
				if (visibleLine && visibleCycle) {
					RenderPixel ();
				}

				if (renderLine && fetchCycle) {
					
					tileData <<= 4;

					switch (cycle % 8) {
					case 1:
						FetchNameTableByte ();
						break;

					case 3:
						FetchAttributeTableByte ();
						break;

					case 5:
						FetchLowTileByte ();
						break;

					case 7:
						FetchHighTileByte ();
						break;

					case 0:
						StoreTileData ();
						break;
					}
				}

				if (preLine && cycle >= 280 && cycle <= 304) {
					CopyY ();
				}

				if (renderLine) {
					if (fetchCycle && cycle % 8 == 0) {
						IncrementX ();
					}

					if (cycle == 256) {
						IncrementY ();	
					}

					if (cycle == 257) {
						CopyX ();
					}
				}
			}

			// sprite logic
			if (renderingEnabled) {
				if (cycle == 257) {
					if (visibleLine) {
						EvaluateSprites ();
					} else {
						spriteCount = 0;
					}
				}
			}

			// vblank logic
			if (scanLine == 241 && cycle == 1) {
				SetVerticalBlank ();
			}


			if (preLine && cycle == 1) {
				ClearVerticalBlank ();
				flagSpriteZeroHit = 0;
				flagSpriteOverflow = 0;
			}
		}
	}
}

/*

*/