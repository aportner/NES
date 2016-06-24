using System;
using System.IO;


namespace NES
{
	public class StepInfo
	{
		public UInt16 address;
		public UInt16 pc;
		public CPU.AddressMode mode;
	}


	public class AddressInfo
	{
		public UInt16 address;
		public bool pageCrossed;
	}


	public class CPU
	{
		/// <summary>
		/// The FREQUENCY of the CPU.
		/// </summary>
		public const int FREQUENCY = 1789773;


		/// <summary>
		/// Type of interrupt
		/// </summary>
		public enum Interrupt
		{
			None = 1,
			NMI,
			IRQ
		}


		/// <summary>
		/// Addressing mode
		/// </summary>
		public enum AddressMode
		{
			Absolute = 1,
			AbsoluteX,
			AbsoluteY,
			Accumulator,
			Immediate,
			Implied,
			IndexedIndirect,
			Indirect,
			IndirectIndexed,
			Relative,
			ZeroPage,
			ZeroPageX,
			ZeroPageY
		}


		/// <summary>
		/// Addressing mode of each instruction
		/// </summary>
		private static byte[] INSTRUCTION_MODES = new byte[] {
			6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
			1, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
			6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
			6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 8, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
			5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
			5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
			5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
			5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
			10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
		};


		/// <summary>
		/// Size of each instruction, in bytes
		/// </summary>
		private static byte[] INSTRUCTION_SIZES = new byte[] {
			1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			3, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 0, 3, 0, 0,
			2, 2, 2, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
			2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
		};


		/// <summary>
		/// Number of instructions per cycle
		/// </summary>
		private static byte[] INSTRUCTION_CYCLES = new byte[] {
			7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
			2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
			2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
			2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
			2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		};


		/// <summary>
		/// Amount of instructions if a page cycle is crossed
		/// </summary>
		private static byte[] INSTRUCTION_PAGE_CYCLES = new byte[] {
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
		};


		private static string[] INSTRUCITON_NAMES = new string[] {
			"BRK", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
			"PHP", "ORA", "ASL", "ANC", "NOP", "ORA", "ASL", "SLO",
			"BPL", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
			"CLC", "ORA", "NOP", "SLO", "NOP", "ORA", "ASL", "SLO",
			"JSR", "AND", "KIL", "RLA", "BIT", "AND", "ROL", "RLA",
			"PLP", "AND", "ROL", "ANC", "BIT", "AND", "ROL", "RLA",
			"BMI", "AND", "KIL", "RLA", "NOP", "AND", "ROL", "RLA",
			"SEC", "AND", "NOP", "RLA", "NOP", "AND", "ROL", "RLA",
			"RTI", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
			"PHA", "EOR", "LSR", "ALR", "JMP", "EOR", "LSR", "SRE",
			"BVC", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
			"CLI", "EOR", "NOP", "SRE", "NOP", "EOR", "LSR", "SRE",
			"RTS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
			"PLA", "ADC", "ROR", "ARR", "JMP", "ADC", "ROR", "RRA",
			"BVS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
			"SEI", "ADC", "NOP", "RRA", "NOP", "ADC", "ROR", "RRA",
			"NOP", "STA", "NOP", "SAX", "STY", "STA", "STX", "SAX",
			"DEY", "NOP", "TXA", "XAA", "STY", "STA", "STX", "SAX",
			"BCC", "STA", "KIL", "AHX", "STY", "STA", "STX", "SAX",
			"TYA", "STA", "TXS", "TAS", "SHY", "STA", "SHX", "AHX",
			"LDY", "LDA", "LDX", "LAX", "LDY", "LDA", "LDX", "LAX",
			"TAY", "LDA", "TAX", "LAX", "LDY", "LDA", "LDX", "LAX",
			"BCS", "LDA", "KIL", "LAX", "LDY", "LDA", "LDX", "LAX",
			"CLV", "LDA", "TSX", "LAS", "LDY", "LDA", "LDX", "LAX",
			"CPY", "CMP", "NOP", "DCP", "CPY", "CMP", "DEC", "DCP",
			"INY", "CMP", "DEX", "AXS", "CPY", "CMP", "DEC", "DCP",
			"BNE", "CMP", "KIL", "DCP", "NOP", "CMP", "DEC", "DCP",
			"CLD", "CMP", "NOP", "DCP", "NOP", "CMP", "DEC", "DCP",
			"CPX", "SBC", "NOP", "ISC", "CPX", "SBC", "INC", "ISC",
			"INX", "SBC", "NOP", "SBC", "CPX", "SBC", "INC", "ISC",
			"BEQ", "SBC", "KIL", "ISC", "NOP", "SBC", "INC", "ISC",
			"SED", "SBC", "NOP", "ISC", "NOP", "SBC", "INC", "ISC",
		};


