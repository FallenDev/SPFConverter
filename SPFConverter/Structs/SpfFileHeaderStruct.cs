namespace SPFverter.Structs;

public struct SpfFileHeader
{
    public uint Unknown1;
    public uint Unknown2;
    public uint ColorFormat;

    public static SpfFileHeader FromBinaryReaderBlock(BinaryReader br)
    {
        var gcHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof(SpfFileHeader))), GCHandleType.Pinned);
        var structure = (SpfFileHeader)(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFileHeader)) ?? throw new InvalidOperationException());
        gcHandle.Free();
        return structure;
    }
}