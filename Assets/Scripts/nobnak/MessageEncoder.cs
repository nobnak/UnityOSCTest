using System.Collections.Generic;
using System.Net;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace nobnak.OSC {	
	public class MessageEncoder {
		private string _address;
		private LinkedList<IParam> _params;
		
		public MessageEncoder(string address) {
			_address = address;
			_params = new LinkedList<IParam>();
		}
		
		public void Add(int content) {
			_params.AddLast(new Int32Param(content));
		}
		public void Add(float content) {
			_params.AddLast(new Float32Param(content));
		}
		public void Add(string content) {
			_params.AddLast(new StringParam(content));
		}
		
		public byte[] Encode() {
			var lenAddress = (_address.Length + 4) & ~3;
			var lenTags = (_params.Count + 5) & ~3;
			var lenDatas = _params.Sum((p) => p.Length);
			var bytedata = new byte[lenAddress + lenTags + lenDatas];
			
			var offset = 0;
			Encoding.UTF8.GetBytes(_address, 0, _address.Length, bytedata, offset);
			offset += lenAddress;
			
			bytedata[offset] = (byte)',';
			var addOffset = 0;
			foreach (var p in _params)
				bytedata[offset + ++addOffset] = p.Tag;
			offset += lenTags;
			
			foreach (var p in _params) {
				p.Assign(bytedata, offset);
				offset += p.Length;
			}
			
			return bytedata;
		}
		
		public static byte[] ReverseBytes(byte[] inArray) {
			byte temp;
			int highCtr = inArray.Length - 1;
			
			for (int ctr = 0; ctr < inArray.Length / 2; ctr++) {
				temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
			return inArray;
		}
		
		public interface IParam {
			byte Tag { get; }
			int Length { get; }
			void Assign(byte[] output, int offset);
		}
		[StructLayout(LayoutKind.Explicit)]
		public class Int32Param : IParam {
			[FieldOffset(0)]
			private int _intdata;
			[FieldOffset(0)]
            public byte Byte0;
            [FieldOffset(1)]
            public byte Byte1;            
            [FieldOffset(2)]
            public byte Byte2;
            [FieldOffset(3)]
            public byte Byte3;
			
			public Int32Param(int intdata) {
				_intdata = intdata;
			}
			#region IParam implementation
			public byte Tag { get { return (byte)'i'; } }
			public int Length { get { return 4; } }
			public void Assign(byte[] output, int offset) {
				var inc = BitConverter.IsLittleEndian ? -1 : 1;
				var accum = BitConverter.IsLittleEndian ? 3 : 0;
				output[offset + accum] = Byte0; accum += inc;
				output[offset + accum] = Byte1; accum += inc;
				output[offset + accum] = Byte2; accum += inc;
				output[offset + accum] = Byte3; accum += inc;
			}
			#endregion
		}
		[StructLayout(LayoutKind.Explicit)]
		public class Float32Param : IParam {
			[FieldOffset(0)]
			private float _floatdata;
			[FieldOffset(0)]
            public byte Byte0;
            [FieldOffset(1)]
            public byte Byte1;            
            [FieldOffset(2)]
            public byte Byte2;
            [FieldOffset(3)]
            public byte Byte3;
			
			public Float32Param(float floatdata) {
				_floatdata = floatdata;
			}

			#region IParam implementation
			public byte Tag { get { return (byte)'f'; } }
			public int Length { get { return 4; } }
			public void Assign(byte[] output, int offset) {
				var inc = BitConverter.IsLittleEndian ? -1 : 1;
				var accum = BitConverter.IsLittleEndian ? 3 : 0;
				output[offset + accum] = Byte0; accum += inc;
				output[offset + accum] = Byte1; accum += inc;
				output[offset + accum] = Byte2; accum += inc;
				output[offset + accum] = Byte3; accum += inc;
			}
			#endregion
		}
		public class StringParam : IParam {
			private string _stringdata;
			
			public StringParam(string stringdata) {
				_stringdata = stringdata;
			}

			#region IParam implementation
			public byte Tag { get { return (byte)'s'; } }
			public int Length { get { return (Encoding.UTF8.GetByteCount(_stringdata) + 4) & ~3; } }
			public void Assign(byte[] output, int offset) {
				Encoding.UTF8.GetBytes(_stringdata, 0, _stringdata.Length, output, offset);
			}
			#endregion
		}
	}
}