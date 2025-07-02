using System;
using System.Runtime.InteropServices;
using System.Text;
using Godot;

namespace BlockadeClassicPrivateServer.Net.Shared;

// has to be a resource to be transmitted via godot's signals
public sealed partial class Packet : Resource
{
	// TODO: handle optional timestamp in header properly
	// the timestamp is only missing on packet #16 (Client.DisconnectByError)
	// but.. that's never called...?

	public const byte MagicByte = 245; // 0xf5
	public const int WriteBufferSize = 1_048_576; // client can receive 1_048_576, and send only 1025
	public const int ReadBufferSize = 1_025;

	[Export] private int seekPosition = 0;
	[Export] private byte[] packetBuffer;

	[Export] private bool isWriting = false;

	public PacketName Name => (PacketName)packetBuffer[sizeof(byte)];
	public ushort Length => GetLength();
	// public int Time { get { byte[] buffer = new byte[sizeof(int)]; Buffer.BlockCopy(sendBuffer, (sizeof(byte) * 2) + sizeof(ushort), buffer, 0, buffer.Length); return BitConverter.ToInt32(buffer); } }

	// to satisfy godot
	#pragma warning disable CS8618
	[Obsolete("do not use this", error: true)] private Packet() {}
	#pragma warning restore CS8618

	private Packet(bool isForWriting)
	{
		packetBuffer = new byte[isForWriting ? WriteBufferSize : ReadBufferSize];
	}

	public override string ToString()
	{
		return $"<Packet {Name} ({Length} B)>";
	}

	public T Read<T>()
	{
		object result;

		if (typeof(T) == typeof(byte))
			result = packetBuffer[seekPosition];
		else if (typeof(T) == typeof(bool))
			result = BitConverter.ToBoolean(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(short))
			result = BitConverter.ToInt16(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(ushort))
			result = BitConverter.ToUInt16(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(int))
			result = BitConverter.ToInt32(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(uint))
			result = BitConverter.ToUInt32(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(float))
			result = BitConverter.ToSingle(packetBuffer, seekPosition);
		else if (typeof(T) == typeof(double))
			result = BitConverter.ToDouble(packetBuffer, seekPosition);
		else
			throw new NotSupportedException($"Unsupported type: {typeof(T)}");

		seekPosition += Marshal.SizeOf(typeof(T));

		if (typeof(T) != result.GetType())
			GD.PrintErr($"doesn't match: {typeof(T)} != {result.GetType()}");

		return (T)result;
	}

	public string ReadString()
	{
		int length = Read<int>();
		UTF8Encoding utf8Encoding = new();
		byte[] buffer = new byte[length];
		Buffer.BlockCopy(packetBuffer, seekPosition, buffer, 0, length);
		seekPosition += length;
		return utf8Encoding.GetString(buffer);
	}

	public Vector3I ReadVector3Byte()
	{
		return new Vector3I(Read<byte>(), Read<byte>(), Read<byte>());
	}

	// shouldn't this be a vector3i?
	public Vector3 ReadVector3Int()
	{
		return new Vector3(Read<int>(), Read<int>(), Read<int>());
	}

	public Vector3 ReadVector3Float()
	{
		return new Vector3(Read<float>(), Read<float>(), Read<float>());
	}

	// TODO: ReadStringClassic, ReadArray?

	public void WriteBytes(byte[] data)
	{
		Buffer.BlockCopy(data, 0, packetBuffer, seekPosition, data.Length);
		seekPosition += data.Length;
	}

	public void WriteBytes(byte[] data, int offset)
	{
		Buffer.BlockCopy(data, offset, packetBuffer, seekPosition, data.Length);
		seekPosition += data.Length;
	}

	public void WriteBytes(byte[] data, int offset, int count)
	{
		Buffer.BlockCopy(data, offset, packetBuffer, seekPosition, count);
		seekPosition += count;
	}