		/// <summary>
		/// Instruction performed by the CPU.
		/// </summary>
		private delegate void Instruction (StepInfo step);


		/// <summary>
		/// Memory interface
		/// </summary>
		public IMemory memory;

		/// <summary>
		/// Number of cycles
		/// </summary>
		public UInt64 cycles;

		/// <summary>
		/// Program counter
		/// </summary>
		private UInt16 pc;

		/// <summary>
		/// Stack pointer
		/// </summary>
		private byte sp;

		/// <summary>
		/// Accumulator
		/// </summary>
		private byte a;

		/// <summary>
		/// X register
		/// </summary>
		private byte x;

		/// <summary>
		/// Y register
		/// </summary>
		private byte y;

		/// <summary>
		/// Carry flag
		/// </summary>
		private byte c;

		/// <summary>
		/// Zero flag
		/// </summary>
		private byte z;

		/// <summary>
		/// Interrupt disable flag
		/// </summary>
		private byte i;

		/// <summary>
		/// Decimal mode flag
		/// </summary>
		private byte d;

		/// <summary>
		/// Break command flag
		/// </summary>
		private byte b;

		/// <summary>
		/// Unused flag
		/// </summary>
		private byte u;

		/// <summary>
		/// Overflow flag
		/// </summary>
		private byte v;

		/// <summary>
		/// Negative flag
		/// </summary>
		private byte n;

		/// <summary>
		/// Interrupt type to perform
		/// </summary>
		private Interrupt interrupt;

		/// <summary>
		/// Number of cycles to stall
		/// </summary>
		public int stall;

		/// <summary>
		/// Table of instructions
		/// </summary>
		private Instruction[] table;


		/// <summary>
		/// Instance of the console
		/// </summary>
		private Console console;


		public CPU (Console console)
		{
			this.console = console;

			memory = new Memory (this.console);

			CreateTable ();
			Reset ();
		}


		/// <summary>
		/// Creates instruction table mapping instruction to method
		/// </summary>
		private void CreateTable ()
		{
			table = new Instruction[] {
				BRK, ORA, KIL, SLO, NOP, ORA, ASL, SLO,
				PHP, ORA, ASL, ANC, NOP, ORA, ASL, SLO,
				BPL, ORA, KIL, SLO, NOP, ORA, ASL, SLO,
				CLC, ORA, NOP, SLO, NOP, ORA, ASL, SLO,
				JSR, AND, KIL, RLA, BIT, AND, ROL, RLA,
				PLP, AND, ROL, ANC, BIT, AND, ROL, RLA,
				BMI, AND, KIL, RLA, NOP, AND, ROL, RLA,
				SEC, AND, NOP, RLA, NOP, AND, ROL, RLA,
				RTI, EOR, KIL, SRE, NOP, EOR, LSR, SRE,
				PHA, EOR, LSR, ALR, JMP, EOR, LSR, SRE,
				BVC, EOR, KIL, SRE, NOP, EOR, LSR, SRE,
				CLI, EOR, NOP, SRE, NOP, EOR, LSR, SRE,
				RTS, ADC, KIL, RRA, NOP, ADC, ROR, RRA,
				PLA, ADC, ROR, ARR, JMP, ADC, ROR, RRA,
				BVS, ADC, KIL, RRA, NOP, ADC, ROR, RRA,
				SEI, ADC, NOP, RRA, NOP, ADC, ROR, RRA,
				NOP, STA, NOP, SAX, STY, STA, STX, SAX,
				DEY, NOP, TXA, XAA, STY, STA, STX, SAX,
				BCC, STA, KIL, AHX, STY, STA, STX, SAX,
				TYA, STA, TXS, TAS, SHY, STA, SHX, AHX,
				LDY, LDA, LDX, LAX, LDY, LDA, LDX, LAX,
				TAY, LDA, TAX, LAX, LDY, LDA, LDX, LAX,
				BCS, LDA, KIL, LAX, LDY, LDA, LDX, LAX,
				CLV, LDA, TSX, LAS, LDY, LDA, LDX, LAX,
				CPY, CMP, NOP, DCP, CPY, CMP, DEC, DCP,
				INY, CMP, DEX, AXS, CPY, CMP, DEC, DCP,
				BNE, CMP, KIL, DCP, NOP, CMP, DEC, DCP,
				CLD, CMP, NOP, DCP, NOP, CMP, DEC, DCP,
				CPX, SBC, NOP, ISC, CPX, SBC, INC, ISC,
				INX, SBC, NOP, SBC, CPX, SBC, INC, ISC,
				BEQ, SBC, KIL, ISC, NOP, SBC, INC, ISC,
				SED, SBC, NOP, ISC, NOP, SBC, INC, ISC
			};
		}


