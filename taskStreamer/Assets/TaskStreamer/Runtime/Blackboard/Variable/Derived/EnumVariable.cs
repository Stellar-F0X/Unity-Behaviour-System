using System;

namespace TaskStreamer
{
    [Serializable, Readable, HideInCreationMenu]
    public class EnumVariable : BlackboardVariable<Enum> { }
}