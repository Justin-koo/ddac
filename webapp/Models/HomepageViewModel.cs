using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
	public class StateViewModel
	{
		public string State { get; set; }
		public int PropertyCount { get; set; }
	}
	public class HomepageViewModel
	{
		public List<PropertyViewModel> HomepagePropertyViewModel { get; set; }
		public List<StateViewModel> States { get; set; }
		public List<AgentViewModel> Agents { get; set; }
	}
}
