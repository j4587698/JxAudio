using System.Reflection;
using FreeSql;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace JxAudio.Core;

public class Setup
{
    
    public static bool CreateTables(DbConfigOption dbConfig)
    {
        try
        {
            var types = Assembly.GetAssembly(typeof(Setup))!.GetTypes().Where(x => x.Name.EndsWith("Entity"));
            BaseEntity.Orm.CodeFirst.SyncStructure(types.ToArray());
            var filePath = Path.Combine(AppContext.BaseDirectory, "config", "settings.json");
            var jObject = File.Exists(filePath) ? JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath)) : new JObject();
            jObject["Db"] = JObject.Parse(JsonConvert.SerializeObject(dbConfig));
            File.WriteAllText(filePath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }
        catch (Exception e)
        {
            Log.ForContext<Setup>().Error("create table fail");
            return false;
        }
            
        return true;
    }
    
    public static (bool isSuccess, string msg) SetupDb(DbConfigOption dbConfig)
    {
        var localizer = Application.GetRequiredService<IStringLocalizer<Setup>>();
        if (!dbConfig.DbType.IsNullOrEmpty() && Enum.TryParse(dbConfig.DbType, true, out DataType dataType))
        {
            var isDevelopment = Application.WebHostEnvironment?.IsDevelopment() ?? true;
            string connStr = "";
            switch (dataType)
            {
                case DataType.MySql:
                    connStr =
                        $"Data Source={dbConfig.DbUrl};Port={dbConfig.DbPort};User ID={dbConfig.Username};Password={dbConfig.Password}; Initial Catalog={dbConfig.DbName};Charset=utf8; SslMode=none;Min pool size=1";
                    break;
                case DataType.SqlServer:
                    connStr =
                        $"Data Source={dbConfig.DbUrl},{dbConfig.DbPort};User Id={dbConfig.Username};Password={dbConfig.Password};Initial Catalog={dbConfig.DbName};TrustServerCertificate=true;Pooling=true;Min Pool Size=1";
                    break;
                case DataType.PostgreSQL:
                    connStr =
                        $"Host={dbConfig.DbUrl};Port={dbConfig.DbPort};Username={dbConfig.Username};Password={dbConfig.Password}; Database={dbConfig.DbName};Pooling=true;Minimum Pool Size=1";
                    break;
                case DataType.Oracle:
                    connStr =
                        $"user id={dbConfig.Username};password={dbConfig.Password}; data source=//{dbConfig.DbUrl}:{dbConfig.DbPort}/{dbConfig.DbName};Pooling=true;Min Pool Size=1";
                    break;
                case DataType.Sqlite:
                    var path = Path.GetDirectoryName(dbConfig.DbName);
                    if (!path.IsNullOrEmpty() && !Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path!);
                    }

                    connStr =
                        $"data source={(dbConfig.DbName?.EndsWith(".db") == true ? dbConfig.DbName : dbConfig.DbName + ".db")}";
                    break;
                default:
                    Log.Error(localizer["InvalidDbType"]);
                    return (false, localizer["InvalidDbType"]);
            }

            var freeSql = new FreeSqlBuilder()
                .UseAutoSyncStructure(isDevelopment)
                .UseAutoSyncStructure(Application.WebHostEnvironment!.IsDevelopment())
                .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                .UseConnectionString(dataType, connStr)
                .Build();

            if (freeSql == null)
            {
                Log.Error(localizer["InitDbFail"]);
                return (false, localizer["InitDbFail"]);
            }

            if (!freeSql.Ado.ExecuteConnectTest())
            {
                Log.Error(localizer["ConnectDbFail"]);
                freeSql.Dispose();
                return (false, localizer["ConnectDbFail"]);
            }

            freeSql.Aop.ConfigEntity += (s, e) =>
            {
                e.ModifyResult.Name = dbConfig.Prefix + e.EntityType.Name.Replace("Entity", "").ToUnderLine();
            };

            BaseEntity.Initialization(freeSql, null);

            return (true, "");
        }
        
        Log.Error(localizer["InvalidDbType"]);
        return (false, localizer["InvalidDbType"]);
    }
}