		public void Save (BinaryWriter writer)
		{
			/*
			encoder.Encode(cpu.Cycles)
			encoder.Encode(cpu.PC)
			encoder.Encode(cpu.SP)
			encoder.Encode(cpu.A)
			encoder.Encode(cpu.X)
			encoder.Encode(cpu.Y)
			encoder.Encode(cpu.C)
			encoder.Encode(cpu.Z)
			encoder.Encode(cpu.I)
			encoder.Encode(cpu.D)
			encoder.Encode(cpu.B)
			encoder.Encode(cpu.U)
			encoder.Encode(cpu.V)
			encoder.Encode(cpu.N)
			encoder.Encode(cpu.interrupt)
			encoder.Encode(cpu.stall)
			*/
		}


		private void Load (BinaryReader reader)
		{
			/*
			decoder.Decode(&cpu.Cycles)
			decoder.Decode(&cpu.PC)
			decoder.Decode(&cpu.SP)
			decoder.Decode(&cpu.A)
			decoder.Decode(&cpu.X)
			decoder.Decode(&cpu.Y)
			decoder.Decode(&cpu.C)
			decoder.Decode(&cpu.Z)
			decoder.Decode(&cpu.I)
			decoder.Decode(&cpu.D)
			decoder.Decode(&cpu.B)
			decoder.Decode(&cpu.U)
			decoder.Decode(&cpu.V)
			decoder.Decode(&cpu.N)
			decoder.Decode(&cpu.interrupt)
			decoder.Decode(&cpu.stall)
			*/
		}


		/// <summary>
		/// Resets the state of the machine
		/// </summary>
		public void Reset ()
		{
			pc = Read16 (0xFFFC);
			sp = 0xFD;
			SetFlags (0x24);
		}


		/// <summary>
		/// Reads 16 bits from the specified address
		/// </summary>
		/// <param name="address">Address.</param>
		private UInt16 Read16 (UInt16 address)
		{
			int lo = memory.Read (address);
			int hi = memory.Read ((UInt16)(address + 1));

			return (UInt16)(hi << 8 | lo);
		}


		/// <summary>
		/// Reads 16 bits, but includes the byte of the low byte wrapping around
		/// without incrementing the high byte.
		/// </summary>
		/// <returns>The bug.</returns>
		/// <param name="address">Address.</param>
		private UInt16 Read16Bug (UInt16 address)
		{
			UInt16 a = address;
			UInt16 b = (UInt16)((a & 0xFF00) | ((a + 1) & 0xFF));

			int lo = memory.Read (a);
			int hi = memory.Read (b);

			return (UInt16)(hi << 8 | lo);
		}



		/// <summary>
		/// Returns the status processfor flags
		/// </summary>
		/// <returns>The flags.</returns>
		private byte GetFlags ()
		{
			int flags = 0;

			flags = (c << 0)
			| (z << 1)
			| (i << 2)
			| (d << 3)
			| (b << 4)
			| (u << 5)
			| (v << 6)
			| (n << 7);
			
			return (byte)flags;
		}


		/// <summary>
		/// Sets the processor status flags
		/// </summary>
		/// <param name="flags">Flags.</param>
		private void SetFlags (byte flags)
		{
			c = (byte)((flags >> 0) & 1);
			z = (byte)((flags >> 1) & 1);
			i = (byte)((flags >> 2) & 1);
			d = (byte)((flags >> 3) & 1);
			b = (byte)((flags >> 4) & 1);
			u = (byte)((flags >> 5) & 1);
			v = (byte)((flags >> 6) & 1);
			n = (byte)((flags >> 7) & 1);
		}


		/// <summary>
		/// Returns true if two addresses reference different pages
		/// </summary>
		/// <returns><c>true</c>, if pages differ, <c>false</c> otherwise.</returns>
		/// <param name="address1">Address 1.</param>
		/// <param name="address2">Address 2.</param>
		private bool PagesDiffer (UInt16 address1, UInt16 address2)
		{
			return (address1 & 0xFF00) != (address2 & 0xFF00);
		}


