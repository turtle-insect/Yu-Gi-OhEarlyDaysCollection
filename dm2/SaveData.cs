﻿namespace dm2
{
    class SaveData
    {
		private static SaveData mThis = new SaveData();
		private String mFileName = String.Empty;
		private Byte[]? mBuffer = null;
		private readonly System.Text.Encoding mEncode = System.Text.Encoding.UTF8;
		public uint Adventure { private get; set; } = 0;

		private SaveData()
		{ }

		public static SaveData Instance()
		{
			return mThis;
		}

		public bool Open(String filename, bool force)
		{
			if (System.IO.File.Exists(filename) == false) return false;
			mBuffer = System.IO.File.ReadAllBytes(filename);

			Adventure = uint.MaxValue;
			int[] offsets = [0, 16];
			foreach (var offset in offsets)
			{
				if (mBuffer.Length <= 0x17F6 + offset + 6) continue;

				// KONAMI check.
				string co = System.Text.Encoding.UTF8.GetString(mBuffer, 0x17F6 + offset, 6);
				if(co == "KONAMI")
				{
					Adventure = (uint)offset;
					break;
				}
			}
			if (Adventure == uint.MaxValue)
			{
				mBuffer = null;
				return false;
			}

			if (force == false)
			{
				var sum = CalcCheckSum(0);
				if (sum != ReadNumber(0x0BF6, 2))
				{
					mBuffer = null;
					return false;
				}
			}

			mFileName = filename;
			Backup();
			return true;
		}

		public bool Save()
		{
			if (String.IsNullOrEmpty(mFileName) || mBuffer == null) return false;

			var sum = CalcCheckSum(0);
			WriteNumber(0x0BF6, 2, sum);
			WriteNumber(0x17EE, 2, sum);
			System.IO.File.WriteAllBytes(mFileName, mBuffer);
			return true;
		}

		public bool SaveAs(String filename)
		{
			if (String.IsNullOrEmpty(mFileName) || mBuffer == null) return false;
			mFileName = filename;
			return Save();
		}

		public void Import(String filename)
		{
			if (String.IsNullOrEmpty(mFileName)) return;

			mBuffer = System.IO.File.ReadAllBytes(filename);
		}

		public void Export(String filename)
		{
			if (mBuffer == null) return;
			System.IO.File.WriteAllBytes(filename, mBuffer);
		}

		public uint ReadNumber(uint address, uint size)
		{
			if (mBuffer == null) return 0;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return 0;
			uint result = 0;
			for (int i = 0; i < size; i++)
			{
				result += (uint)mBuffer[address + i] << (i * 8);
			}
			return result;
		}

		public Byte[] ReadValue(uint address, uint size)
		{
			Byte[] result = new Byte[size];
			if (mBuffer == null) return result;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return result;
			Array.Copy(mBuffer, address, result, 0, size);
			return result;
		}

		// 0 to 7.
		public bool ReadBit(uint address, uint bit)
		{
			if (bit > 7) return false;
			if (mBuffer == null) return false;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return false;
			Byte mask = (Byte)(1 << (int)bit);
			return (mBuffer[address] & mask) != 0;
		}

		public String ReadText(uint address, uint size)
		{
			if (mBuffer == null) return "";
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return "";

			Byte[] tmp = new Byte[size];
			for (uint i = 0; i < size; i++)
			{
				if (mBuffer[address + i] == 0) break;
				tmp[i] = mBuffer[address + i];
			}
			return mEncode.GetString(tmp).Trim('\0');
		}

		public void WriteNumber(uint address, uint size, uint value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = (Byte)(value & 0xFF);
				value >>= 8;
			}
		}

		// 0 to 7.
		public void WriteBit(uint address, uint bit, bool value)
		{
			if (bit > 7) return;
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return;
			Byte mask = (Byte)(1 << (int)bit);
			if (value) mBuffer[address] = (Byte)(mBuffer[address] | mask);
			else mBuffer[address] = (Byte)(mBuffer[address] & ~mask);
		}

		public void WriteText(uint address, uint size, String value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			Byte[] tmp = mEncode.GetBytes(value);
			Array.Resize(ref tmp, (int)size);
			Array.Copy(tmp, 0, mBuffer, address, size);
		}

		public void WriteValue(uint address, Byte[] buffer)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + buffer.Length >= mBuffer.Length) return;
			Array.Copy(buffer, 0, mBuffer, address, buffer.Length);
		}

		public void Fill(uint address, uint size, Byte number)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = number;
			}
		}

		public void Copy(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[to + i] = mBuffer[from + i];
			}
		}

		public void Swap(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				Byte tmp = mBuffer[to + i];
				mBuffer[to + i] = mBuffer[from + i];
				mBuffer[from + i] = tmp;
			}
		}

		public List<uint> FindAddress(String name, uint index)
		{
			List<uint> result = new List<uint>();
			if (mBuffer == null) return result;
			for (; index < mBuffer.Length; index++)
			{
				if (mBuffer[index] != name[0]) continue;

				int len = 1;
				for (; len < name.Length; len++)
				{
					if (mBuffer[index + len] != name[len]) break;
				}
				if (len >= name.Length) result.Add(index);
				index += (uint)len;
			}
			return result;
		}

		private uint CalcAddress(uint address)
		{
			return address + Adventure;
		}

		private uint CalcCheckSum(uint offset)
		{
			// ROM4:4B59
			uint sum = 0;
			for (uint i = 0; i < 0x0BF6; i++)
			{
				sum += ReadNumber(i + offset, 1);

				// dm1
				// ROM4:4586
				// sum += ReadNumber(i + offset, 1);
				// sum += (sum & 0xFF) << 8;
			}

			return sum & 0xFFFF;
		}

		private void Backup()
		{
			DateTime now = DateTime.Now;
			String path = System.IO.Directory.GetCurrentDirectory();
			path = System.IO.Path.Combine(path, "backup");
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}
			path = System.IO.Path.Combine(path, $"{now:yyyy-MM-dd HH-mm-ss} {System.IO.Path.GetFileName(mFileName)}");
			System.IO.File.Copy(mFileName, path, true);
		}
	}
}
