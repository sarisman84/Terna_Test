using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Terna.Serializer
{
	public struct SerializedMachinePart
	{
		public int partID;
	}
	public class MachineSerializer
	{

		private MachineBuilder builder;


		public MachineSerializer(MachineBuilder builder)
		{
			this.builder = builder;
		}

		/// <summary>
		/// Save the assembly to an assembly file using the JSON format
		/// </summary>
		/// <param name="path"> Save path to write the JSON data to </param>
		public void SaveAssembly(string path)
		{
			// Get the assembled machine parts and machine parts
			List<AssembledMachinePart> assembledMachineParts = builder.GetAssembledMachineParts();
			MachinePart[] machineParts = builder.GetMachineParts();

			List<SerializedMachinePart> serializedAssembledMachineParts = new List<SerializedMachinePart>();
			// Sort the assembled machine parts by parent index (order of assembly)
			List<AssembledMachinePart> sortedAssemledMachineParts = assembledMachineParts.OrderBy(part => part.parentIndex).ToList();

			// Serialize the assembled machine parts
			sortedAssemledMachineParts.ForEach(part =>
			{
				SerializedMachinePart serializedPart = new SerializedMachinePart();
				serializedPart.partID = machineParts[part.machinePartIndex].GetInstanceID();
				serializedAssembledMachineParts.Add(serializedPart);
			});

			// Write the JSON data to the file
			string data = JsonConvert.SerializeObject(serializedAssembledMachineParts);
			System.IO.File.WriteAllText(path, data);
		}

		/// <summary>
		/// Load an assembly from an assembly file using the JSON format
		/// </summary>
		/// <param name="path"> Path to read the JSON data from </param>
		public void LoadAssembly(string path)
		{
			// Read the JSON data from the file
			string data = System.IO.File.ReadAllText(path);

			// Deserialize the JSON data
			List<SerializedMachinePart> assembledMachineParts = JsonConvert.DeserializeObject<List<SerializedMachinePart>>(data);

			// Get the machine parts
			builder.StartCoroutine(builder.BuildAssemblyInCoroutine(assembledMachineParts));
		}
	}
}