		/// <summary>
		/// Adds a cycle for branching and adds another cycle if the branch jumps
		/// to a new page.
		/// </summary>
		/// <param name="info">Step Info.</param>
		private void AddBranchCycles (StepInfo info)
		{
			++cycles;

			if (PagesDiffer (info.pc, info.address))
				++cycles;
		}


		/// <summary>
		/// Pushes a byte to the stack
		/// </summary>
		/// <param name="value">Value.</param>
		private void Push (byte value)
		{
			memory.Write ((UInt16)(0x100 | (UInt16)sp), value);
			--sp;
		}


		/// <summary>
		/// Pull a byte from the stack.
		/// </summary>
		private byte Pull ()
		{
			++sp;
			return memory.Read ((UInt16)(0x100 | (UInt16)sp));
		}


		/// <summary>
		/// Pushes a 16-bit value onto the stack.
		/// </summary>
		/// <param name="value">Value.</param>
		private void Push16 (UInt16 value)
		{
			byte hi = (byte)(value >> 8);
			byte lo = (byte)(value & 0xff);

			Push (hi);
			Push (lo);
		}


		/// <summary>
		/// Pulls a 16-bit value from the stack.
		/// </summary>
		private UInt16 Pull16 ()
		{
			UInt16 lo = (UInt16)Pull ();
			UInt16 hi = (UInt16)Pull ();

			return (UInt16)(hi << 8 | lo);
		}




		/// <summary>
		/// Sets the zero flag if the argument is zero
		/// </summary>
		/// <param name="value">Value.</param>
		private void SetZ (byte value)
		{
			if (value == 0)
				z = 1;
			else
				z = 0;
		}


		/// <summary>
		/// Sets the negative flag if the value is negative
		/// </summary>
		/// <param name="value">Value.</param>
		private void SetN (byte value)
		{
			if ((value & 0x80) != 0)
				n = 1;
			else
				n = 0;
		}


		/// <summary>
		/// Sets the zero and negative flag
		/// </summary>
		/// <param name="value">Value.</param>
		private void SetZN (byte value)
		{
			SetZ (value);
			SetN (value);
		}


		/// <summary>
		/// Compares two numbers. Sets the carry flag if the first value is bigger.
		/// </summary>
		/// <param name="value1">Value1.</param>
		/// <param name="value2">Value2.</param>
		private void Compare (byte value1, byte value2)
		{
			SetZN ((byte)(value1 - value2));

			if (value1 >= value2)
				c = 1;
			else
				c = 0;
		}


		/// <summary>
		/// Causes a non-maskable interrupt to occur on the next cycle
		/// </summary>
		public void TriggerNMI ()
		{
			interrupt = Interrupt.NMI;
		}


		/// <summary>
		/// Causes an IRQ interrupt to occur on the next cycle
		/// </summary>
		private void TriggerIRQ ()
		{
			if (i == 0)
				interrupt = Interrupt.IRQ;
		}


		/// <summary>
		/// Non maskable interrupt
		/// </summary>
		private void NMI ()
		{
			Push16 (pc);
			PHP (null);
			pc = Read16 (0xFFFA);

			i = 1;
			cycles += 7;
		}

		/// <summary>
		/// IRQ interrupt
		/// </summary>
		private void IRQ ()
		{
			Push16 (pc);
			PHP (null);
			pc = Read16 (0xFFFE);

			i = 1;
			cycles += 7;
		}


		public int Step ()
		{
			// If we are stalling, stall for a cycle
			if (stall > 0) {
				--stall;
				return 1;
			}

			UInt64 startCycles = cycles;

			HandleInterrupt ();

			byte opCode = memory.Read (pc);
			AddressMode mode = (AddressMode)INSTRUCTION_MODES [opCode];
			AddressInfo info = GetAddress (mode);

			pc += INSTRUCTION_SIZES [opCode];
			cycles += INSTRUCTION_CYCLES [opCode];

			if (info.pageCrossed)
				cycles += INSTRUCTION_PAGE_CYCLES [opCode];

			StepInfo step = new StepInfo {
				address = info.address,
				pc = pc,
				mode = mode
			};

			#if UNITY_EDITOR
			LogInstruction (opCode, step);
			#endif

			table [opCode] (step);

			return (int)(cycles - startCycles);
		}


