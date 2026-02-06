using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TaskStreamer.Runtime
{
	internal class CommandProcessor
	{
		private readonly Queue<CommandChunk> _commandQueue = new Queue<CommandChunk>();
		
		
		public void Process()
		{
			
		}


		public void Push(ICommand command)
		{
			
		}
	}
}