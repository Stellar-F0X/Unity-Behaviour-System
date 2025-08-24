using System;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class EnumVariable : Variable<Enum> { }
}