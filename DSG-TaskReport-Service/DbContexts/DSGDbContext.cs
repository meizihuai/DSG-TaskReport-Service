using DSGServerCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DSG_TaskReport_Service
{
    public class DSGDbContext : DbContext
    {
       public DSGDbContext()
        {
            int? connTimeout = 900000;
            Database.SetCommandTimeout(connTimeout);
        }
        public DbSet<DeviceInfo> DeviceTable { get; set; }
        public DbSet<UserInfo> UserTable { get; set; }
        public DbSet<WhiteRadioInfo> WhiteRadioTable { get; set; }
        public DbSet<UserTaskInfo> UserTaskTable { get; set; }
        public DbSet<WarnInfo> WarnTable { get; set; }
        public DbSet<WorkLogInfo> WorkLogTable { get; set; }
        public DbSet<DeviceLogInfo> DeviceLogTable { get; set; }
        public DbSet<TaskMsgInfo> TaskMsgTable { get; set; }
        public DbSet<RemoteFreqModule> FreqModuleTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            /// 除了startup里面通过appsetting配置之外，还可以直接在此配置 如下：
            /// server = 123.207.31.37; port = 3306; database = efTest; uid = root; password = Mei19931129; sslmode = none
            string conn = Module.MysqlConnstr;
            optionsBuilder.UseMySQL(conn);//配置连接字符串 必须加sslmode=none
        }

        public int Update<TEntity>(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties) where TEntity : class
        {
            var dbEntityEntry = this.Entry(entity);
            if (updatedProperties.Any())
            {
                foreach (var property in updatedProperties)
                {
                    dbEntityEntry.Property(property).IsModified = true;
                }
            }
            else
            {
                foreach (var property in dbEntityEntry.OriginalValues.Properties)
                {
                    string pName = property.Name;
                    var original = dbEntityEntry.OriginalValues.GetValue<object>(pName);
                    var current = dbEntityEntry.CurrentValues.GetValue<object>(pName);
                    if (original != null && !original.Equals(current))
                    {
                        dbEntityEntry.Property(pName).IsModified = true;
                    }
                }
            }
            return this.SaveChanges();
        }

    }
}
