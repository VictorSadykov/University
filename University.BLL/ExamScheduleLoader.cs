using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.BLL
{
    public class ExamScheduleLoader
    {
        private IGroupRepository _groupRepo;
        private IExamRepository examRepository;
        private ITeacherRepository _teacherRepo;
    }
}
