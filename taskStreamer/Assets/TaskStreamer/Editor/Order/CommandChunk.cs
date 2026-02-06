using System.Collections.Generic;

namespace TaskStreamer.Runtime
{
	internal struct CommandChunk
	{
		public int groupId;
		public Queue<ICommand> commands;
	}
}