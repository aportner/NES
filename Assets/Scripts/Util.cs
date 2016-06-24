using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NES
{
	public static class Util
	{
		public static T ReadStruct<T>(this BinaryReader reader)
			where T : struct
		{
			Byte[] buffer = new Byte[Marshal.SizeOf(typeof(T))];
			reader.Read(buffer, 0, buffer.Length);
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return result;
		}
	}
}