		/// <summary>
		/// Handles an interrupt if present
		/// </summary>
		private void HandleInterrupt ()
		{
			if (interrupt == Interrupt.NMI)
				NMI ();
			else if (interrupt == Interrupt.IRQ)
				IRQ ();

			interrupt = Interrupt.None;
		}


		/// <summary>
		/// Gets the address given the address mode
		/// </summary>
		/// <returns>The address.</returns>
		/// <param name="mode">Mode.</param>
		private AddressInfo GetAddress (AddressMode mode)
		{
			AddressInfo result = new AddressInfo ();
			UInt16 pcNext = (UInt16)(pc + 1);

			switch (mode) {
			case AddressMode.Absolute:
				result.address = Read16 (pcNext);
				break;

			case AddressMode.AbsoluteX:
				result.address = (UInt16)(Read16 (pcNext) + (UInt16)x);
				result.pageCrossed = PagesDiffer ((UInt16)(result.address - (UInt16)x), result.address);
				break;

			case AddressMode.AbsoluteY:
				result.address = (UInt16)(Read16 (pcNext) + (UInt16)y);
				result.pageCrossed = PagesDiffer ((UInt16)(result.address - (UInt16)y), result.address);
				break;
			
			case AddressMode.Accumulator:
				result.address = 0;
				break;

			case AddressMode.Immediate:
				result.address = pcNext;
				break;

			case AddressMode.Implied:
				result.address = 0;
				break;

			case AddressMode.IndexedIndirect:
				result.address = Read16Bug ((UInt16)(memory.Read (pcNext) + x));
				break;

			case AddressMode.Indirect:
				result.address = Read16Bug (Read16 (pcNext));
				break;

			case AddressMode.IndirectIndexed:
				result.address = (UInt16)(Read16Bug ((UInt16)memory.Read (pcNext)) + (UInt16)y);
				result.pageCrossed = PagesDiffer ((UInt16)(result.address - (UInt16)y), result.address);
				break;

			case AddressMode.Relative:
				UInt16 offset = (UInt16)memory.Read (pcNext);

				if (offset < 0x80) {
					result.address = (UInt16)(pc + 2 + offset);
				} else {
					result.address = (UInt16)(pc + 2 + offset - 0x100);
				}

				break;

			case AddressMode.ZeroPage:
				result.address = (UInt16)memory.Read (pcNext);
				break;

			case AddressMode.ZeroPageX:
				result.address = (UInt16)(memory.Read (pcNext) + x);
				break;

			case AddressMode.ZeroPageY:
				result.address = (UInt16)(memory.Read (pcNext) + y);
				break;
			}

			return result;
		}


		/// <summary>
		/// Prints out the instruction
		/// </summary>
		private void LogInstruction (byte opCode, StepInfo step)
		{
			byte bytes = INSTRUCTION_SIZES [opCode];
			string name = INSTRUCITON_NAMES [opCode];

			string w0 = memory.Read ((UInt16)(step.pc - 1)).ToString ("X2");
			string w1 = memory.Read (step.pc).ToString ("X2");
			string w2 = memory.Read ((UInt16)(step.pc + 1)).ToString ("X2");

			if (bytes < 2)
				w1 = "  ";

			if (bytes < 3)
				w2 = "  ";


			string logString = string.Format ("{0}  {1} {2} {3}  {4} {5,28} A:{6} X:{7} Y:{8} P:{9} SP:{10} CYC:{11}",
				                   (step.pc - 1).ToString ("X4"), w0, w1, w2, name, "",
				                   a.ToString ("X2"), x.ToString ("X2"), y.ToString ("X2"), GetFlags ().ToString ("X2"), sp.ToString ("X2"), (cycles * 3) % 341);

			console.Log (logString);
			/*
					fmt.Printf(
						"%4X  %s %s %s  %s %28s"+
						"A:%02X X:%02X Y:%02X P:%02X SP:%02X CYC:%3d\n",
						cpu.PC, w0, w1, w2, name, "",
						cpu.A, cpu.X, cpu.Y, cpu.Flags(), cpu.SP, (cpu.Cycles*3)%341)
				}
				*/
		}


		// --------------------
		// --- INSTRUCTIONS ---
		// --------------------


		/// <summary>
		/// Add with carry
		/// </summary>
		/// <param name="step">Step.</param>
		private void ADC (StepInfo step)
		{
			byte a = this.a;
			byte b = memory.Read (step.address);
			byte c = this.c;

			int sum = a + b + c;
			this.a = (byte)sum;

			SetZN (this.a);

			if (sum > 0xFF)
				this.c = 1;
			else
				this.c = 0;

			if (((a ^ b) & 0x80) == 0 && ((a ^ this.a) & 0x80) != 0)
				v = 1;
			else
				v = 0;
		}


