using UnityEngine;
using System.Collections;
using System.IO;
using Mono.Data.Sqlite;

//using Mono.Data.SqliteClient;

public class Test : MonoBehaviour 
{
	
	string  name1 = null;
	string  email = null;
	string  path = null;
	string   fileE = null;

	void Start () 
	{

		
		string appDBPath = Application.dataPath  + "/xuanyusong.db";

		DbAccess db = new DbAccess(@"Data Source=" + appDBPath);
			
	
		path = appDBPath;
		
		//请注意 插入字符串是 已经要加上'宣雨松' 不然会报错
		if (File.Exists (path)) {
			fileE = "数据库存在，未创建数据库";

		} else {
			db.CreateTable ("momo", new string[]{"name","qq","email","blog"}, new string[]{"text","text","text","text"});
			fileE = "文件不存在，新建了一个数据库：" + path;
		}
			//我在数据库中连续插入三条数据
		db.InsertInto("momo", new string[]{ "'qw'","'289187120'","'xuanyusong@gmail.com'","'www.xuanyusong.com'"   });
		db.InsertInto("momo", new string[]{ "'re'","'289187120'","'000@gmail.com'","'www.xuanyusong.com'"   });
		db.InsertInto("momo", new string[]{ "'tg'","'289187120'","'111@gmail.com'","'www.xuanyusong.com'"   });
		
		//然后在删掉两条数据
//		db.Delete("momo",new string[]{"email","email"}, new string[]{"'xuanyusong@gmail.com'","'000@gmail.com'"}  );
		
		//注解1
		using (SqliteDataReader sqReader = db.SelectWhere("momo",new string[]{"name","email"},new string[]{"qq"},new string[]{"="},new string[]{"289187120"}))
		{
	
			while (sqReader.Read())  
    		{ 	
				//目前中文无法显示
     	 		Debug.Log("xuanyusong" + sqReader.GetString(sqReader.GetOrdinal("name")));
			
		
				Debug.Log("xuanyusong" + sqReader.GetString(sqReader.GetOrdinal("email")));
				
				
				name1 = sqReader.GetString(sqReader.GetOrdinal("name"));
				email = sqReader.GetString(sqReader.GetOrdinal("email"));
				
    		} 
		
			sqReader.Close();
		}
		
		db.CloseSqlConnection();
	}
	
	
	void OnGUI()
	{
		if(name1 != null)
		{
			GUILayout.Label(name1);
		}
		
		if(email != null)
		{
			GUILayout.Label(email);
		}
		
		
		if(path != null)
		{
			GUILayout.Label(path);
			GUILayout.Label(fileE);
		}
	}

}