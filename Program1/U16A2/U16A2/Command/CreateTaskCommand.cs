using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using U16A2.viewModel;

namespace U16A2.Command
{
    public class CreateTaskCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TaskListViewModel taskList)
            {
                taskList.Tasks.Add(new TaskViewModel() { Name = taskList.TaskName, Complete = false });
            }
        }
    }
}