		/// <summary>
		/// Logical AND
		/// </summary>
		/// <param name="step">Step.</param>
		private void AND (StepInfo step)
		{
			a = (byte)(a & memory.Read (step.address));
			SetZN (a);
		}


		/// <summary>
		/// Arithmatic shift left
		/// </summary>
		/// <param name="step">Step.</param>
		private void ASL (StepInfo step)
		{
			if (step.mode == AddressMode.Accumulator) {
				c = (byte)((a >> 7) & 1);
				a = (byte)(a << 1);

				SetZN (a);
			} else {
				byte value = memory.Read (step.address);

				c = (byte)((value >> 7) & 1);
				value = (byte)(value << 1);

				memory.Write (step.address, value);
				SetZN (value);
			}
		}


		/// <summary>
		/// Branch if carry clear
		/// </summary>
		/// <param name="step">Step.</param>
		private void BCC (StepInfo step)
		{
			if (c == 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Branch if carry set
		/// </summary>
		/// <param name="step">Step.</param>
		private void BCS (StepInfo step)
		{
			if (c != 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Branch if equal
		/// </summary>
		/// <param name="step">Step.</param>
		private void BEQ (StepInfo step)
		{
			if (z != 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Bit test
		/// </summary>
		/// <param name="step">Step.</param>
		private void BIT (StepInfo step)
		{
			byte value = memory.Read (step.address);

			v = (byte)((value >> 6) & 1);

			SetZ ((byte)(value & a));
			SetN (value);
		}


		/// <summary>
		/// Branch if minus
		/// </summary>
		/// <param name="step">Step.</param>
		private void BMI (StepInfo step)
		{
			if (n != 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Branch if not equal
		/// </summary>
		/// <param name="step">Step.</param>
		private void BNE (StepInfo step)
		{
			if (z == 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Branch if positive
		/// </summary>
		/// <param name="step">Step.</param>
		private void BPL (StepInfo step)
		{
			if (n == 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Break (force interrupt)
		/// </summary>
		/// <param name="step">Step.</param>
		private void BRK (StepInfo step)
		{
			Push16 (pc);
			PHP (step);
			SEI (step);
			pc = Read16 (0xFFFE);
		}


		/// <summary>
		/// Branch if overflow clear
		/// </summary>
		/// <param name="step">Step.</param>
		private void BVC (StepInfo step)
		{
			if (v == 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Branch if overflow set
		/// </summary>
		/// <param name="step">Step.</param>
		private void BVS (StepInfo step)
		{
			if (v != 0) {
				pc = step.address;
				AddBranchCycles (step);
			}
		}


		/// <summary>
		/// Clear carry flag
		/// </summary>
		/// <param name="info">Info.</param>
		private void CLC (StepInfo info)
		{
			c = 0;
		}


		/// <summary>
		/// Clear decimal mode
		/// </summary>
		/// <param name="info">Info.</param>
		private void CLD (StepInfo info)
		{
			d = 0;
		}


		/// <summary>
		/// Clear interrupt disable
		/// </summary>
		/// <param name="info">Info.</param>
		private void CLI (StepInfo info)
		{
			i = 0;
		}


		/// <summary>
		/// Clear overflow
		/// </summary>
		/// <param name="info">Info.</param>
		private void CLV (StepInfo info)
		{
			v = 0;
		}


		/// <summary>
		/// Compare
		/// </summary>
		/// <param name="step">Step.</param>
		private void CMP (StepInfo step)
		{
			byte value = memory.Read (step.address);
			Compare (a, value);
		}


		/// <summary>
		/// Compare x
		/// </summary>
		/// <param name="step">Step.</param>
		private void CPX (StepInfo step)
		{
			byte value = memory.Read (step.address);
			Compare (x, value);
		}


		/// <summary>
		/// Compare y
		/// </summary>
		/// <param name="step">Step.</param>
		private void CPY (StepInfo step)
		{
			byte value = memory.Read (step.address);
			Compare (y, value);
		}


		/// <summary>
		/// Decrement memory
		/// </summary>
		/// <param name="step">Step.</param>
		private void DEC (StepInfo step)
		{
			byte value = (byte)(memory.Read (step.address) - 1);
			memory.Write (step.address, value);

			SetZN (value);
		}


		/// <summary>
		/// Decrement X
		/// </summary>
		/// <param name="step">Step.</param>
		private void DEX (StepInfo step)
		{
			--x;
			SetZN (x);
		}


		/// <summary>
		/// Decrement Y
		/// </summary>
		/// <param name="step">Step.</param>
		private void DEY (StepInfo step)
		{
			--y;
			SetZN (y);
		}


		/// <summary>
		/// Exclusive OR
		/// </summary>
		/// <param name="step">Step.</param>
		private void EOR (StepInfo step)
		{
			a = (byte)(a ^ memory.Read (step.address));
			SetZN (a);
		}


		/// <summary>
		/// Increment memory
		/// </summary>
		/// <param name="step">Step.</param>
		private void INC (StepInfo step)
		{
			byte value = (byte)(memory.Read (step.address) + 1);
			memory.Write (step.address, value);

			SetZN (value);
		}


		/// <summary>
		/// Increment X
		/// </summary>
		/// <param name="step">Step.</param>
		private void INX (StepInfo step)
		{
			++x;
			SetZN (x);
		}


		/// <summary>
		/// Increment Y
		/// </summary>
		/// <param name="step">Step.</param>
		private void INY (StepInfo step)
		{
			++y;
			SetZN (y);
		}


		/// <summary>
		/// Jump
		/// </summary>
		/// <param name="step">Step.</param>
		private void JMP (StepInfo step)
		{
			pc = step.address;
		}


		/// <summary>
		/// Jump to subroutine
		/// </summary>
		/// <param name="step">Step.</param>
		private void JSR (StepInfo step)
		{
			Push16 ((UInt16)(pc - 1));
			pc = step.address;
		}


		/// <summary>
		/// Load accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void LDA (StepInfo step)
		{
			a = memory.Read (step.address);
			SetZN (a);
		}


		/// <summary>
		/// Load accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void LDX (StepInfo step)
		{
			x = memory.Read (step.address);
			SetZN (x);
		}


		/// <summary>
		/// Load accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void LDY (StepInfo step)
		{
			y = memory.Read (step.address);
			SetZN (y);
		}


		/// <summary>
		/// Logical shift right
		/// </summary>
		/// <param name="step">Step.</param>
		private void LSR (StepInfo step)
		{
			if (step.mode == AddressMode.Accumulator) {
				c = (byte)(a & 1);
				a = (byte)(a >> 1);
				SetZN (a);
			} else {
				byte value = memory.Read (step.address);
				c = (byte)(value & 1);
				value = (byte)(value >> 1);

				memory.Write (step.address, value);
				SetZN (value);
			}
		}


		/// <summary>
		/// No operation
		/// </summary>
		/// <param name="step">Step.</param>
		private void NOP (StepInfo step)
		{
		}


		/// <summary>
		/// Logical inclusive OR
		/// </summary>
		/// <param name="step">Step.</param>
		private void ORA (StepInfo step)
		{
			a = (byte)(a | memory.Read (step.address));
			SetZN (a);
		}


		/// <summary>
		/// Push accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void PHA (StepInfo step)
		{
			Push (a);
		}

		/// <summary>
		/// Pushes processor status
		/// </summary>
		/// <param name="info">Info.</param>
		private void PHP (StepInfo info)
		{
			Push ((byte)(GetFlags () | 0x10));
		}


		/// <summary>
		/// Pull accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void PLA (StepInfo step)
		{
			a = Pull ();
			SetZN (a);
		}


		/// <summary>
		/// Pull processor status
		/// </summary>
		/// <param name="step">Step.</param>
		private void PLP (StepInfo step)
		{
			SetFlags ((byte)((Pull () & 0xEF) | 0x20));
		}


		/// <summary>
		/// Rotate left
		/// </summary>
		/// <param name="step">Step.</param>
		private void ROL (StepInfo step)
		{
			byte c = this.c;

			if (step.mode == AddressMode.Accumulator) {
				this.c = (byte)((a >> 7) & 1);
				a = (byte)((a << 1) | c);

				SetZN (a);
			} else {
				byte value = memory.Read (step.address);
				this.c = (byte)((value >> 7) & 1);
				value = (byte)((value << 1) | c);

				memory.Write (step.address, value);
				SetZN (value);
			}
		}


		/// <summary>
		/// Rotate right
		/// </summary>
		/// <param name="step">Step.</param>
		private void ROR (StepInfo step)
		{
			byte c = this.c;

			if (step.mode == AddressMode.Accumulator) {
				this.c = (byte)(a & 1);
				a = (byte)((a >> 1) | (c << 7));
				SetZN (a);
			} else {
				byte value = memory.Read (step.address);
				this.c = (byte)(value & 1);
				value = (byte)((value >> 1) | (c << 7));

				memory.Write (step.address, value);
				SetZN (value);
			}
		}


		/// <summary>
		/// Return from interrupt
		/// </summary>
		/// <param name="">.</param>
		private void RTI (StepInfo step)
		{
			SetFlags ((byte)((Pull () & 0xEF) | 20));
			pc = Pull16 ();
		}


		/// <summary>
		/// Return from subroutine
		/// </summary>
		/// <param name="step">Step.</param>
		private void RTS (StepInfo step)
		{
			pc = (UInt16)(Pull16 () + 1);
		}


		/// <summary>
		/// Subtract with carry flag
		/// </summary>
		/// <param name="step">Step.</param>
		private void SBC (StepInfo step)
		{
			byte a = this.a;
			byte b = memory.Read (step.address);
			byte c = this.c;

			int sum = a - b - (1 - c);
			this.a = (byte)sum;
			SetZN (this.a);

			if (sum >= 0)
				c = 1;
			else
				c = 0;

			if (((a ^ b) & 0x80) != 0 && ((a ^ this.a) & 0x80) != 0)
				v = 1;
			else
				v = 0;
		}


		/// <summary>
		/// Set carry flag
		/// </summary>
		/// <param name="step">Step.</param>
		private void SEC (StepInfo step)
		{
			c = 1;
		}


		/// <summary>
		/// Set decimal flag
		/// </summary>
		/// <param name="step">Step.</param>
		private void SED (StepInfo step)
		{
			d = 1;
		}


		/// <summary>
		/// Set interrupt disable
		/// </summary>
		/// <param name="step">Step.</param>
		private void SEI (StepInfo step)
		{
			i = 1;
		}


		/// <summary>
		/// Store accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void STA (StepInfo step)
		{
			memory.Write (step.address, a);
		}


		/// <summary>
		/// Store x register
		/// </summary>
		/// <param name="step">Step.</param>
		private void STX (StepInfo step)
		{
			memory.Write (step.address, x);
		}


		/// <summary>
		/// Store y register
		/// </summary>
		/// <param name="step">Step.</param>
		private void STY (StepInfo step)
		{
			memory.Write (step.address, y);
		}


		/// <summary>
		/// Transfer accumulator to x
		/// </summary>
		/// <param name="step">Step.</param>
		private void TAX (StepInfo step)
		{
			x = a;
			SetZN (x);
		}


		/// <summary>
		/// Transfer accumulator to y
		/// </summary>
		/// <param name="step">Step.</param>
		private void TAY (StepInfo step)
		{
			y = a;
			SetZN (x);
		}


		/// <summary>
		/// Transfer stack pointer to x
		/// </summary>
		/// <param name="step">Step.</param>
		private void TSX (StepInfo step)
		{
			x = sp;
			SetZN (x);
		}


		/// <summary>
		/// Transfer x to accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void TXA (StepInfo step)
		{
			a = x;
			SetZN (a);
		}


		/// <summary>
		/// Transfer x to stack pointer
		/// </summary>
		/// <param name="step">Step.</param>
		private void TXS (StepInfo step)
		{
			sp = x;
		}


		/// <summary>
		/// Transfer Y to accumulator
		/// </summary>
		/// <param name="step">Step.</param>
		private void TYA (StepInfo step)
		{
			a = y;
			SetZN (a);
		}


		private void AHX (StepInfo step)
		{
		}

		private void ALR (StepInfo step)
		{
		}

		private void ANC (StepInfo step)
		{
		}

		private void ARR (StepInfo step)
		{
		}

		private void AXS (StepInfo step)
		{
		}

		private void DCP (StepInfo step)
		{
		}

		private void ISC (StepInfo step)
		{
		}

		private void KIL (StepInfo step)
		{
		}

		private void LAS (StepInfo step)
		{
		}

		private void LAX (StepInfo step)
		{
		}

		private void RLA (StepInfo step)
		{
		}

		private void RRA (StepInfo step)
		{
		}

		private void SAX (StepInfo step)
		{
		}

		private void SHX (StepInfo step)
		{
		}

		private void SHY (StepInfo step)
		{
		}

		private void SLO (StepInfo step)
		{
		}

		private void SRE (StepInfo step)
		{
		}

		private void TAS (StepInfo step)
		{
		}

		private void XAA (StepInfo step)
		{
		}
	}
}