	public void Write<T>(T value)
	{
		int size = Marshal.SizeOf<T>();
		Span<byte> span = new(packetBuffer, seekPosition, size);

		if (value is byte b) packetBuffer[seekPosition] = b;
		else if (value is sbyte sb) packetBuffer[seekPosition] = (byte)sb;
		else if (value is float f) BitConverter.TryWriteBytes(span, f);
		else if (value is double d) BitConverter.TryWriteBytes(span, d);
		else if (value is int i) BitConverter.TryWriteBytes(span, i);
		else if (value is uint ui) BitConverter.TryWriteBytes(span, ui);
		else if (value is short s) BitConverter.TryWriteBytes(span, s);
		else if (value is ushort us) BitConverter.TryWriteBytes(span, us);
		else throw new InvalidOperationException($"Unsupported type: {typeof(T)}");

		seekPosition += size;
	}

	/// <summary>
	/// Null terminated
	/// </summary>
	public void WriteStringClassic(string svalue)
	{
		UTF8Encoding utf8Encoding = new();
		WriteBytes(utf8Encoding.GetBytes(svalue));
		Write<byte>(0);
	}

	/// <summary>
	/// Length (4 bytes) prefixed
	/// </summary>
	public void WriteString(string svalue)
	{
		UTF8Encoding utf8Encoding = new();
		int byteCount = utf8Encoding.GetByteCount(svalue);
		Write<int>(byteCount);
		WriteBytes(utf8Encoding.GetBytes(svalue));
	}

	public void WriteVectorByte(Vector2I vector)
	{
		Write<byte>((byte)vector.X);
		Write<byte>((byte)vector.Y);
	}

	public void WriteVectorByte(Vector3I vector)
	{
		Write<byte>((byte)vector.X);
		Write<byte>((byte)vector.Y);
		Write<byte>((byte)vector.Z);
	}

	public void WriteVectorFloat(Vector2 vector)
	{
		Write<float>(vector.X);
		Write<float>(vector.Y);
	}

	public void WriteVectorFloat(Vector3 vector)
	{
		Write<float>(vector.X);
		Write<float>(vector.Y);
		Write<float>(vector.Z);
	}

	public void WriteVectorUShort(Vector2 vector)
	{
		// 0.00390625f = 1.0f / 256.0f
		Write<ushort>((ushort)(vector.X / 0.00390625f));
		Write<ushort>((ushort)(vector.Y / 0.00390625f));
	}

	public void WriteVectorUShort(Vector3 vector)
	{
		Write<ushort>((ushort)(vector.X / 0.00390625f));
		Write<ushort>((ushort)(vector.Y / 0.00390625f));
		Write<ushort>((ushort)(vector.Z / 0.00390625f));
	}

	public byte[] Build()
	{
		isWriting = false;
		var num = seekPosition;
		seekPosition = 2;
		Write<short>((short)num);
		seekPosition = num;
		return packetBuffer[..num];
	}

	private ushort GetLength()
	{
		if (isWriting)
			return (ushort)seekPosition;

		return BitConverter.ToUInt16(packetBuffer, sizeof(byte) * 2);
	}

	public static Packet Create(PacketName packetName)
	{
		Packet instance = new(true);

		// header
		instance.Write<byte>(MagicByte);
		instance.Write<byte>((byte)packetName);
		instance.Write<ushort>(0); // 2 bytes for length
		// instance.Write<int>((int)Godot.Time.GetTicksMsec()); // time in msec since server started

		return instance;
	}

	public static Packet Parse(StreamPeerTcp peer)
	{
		// assuming magic byte is already consumed
		byte cmd = peer.GetU8();

		// it starts counting from (and including) magic byte
		ushort length = (ushort)(peer.GetU16() - (sizeof(byte) * 2) - sizeof(ushort));

		// int time = peer.Get32(); // in ms since client game start

		byte[] arr = new byte[ReadBufferSize - (sizeof(byte) * 2) - sizeof(ushort)];
		for (var i = 0; i < length; i++)
			arr[i] = peer.GetU8();

		return Parse(cmd, length, arr);
	}
	public static Packet Parse(byte packetName, ushort length, byte[] arr)
	{
		Packet instance = new(false);

		// header
		instance.Write<byte>(MagicByte);
		instance.Write<byte>(packetName);
		instance.Write<ushort>(length);
		// instance.Write<int>(time);

		// data
		// TODO: handle overflows
		instance.WriteBytes(arr);

		// seek back past header
		instance.seekPosition = (sizeof(byte) * 2) + sizeof(ushort);

		return instance;
	}
}
