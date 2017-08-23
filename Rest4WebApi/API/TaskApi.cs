using System.Collections.Generic;
using Common.DTO;
using WebApiClient.Controllers.Exceptions;

namespace WebApiClient.Controllers
{
	public class TaskApi : BaseWebApi<TaskDto>
	{
		public TaskApi(string uri)
			: base(uri, "tasks")
		{ }

		public bool CreateTask(TaskDto newTask)
		{
			try
			{
				var response = Create(newTask);
				return response.IsSuccessStatusCode;
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return false;
		}

		public IList<TaskDto> GetAllTasks()
		{
			try
			{
				var tasks = Get<IList<TaskDto>>();
				return tasks ?? new List<TaskDto>();
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return new List<TaskDto>();
		}

		public IEnumerable<TaskDto> GetTasks(string deviceUniqueId)
		{
			try
			{
				var tasks = GetByParam<IEnumerable<TaskDto>>(deviceUniqueId);
				return tasks ?? new List<TaskDto>();
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return new List<TaskDto>();
		}

		public void UpdateTask(string deviceUniqueId, TaskDto updatedTask)
		{
			try
			{
				Update(updatedTask, deviceUniqueId);
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}
		}

		public bool DeleteTasks(IList<TaskDto> tasks)
		{
			try
			{
				var response = DoDelete(tasks);
				return response.IsSuccessStatusCode;
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return false;
		}
	}
}
