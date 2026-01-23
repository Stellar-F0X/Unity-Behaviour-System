using System;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable, HideInCreationMenu]
    public class EnumVariable : BlackboardVariable<Enum> { }
}