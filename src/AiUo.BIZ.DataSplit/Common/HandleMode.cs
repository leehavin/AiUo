using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiUo.BIZ.DataSplit
{
    public enum HandleMode
    {
        None = 0,
        /// <summary>
        /// 1-删除 
        ///     保留天数：MoveKeepMode + MoveKeepValue
        /// </summary>
        Delete = 1,
        /// <summary>
        /// 2-备份
        ///     保留天数：MoveKeepMode + MoveKeepValue
        ///     目标表名: MoveTableMode + MoveTableValue
        /// </summary>
        Backup = 2,
        /// <summary>
        /// 3-分表(按最大行数分)
        /// </summary>
        MaxRows = 3
    }

}
