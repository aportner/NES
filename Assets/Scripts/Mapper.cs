using System;

namespace NES
{
	public interface IMapper
	{
		void Step ();

		byte Read (UInt16 address);

		void Write (UInt16 address, byte value);
	}

	public static class Mapper
	{
		public static IMapper Create (Cartridge cart)
		{
			if (cart.mapper == 0 || cart.mapper == 2) {
				return new Mapper2 (cart);
			}

			return null;
		}
	}

	public class Mapper2 : IMapper
	{
		private Cartridge cart;
		private int prgBanks, prgBank1, prgBank2;

		public Mapper2 (Cartridge cart)
		{
			this.cart = cart;

			prgBanks = cart.prg.Length / 0x4000;
			prgBank1 = 0;
			prgBank2 = prgBanks - 1;
		}

		public void Step ()
		{
		}

		public byte Read(UInt16 address) {
			int index;

			if (address < 0x2000) {
				return cart.chr [address];
			} else if (address >= 0xC000) {
				index = prgBank2 * 0x4000 + (int)(address - 0xC000);
				return cart.prg [index];
			} else if (address >= 0x8000) {
				index = prgBank1 * 0x4000 + (int)(address - 0x8000);
				return cart.prg [index];
			} else if (address >= 0x6000) {
				index = (int)(address) - 0x6000;
				return cart.sram [index];
			} else {
				//throw new Exception ("unhandled mapper2 read at address: " + address.ToString());
			}

			return 0;
		}


		public void Write(UInt16 address, byte value) {
			if (address < 0x2000) {
				cart.chr [address] = value;
			} else if (address >= 0x8000) {
				prgBank1 = (int)(value) % prgBanks;
			} else if (address >= 0x6000) {
				int index = (int)(address) - 0x6000;
					cart.sram[index] = value;
			} else {
				// throw new Exception("unhandled mapper2 write at address: " + address.ToString());
			}
		}
	}

	/*
	 * package nes


func NewMapper2(cartridge *Cartridge) Mapper {
	prgBanks := len(cartridge.PRG) / 0x4000
	prgBank1 := 0
	prgBank2 := prgBanks - 1
	return &Mapper2{cartridge, prgBanks, prgBank1, prgBank2}
}

func (m *Mapper2) Save(encoder *gob.Encoder) error {
	encoder.Encode(m.prgBanks)
	encoder.Encode(m.prgBank1)
	encoder.Encode(m.prgBank2)
	return nil
}

func (m *Mapper2) Load(decoder *gob.Decoder) error {
	decoder.Decode(&m.prgBanks)
	decoder.Decode(&m.prgBank1)
	decoder.Decode(&m.prgBank2)
	return nil
}

func (m *Mapper2) Step() {
}

*/
}

