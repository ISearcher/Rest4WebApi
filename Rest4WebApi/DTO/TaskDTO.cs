using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DTO
{
	public class TaskDto
	{
		public int Type { get; set; }

		public string TaskEntry { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public List<int> AssignedDevices { get; set; }

		public Guid Guid { get; set; }

		public int Status { get; set; }

		[Column(TypeName = "DateTime2")]
		public DateTime CreationTime { get; set; }

		[Column(TypeName = "DateTime2")]
		public DateTime? StartTime { get; set; }

		[Column(TypeName = "DateTime2")]
		public DateTime? EndTime { get; set; }
	}
}
